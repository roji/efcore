// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class PrecompiledQueryRewriter : CSharpSyntaxRewriter
{
    public SyntaxTree RewriteQueries(SyntaxTree syntaxTree)
    {
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
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax invocation)
    {
        if (invocation.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind).SingleOrDefault() is not { } efQueryAnnotation)
        {
            return base.VisitInvocationExpression(invocation);
        }

        // TODO: Support non-extension invocation syntax: var blogs = ToList(ctx.Blogs);
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.Text : var identifier },
                Expression: var innerExpression
            })
        {
            return base.VisitInvocationExpression(invocation);
        }

        // First, pattern-match on the method name as a string; this avoids accessing the semantic model for each and
        // every invocation (more efficient).
        //
        // Some terminating operators need to go into the query tree (Single), others not (ToList).
        // Note that checking whether the method's parameter is an IQueryable or not isn't sufficient (e.g.
        // ToListAsync accepts an IQueryable parameter but should not be part of the query tree).
        switch (identifier)
        {
            // Sync ToList, ToArray and AsEnumerable exist over IEnumerable only, so verify the actual argument is an
            // IQueryable (otherwise this is just LINQ to Objects)
            // Also, the terminating operator in these cases should not be part of the expression tree.
            case nameof(Enumerable.ToList):
            case nameof(Enumerable.ToArray):
            case nameof(Enumerable.AsEnumerable):
                return innerExpression.WithAdditionalAnnotations(efQueryAnnotation);

            case nameof(EntityFrameworkQueryableExtensions.ToListAsync):
            case nameof(EntityFrameworkQueryableExtensions.ToArrayAsync):
            case nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable):
                throw new NotImplementedException("Need to rewrite to sync as below");
            // return IsOnEfQueryableExtensions()
            //     ? CheckAndAddQuery(invocation, async: true)
            //     : invocation;

            case nameof(Queryable.All):
            case nameof(Queryable.Any):
            case nameof(Queryable.Average):
            case nameof(Queryable.Contains):
            case nameof(Queryable.Count):
            case nameof(Queryable.DefaultIfEmpty):
            case nameof(Queryable.ElementAt):
            case nameof(Queryable.ElementAtOrDefault):
            case nameof(Queryable.First):
            case nameof(Queryable.FirstOrDefault):
            case nameof(Queryable.Last):
            case nameof(Queryable.LastOrDefault):
            case nameof(Queryable.LongCount):
            case nameof(Queryable.Max):
            case nameof(Queryable.MaxBy):
            case nameof(Queryable.Min):
            case nameof(Queryable.MinBy):
            case nameof(Queryable.Single):
            case nameof(Queryable.SingleOrDefault):
            case nameof(Queryable.Sum):
                throw new NotImplementedException();
            // return IsOnQueryable()
            //     ? CheckAndAddQuery(invocation, async: false)
            //     : invocation;

            case nameof(EntityFrameworkQueryableExtensions.AllAsync):
            case nameof(EntityFrameworkQueryableExtensions.AnyAsync):
            case nameof(EntityFrameworkQueryableExtensions.AverageAsync):
            case nameof(EntityFrameworkQueryableExtensions.ContainsAsync):
            case nameof(EntityFrameworkQueryableExtensions.CountAsync):
            // case nameof(EntityFrameworkQueryableExtensions.DefaultIfEmptyAsync):
            case nameof(EntityFrameworkQueryableExtensions.ElementAtAsync):
            case nameof(EntityFrameworkQueryableExtensions.ElementAtOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.FirstAsync):
            case nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.LastAsync):
            case nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.LongCountAsync):
            case nameof(EntityFrameworkQueryableExtensions.MaxAsync):
            // case nameof(EntityFrameworkQueryableExtensions.MaxByAsync):
            case nameof(EntityFrameworkQueryableExtensions.MinAsync):
            // case nameof(EntityFrameworkQueryableExtensions.MinByAsync):
            case nameof(EntityFrameworkQueryableExtensions.SingleAsync):
            case nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync):
            case nameof(EntityFrameworkQueryableExtensions.SumAsync):
            {
                throw new NotImplementedException();
                // return IsOnEfQueryableExtensions() && TryRewriteInvocationToSync(out var rewrittenSyncInvocation)
                //     ? CheckAndAddQuery(rewrittenSyncInvocation, async: true)
                //     : invocation;
            }

            case nameof(RelationalQueryableExtensions.ExecuteDelete):
            case nameof(RelationalQueryableExtensions.ExecuteUpdate):
                throw new NotImplementedException();
            // return IsOnEfRelationalQueryableExtensions()
            //     ? CheckAndAddQuery(invocation, async: false)
            //     : invocation;

            case nameof(RelationalQueryableExtensions.ExecuteDeleteAsync):
            case nameof(RelationalQueryableExtensions.ExecuteUpdateAsync):
            {
                throw new NotImplementedException();
                // return IsOnEfRelationalQueryableExtensions() && TryRewriteInvocationToSync(out var rewrittenSyncInvocation)
                //     ? CheckAndAddQuery(rewrittenSyncInvocation, async: true)
                //     : invocation;
            }

            default:
                return base.VisitInvocationExpression(invocation)!;
        }
    }
}
