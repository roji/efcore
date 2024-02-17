// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// TODO: Do a proper service interface for this
public class PrecompiledQueryRewriter : CSharpSyntaxRewriter
{
    private Compilation? _compilation;

#pragma warning disable CS8618 // Uninitialized non-nullable fields. We check _compilation to make sure LoadCompilation was invoked.
    private ITypeSymbol _genericIQueryableSymbol, _nonGenericIQueryableSymbol, _dbSetSymbol;
    private ITypeSymbol _enumerableSymbol, _queryableSymbol, _efQueryableExtensionsSymbol, _efRelationalQueryableExtensionsSymbol;
    private ITypeSymbol _cancellationTokenSymbol;
#pragma warning restore CS8618

    private SemanticModel? _currentSemanticModel;

    private bool _inEfLinqQuery;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void LoadCompilation(Compilation compilation)
    {
        _compilation = compilation;

        _genericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable`1");
        _nonGenericIQueryableSymbol = GetTypeSymbolOrThrow("System.Linq.IQueryable");
        _dbSetSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.DbSet`1");

        _enumerableSymbol = GetTypeSymbolOrThrow("System.Linq.Enumerable");
        _queryableSymbol = GetTypeSymbolOrThrow("System.Linq.Queryable");
        _efQueryableExtensionsSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions");
        _efRelationalQueryableExtensionsSymbol = GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");
        _cancellationTokenSymbol = GetTypeSymbolOrThrow("System.Threading.CancellationToken");

        ITypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
                ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }

    public SyntaxTree RewriteQueries(Compilation compilation, SyntaxTree syntaxTree)
    {
        _compilation = compilation;

        var oldRoot = syntaxTree.GetRoot();
        var newRoot = Visit(oldRoot);

        // Note that we rewrite the syntax tree for async methods, since SingleAsync inserts a sync Single node into
        // the tree, not SingleAsync.
        if (!ReferenceEquals(newRoot, oldRoot))
        {
            syntaxTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);
        }

        return syntaxTree;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        var setAnnotation = false;
        if (invocation.HasAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind))
        {
            // We've reached the top-most node of an EF LINQ query, as identifier by QueryLocator.
            // Mark this in our state and visit recursively, rewriting nodes as needed.
            Check.DebugAssert(!_inEfLinqQuery, "Nested EfQueryCandidateAnnotationKind");
            _inEfLinqQuery = true;
            setAnnotation = true;
        }

        var result = base.VisitInvocationExpression(invocation);

        if (result is not InvocationExpressionSyntax visitedInvocation)
        {
            throw new InvalidOperationException("Visitation returned non-invocation");
        }

        if (_inEfLinqQuery)
        {
            result = RewriteInvocationExpression(visitedInvocation);
        }

        if (setAnnotation)
        {
            _inEfLinqQuery = false;
        }

        return result;
    }

    /// <summary>
    ///     Optionally rewrites a method invocation, replacing a node as it was in user code with the corresponding node that will go
    ///     into EF's query compilation pipeline.
    /// </summary>
    protected virtual ExpressionSyntax RewriteInvocationExpression(InvocationExpressionSyntax invocation)
    {
        // TODO: Support non-extension invocation syntax: var blogs = ToList(ctx.Blogs);

        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text : var identifier },
                Expression: var innerExpression
            } memberAccess)
        {
            return invocation;
        }

        // First, pattern-match on the method name as a string; this avoids accessing the semantic model for each and
        // every invocation (more efficient).
        //
        // Some terminating operators need to go into the query tree (Single), others not (ToList).
        // Note that checking whether the method's parameter is an IQueryable or not isn't sufficient (e.g.
        // ToListAsync accepts an IQueryable parameter but should not be part of the query tree).
        return identifier switch
        {
            // Sync ToList, ToArray and AsEnumerable are defined over IEnumerable, and don't inject a node into the query tree.
            // Simply remove them.
            nameof(Enumerable.ToList)
                or nameof(Enumerable.ToArray)
                or nameof(Enumerable.AsEnumerable)
                // when IsOnEnumerable() && IsQueryable(innerExpression)
                => innerExpression
                    .WithoutAnnotations() // TODO: Shouldn't be necessary but why not
                    .WithAdditionalAnnotations(invocation.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind)),
            // .WithAdditionalAnnotations(new SyntaxAnnotation(IQueryLocator.EfQueryCandidateAnnotationKind, "Sync")),

            nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToArrayAsync)
                or nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                // when IsOnEfQueryableExtensions()
                // when TryRewriteInvocationToSync(out var rewrittenSyncInvocation)
                => innerExpression
                    .WithoutAnnotations()
                    .WithAdditionalAnnotations(invocation.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind)),

            nameof(EntityFrameworkQueryableExtensions.AllAsync)
                or nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                or nameof(EntityFrameworkQueryableExtensions.AverageAsync)
                or nameof(EntityFrameworkQueryableExtensions.ContainsAsync)
                or nameof(EntityFrameworkQueryableExtensions.CountAsync)
                // or nameof(EntityFrameworkQueryableExtensions.DefaultIfEmptyAsync)
                or nameof(EntityFrameworkQueryableExtensions.ElementAtAsync)
                or nameof(EntityFrameworkQueryableExtensions.ElementAtOrDefaultAsync)
                or nameof(EntityFrameworkQueryableExtensions.FirstAsync)
                or nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync)
                or nameof(EntityFrameworkQueryableExtensions.LastAsync)
                or nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync)
                or nameof(EntityFrameworkQueryableExtensions.LongCountAsync)
                or nameof(EntityFrameworkQueryableExtensions.MaxAsync)
                // or nameof(EntityFrameworkQueryableExtensions.MaxByAsync)
                or nameof(EntityFrameworkQueryableExtensions.MinAsync)
                // or nameof(EntityFrameworkQueryableExtensions.MinByAsync)
                or nameof(EntityFrameworkQueryableExtensions.SingleAsync)
                or nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync)
                or nameof(EntityFrameworkQueryableExtensions.SumAsync)
                // when IsOnEfQueryableExtensions()
                => TryRewriteInvocationToSync(out var rewrittenInvocation)
                    ? rewrittenInvocation
                        .WithoutAnnotations() // TODO: Shouldn't be necessary but why not
                        .WithAdditionalAnnotations(invocation.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind))
                    : throw new InvalidOperationException(), // TODO: Log warning instead

            nameof(RelationalQueryableExtensions.ExecuteDeleteAsync) or nameof(RelationalQueryableExtensions.ExecuteUpdateAsync)
                // when IsOnEfRelationalQueryableExtensions()
                // TODO: Why Try? Maybe just throw from inside?
                => TryRewriteInvocationToSync(out var rewrittenInvocation)
                    ? rewrittenInvocation
                        .WithoutAnnotations() // TODO: Shouldn't be necessary but why not
                        .WithAdditionalAnnotations(invocation.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind))
                    : throw new InvalidOperationException(), // TODO: Log warning instead

            // nameof(RelationalQueryableExtensions.FromSql) or nameof(RelationalQueryableExtensions.FromSqlRaw)
            //     or nameof(RelationalQueryableExtensions.FromSqlInterpolated)
            //     // when IsOnEfRelationalQueryableExtensions()
            //     => RewriteFromSql(invocation),

            _ => invocation
        };

        // bool IsOnEnumerable()
        //     => IsOnTypeSymbol(_enumerableSymbol);
        //
        // bool IsOnQueryable()
        //     => IsOnTypeSymbol(_queryableSymbol);
        //
        // bool IsOnEfQueryableExtensions()
        //     => IsOnTypeSymbol(_efQueryableExtensionsSymbol);
        //
        // bool IsOnEfRelationalQueryableExtensions()
        //     => IsOnTypeSymbol(_efRelationalQueryableExtensionsSymbol);
        //
        // bool IsOnTypeSymbol(ITypeSymbol typeSymbol)
        // {
        //     if (GetSymbol(invocation) is not IMethodSymbol methodSymbol)
        //     {
        //         Console.WriteLine("Couldn't get method symbol for invocation: " + invocation);
        //         return false;
        //     }
        //
        //     return SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, typeSymbol);
        // }

        bool TryRewriteInvocationToSync([NotNullWhen(true)] out InvocationExpressionSyntax? syncInvocation)
        {
            // Chop off the Async suffix
            Debug.Assert(identifier.EndsWith("Async", StringComparison.Ordinal));
            var syncMethodName = identifier.Substring(0, identifier.Length - "Async".Length);

            // If the last argument is a cancellation token, chop it off
            var arguments = invocation.ArgumentList.Arguments;
            if (GetSymbol(invocation) is not IMethodSymbol methodSymbol)
            {
                syncInvocation = null;
                return false;
            }

            if (SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[^1].Type, _cancellationTokenSymbol)
                && invocation.ArgumentList.Arguments.Count == methodSymbol.Parameters.Length)
            {
                arguments = arguments.RemoveAt(arguments.Count - 1);
            }

            syncInvocation = invocation.Update(
                memberAccess.Update(
                    memberAccess.Expression,
                    memberAccess.OperatorToken,
                    SyntaxFactory.IdentifierName(syncMethodName)),
                invocation.ArgumentList.WithArguments(arguments));

            return true;
        }
    }

    // TODO: Duplication with QueryLocator
    private bool IsQueryable(ExpressionSyntax expression)
    {
        var symbol = GetSymbol(expression);
        switch (symbol)
        {
            case IMethodSymbol methodSymbol:
                return SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType.OriginalDefinition, _genericIQueryableSymbol) ||
                    methodSymbol.ReturnType.OriginalDefinition.AllInterfaces.Contains(_nonGenericIQueryableSymbol, SymbolEqualityComparer.Default);

            case IPropertySymbol propertySymbol:
                return IsDbSet(propertySymbol.Type);

            // TODO: Other cases of DbSet, e.g. field, local variable...

            case null:
                Console.WriteLine("Could not resolve symbol for query: " + expression);
                return false;

            default:
                // Console.WriteLine($"Unexpected symbol type '{symbol.GetType().Name}' for symbol '{symbol}' for query: " + expression);
                return false;
        }
    }

    private ISymbol? GetSymbol(SyntaxNode node)
    {
        if (_currentSemanticModel?.SyntaxTree != node.SyntaxTree)
        {
            _currentSemanticModel = _compilation!.GetSemanticModel(node.SyntaxTree);
        }

        var symbol = _currentSemanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is null)
        {
            Console.WriteLine("Could not find symbol for: " + node);
        }

        return symbol;
    }

    // TODO: Handle DbSet subclasses which aren't InternalDbSet?
    private bool IsDbSet(ITypeSymbol typeSymbol)
        => SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _dbSetSymbol);
}
