// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.EntityFrameworkCore.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CS8602 // TODO

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// TODO: Should extend ILanguageBasedService, go through IQueryCodeGeneratorSelector

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PrecompiledQueryCodeGenerator : IPrecompiledQueryCodeGenerator
{
    private readonly IQueryLocator _queryLocator;
    private readonly PrecompiledQueryRewriter _queryRewriter;
    private readonly ICSharpToLinqTranslator _csharpToLinqTranslator;

    private SyntaxGenerator _g = null!;
    private INamedTypeSymbol _linqGenericLambdaType = null!;
    private DbContext _dbContext = null!;
    private IQueryCompiler _queryCompiler = null!;
    private ExpressionTreeFuncletizer _funcletizer = null!;
    private LinqToCSharpSyntaxTranslator _linqToCSharpTranslator = null!;
    private LiftableConstantProcessor _liftableConstantProcessor = null!;
    private HashSet<string> _namespaces = new();
    private ExpressionPrinter _sqlExpressionPrinter = null!;
    private StringBuilder _stringBuilder = new();

    private INamedTypeSymbol _genericEnumerableSymbol = null!;

    private static readonly ShaperPublicMethodVerifier ShaperPublicMethodVerifier = new();

    private const string InterceptorsNamespace = "Microsoft.EntityFrameworkCore.GeneratedInterceptors";
    private const string OutputFileName = "EFPrecompiledQueryBootstrapper.cs";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PrecompiledQueryCodeGenerator(IQueryLocator queryLocator, ICSharpToLinqTranslator csharpToLinqTranslator)
    {
        _queryLocator = queryLocator;
        // TODO: Inject as a proper service
        _queryRewriter = new();
        _csharpToLinqTranslator = csharpToLinqTranslator;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async Task GeneratePrecompiledQueries(
        string projectFilePath,
        DbContext dbContext,
        string outputDir,
        CancellationToken cancellationToken = default)
    {
        // https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3
        MSBuildLocator.RegisterDefaults();

        Console.Error.WriteLine("Loading project...");
        using var workspace = MSBuildWorkspace.Create();

        var project = await workspace.OpenProjectAsync(projectFilePath, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Console.WriteLine("Compiling project...");
        var compilation = await project.GetCompilationAsync(cancellationToken)
            .ConfigureAwait(false);

        var errorDiagnostics = compilation.GetDiagnostics(cancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errorDiagnostics.Any())
        {
            Console.Error.WriteLine("Compilation failed with errors:");
            Console.Error.WriteLine();
            foreach (var diagnostic in errorDiagnostics)
            {
                Console.WriteLine(diagnostic);
            }

            Environment.Exit(1);
        }

        Console.WriteLine($"Compiled assembly {compilation.Assembly.Name}");

        var syntaxGenerator = SyntaxGenerator.GetGenerator(project);

        var generatedSyntaxTrees = await GeneratePrecompiledQueries(compilation, syntaxGenerator, dbContext, cancellationToken)
            .ConfigureAwait(false);

        foreach (var generatedSyntaxTree in generatedSyntaxTrees)
        {
            // var document = project.AddDocument(OutputFileName, bootstrapperSyntaxRoot);

            var generatedSource = (await generatedSyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false))
                .ToFullString();
            // var outputFilePath = Path.Combine(outputDir, OutputFileName);
            // File.WriteAllText(outputFilePath, bootstrapperText);

            var document = project.AddDocument(OutputFileName, generatedSource);

            // document = await ImportAdder.AddImportsAsync(document, options: null, cancellationToken).ConfigureAwait(false);
            // document = await ImportAdder.AddImportsFromSymbolAnnotationAsync(
            //     document, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // document = await ImportAdder.AddImportsAsync(document, options: null, cancellationToken).ConfigureAwait(false);

            // Run the simplifier to e.g. get rid of unneeded parentheses
            var syntaxRootFoo = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
            var annotatedDocument = document.WithSyntaxRoot(syntaxRootFoo.WithAdditionalAnnotations(Simplifier.Annotation));
            document = await Simplifier.ReduceAsync(annotatedDocument, optionSet: null, cancellationToken).ConfigureAwait(false);

            // format any node with explicit formatter annotation
            // document = await Formatter.FormatAsync(document, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any elastic whitespace
            // document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            document = await Formatter.FormatAsync(document, options: null, cancellationToken).ConfigureAwait(false);

            // document = await CaseCorrector.CaseCorrectAsync(document, CaseCorrector.Annotation, cancellationToken).ConfigureAwait(false);


            var outputFilePath = Path.Combine(outputDir, OutputFileName);
            var finalSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var finalText = await finalSyntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);
            File.WriteAllText(outputFilePath, finalText.ToString());

            // TODO: This is nicer - it adds the file to the project, but also adds a <Compile> node in the csproj for some reason.
            // var applied = workspace.TryApplyChanges(document.Project.Solution);
            // if (!applied)
            // {
            //     Console.WriteLine("Failed to apply changes to project");
            // }
        }

        // Console.WriteLine($"Query precompilation complete, processed {queriesPrecompiled} queries.");
        Console.WriteLine("Query precompilation complete.");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async Task<IReadOnlyList<SyntaxTree>> GeneratePrecompiledQueries(
        Compilation compilation,
        SyntaxGenerator syntaxGenerator,
        DbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        // TODO: check reference to EF, bail early if not found?
        _queryLocator.LoadCompilation(compilation);
        _queryRewriter.LoadCompilation(compilation);

        _genericEnumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")
            ?? throw new InvalidOperationException("Couldn't found type symbol for IEnumerable<T> in the compilation");

        _g = syntaxGenerator;
        _linqToCSharpTranslator = new LinqToCSharpSyntaxTranslator(_g);
        _liftableConstantProcessor = new LiftableConstantProcessor(null!);
        _dbContext = dbContext;
        _queryCompiler = dbContext.GetService<IQueryCompiler>();
        _sqlExpressionPrinter = new ExpressionPrinter();
        _funcletizer = new ExpressionTreeFuncletizer(
            dbContext.Model,
            dbContext.GetService<IEvaluatableExpressionFilter>(),
            dbContext.GetType(),
            generateContextAccessors: false,
            dbContext.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>());

        _linqGenericLambdaType = compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1")
            ?? throw new InvalidOperationException("System.Linq.Expressions.Expression`1 not found");

        var variableNames = new HashSet<string> { "model", "dbSetToQueryRootReplacer" };

        var generatedSyntaxTrees = new List<SyntaxTree>();

        // TODO: Ignore our auto-generated code! Also compiled model...
        var syntaxTreePairs = new List<(SyntaxTree, SyntaxTree)>();
        var rewrittenCompilation = compilation;
        foreach (var syntaxTree in compilation.SyntaxTrees
                     .Where(t => t.FilePath.Split(Path.DirectorySeparatorChar)[^1] != OutputFileName))
        {
            var annotatedSyntaxTree = _queryLocator.LocateQueries(syntaxTree);

            if (ReferenceEquals(annotatedSyntaxTree, syntaxTree))
            {
                // If the tree hasn't changed, that means no queries were located in it (the locator adds annotations to terminating
                // operator nodes, which changes the treE).
                continue;
            }

            compilation = compilation.ReplaceSyntaxTree(syntaxTree, annotatedSyntaxTree);

            // The syntax nodes annotated by the query locator represent the terminating operators in the original source code.
            // This is exactly what we need to generate interceptors for; but for the purposes of compiling queries in the EF query
            // pipeline, it's problematic:
            // 1. Some terminating operators (ToList(), AsEnumerable()) should not be in the query tree
            // 2. The async terminating operators (e.g. SumAsync()) inject a node for their sync counterpart (Sum()), so we need to rewrite
            //    the tree.
            // So we'll work with two syntax tree/compilations/semantic models: the original one for generating the interceptors, and a
            // rewritten one for compiling the query with EF.
            var rewrittenSyntaxTree = _queryRewriter.RewriteQueries(compilation, annotatedSyntaxTree);

            rewrittenCompilation = compilation.ReplaceSyntaxTree(annotatedSyntaxTree, rewrittenSyntaxTree);
            syntaxTreePairs.Add((annotatedSyntaxTree, rewrittenSyntaxTree));
        }

        // This must be done after we complete generating the final compilation above
        _csharpToLinqTranslator.Load(rewrittenCompilation, dbContext);

        foreach (var (syntaxTree, rewrittenSyntaxTree) in syntaxTreePairs)
        {
            var generatedSyntaxTree = await ProcessSyntaxTreeAsync(
                syntaxTree, compilation, rewrittenSyntaxTree, rewrittenCompilation, cancellationToken).ConfigureAwait(false);
            if (generatedSyntaxTree is not null)
            {
                generatedSyntaxTrees.Add(generatedSyntaxTree);
            }
        }

        return generatedSyntaxTrees;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual async Task<SyntaxTree?> ProcessSyntaxTreeAsync(
        SyntaxTree syntaxTree,
        Compilation compilation,
        SyntaxTree rewrittenSyntaxTree,
        Compilation rewrittenCompilation,
        CancellationToken cancellationToken)
    {
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var rewrittenSemanticModel = rewrittenCompilation.GetSemanticModel(rewrittenSyntaxTree);

        var annotatedQueries = (await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false))
            .GetAnnotatedNodes(IQueryLocator.EfQueryCandidateAnnotationKind).ToList();
        var rewrittenQueries = (await rewrittenSyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false))
            .GetAnnotatedNodes(IQueryLocator.EfQueryCandidateAnnotationKind).ToList();

        Check.DebugAssert(annotatedQueries.Count == rewrittenQueries.Count, "Count mismatch");

        var queriesPrecompiledInFile = 0;
        _namespaces.Clear();
        var interceptors = new List<SyntaxNode>();

        for (var queryNum = 0; queryNum < annotatedQueries.Count; queryNum++)
        {
            var (query, rewrittenQuery) = (annotatedQueries[queryNum], rewrittenQueries[queryNum]);

            var async =
                rewrittenQuery.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind).Single().Data switch
                {
                    "Async" => true,
                    "Sync" => false,
                    _ => throw new InvalidOperationException(
                        $"Invalid data for syntax annotation {IQueryLocator.EfQueryCandidateAnnotationKind}")
                };

            // Convert the query's Roslyn syntax tree into a LINQ expression tree, and compile the query via EF's query pipeline.
            // This returns the query's executor function, which can produce an enumerable that invokes the query.
            // TODO: Handle query compilation failure
            // TODO: Get the parameters after parameter extraction, validate that what we extract below matches (as an assertion).
            var queryExecutor = CompileQuery(rewrittenQuery, rewrittenSemanticModel, async);

            // The query has been compiled successfully by the EF query pipeline.
            // Now go over each LINQ operator, generating an interceptor for it.
            ProcessQueryOperator(
                interceptors, (InvocationExpressionSyntax)query, queryNum + 1, operatorNum: out _, isTerminatingOperator: true, queryExecutor);

            // We're done generating the interceptors for the query's LINQ operators.
            // TODO: Wrap the query's interceptor in a region
            // interceptorMethodDeclaration = interceptorMethodDeclaration.WithLeadingTrivia(
            // RegionDirectiveTrivia());

            queriesPrecompiledInFile++;
        }

        if (queriesPrecompiledInFile == 0)
        {
            return null;
        }

        var usingDirectives = List(
            _namespaces
                // In addition to the namespaces auto-detected by LinqToCSharpTranslator, we manually add these namespaces which are required
                // by manually generated code above.
                .Append("System")
                .Append("System.Collections.Concurrent")
                .Append("System.Linq")
                .Append("System.Linq.Expressions")
                .Append("System.Runtime.CompilerServices")
                .Append("System.Reflection")
                .Append("System.Collections.Generic")
                .Append("Microsoft.EntityFrameworkCore")
                .Append("Microsoft.EntityFrameworkCore.Query")
                .Append("Microsoft.EntityFrameworkCore.ChangeTracking.Internal")
                .Append("Microsoft.EntityFrameworkCore.Query.Internal")
                .Append("Microsoft.EntityFrameworkCore.Diagnostics")
                .Append("Microsoft.EntityFrameworkCore.Infrastructure")
                .Append("Microsoft.EntityFrameworkCore.Infrastructure.Internal")
                .Append("Microsoft.EntityFrameworkCore.Metadata")
                .OrderBy(
                    ns => ns switch
                    {
                        _ when ns.StartsWith("System.", StringComparison.Ordinal) => 10,
                        _ when ns.StartsWith("Microsoft.", StringComparison.Ordinal) => 9,
                        _ => 0
                    })
                .ThenBy(ns => ns)
                .Select(_g.NamespaceImportDeclaration));

        // sealed class InterceptsLocationAttribute : Attribute
        // {
        //     public InterceptsLocationAttribute(string filePath, int line, int column) { }
        // }
        var interceptsLocationAttributeDeclaration =
            _g.ClassDeclaration(
                "InterceptsLocationAttribute",
                baseType: IdentifierName(nameof(Attribute)),
                modifiers: DeclarationModifiers.Sealed | DeclarationModifiers.File,
                members: new[]
                {
                    _g.ConstructorDeclaration(
                        accessibility: Accessibility.Public,
                        parameters: new[]
                        {
                            _g.ParameterDeclaration("filePath", _g.TypeExpression(SpecialType.System_String)),
                            _g.ParameterDeclaration("line", _g.TypeExpression(SpecialType.System_Int32)),
                            _g.ParameterDeclaration("column", _g.TypeExpression(SpecialType.System_Int32)),
                        }
                    )
                });

        // [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        interceptsLocationAttributeDeclaration = _g.AddAttributes(
            interceptsLocationAttributeDeclaration,
            _g.Attribute(
                "AttributeUsage",
                _g.MemberAccessExpression(IdentifierName("AttributeTargets"), nameof(AttributeTargets.Method)),
                _g.AttributeArgument("AllowMultiple", _g.TrueLiteralExpression())));

        // TODO: Add generated comment
        var compilationUnit =
            _g.CompilationUnit(
                    new List<SyntaxNode>(usingDirectives)
                    {
                        _g.NamespaceDeclaration(
                                InterceptorsNamespace,
                                _g.ClassDeclaration(
                                    "EntityFrameworkCoreInterceptors",
                                    modifiers: DeclarationModifiers.Static | DeclarationModifiers.File,
                                    members: interceptors))
                            .WithLeadingTrivia(
                                // Suppress EF1001 as it's OK to reference EF-pubternal stuff from within generated code.
                                Trivia(
                                    PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)
                                        .WithErrorCodes(SingletonSeparatedList<ExpressionSyntax>(IdentifierName("EF1001")))),
                                // TODO: Enable nullable reference types by inspecting Roslyn symbols of corresponding LINQ expression methods etc.
                                Trivia(NullableDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true))),
                        _g.NamespaceDeclaration("System.Runtime.CompilerServices", interceptsLocationAttributeDeclaration)
                    })
                .NormalizeWhitespace();

        return SyntaxTree(
            compilationUnit,
            path: $"{Path.GetFileNameWithoutExtension(syntaxTree.FilePath)}.EFInterceptors.g{Path.GetExtension(syntaxTree.FilePath)}");

        Expression CompileQuery(SyntaxNode querySyntax, SemanticModel semanticModel, bool async)
        {
            // We have a query lambda, as a Roslyn syntax tree. Translate to LINQ expression tree.
            // TODO: Add verification that this is an EF query over our user's context. If translation returns null the moment
            // there's another query root (another context or another LINQ provider), that's fine.
            var queryTree = _csharpToLinqTranslator.Translate(querySyntax, semanticModel);
            Type returnType;

            // We now have a LINQ representation of the query tree; we will now evaluate it to cause the queryable expression tree ot get
            // built - just like the operators are evaluated in normal query processing. Note the difference between the expression tree
            // representing the queryable operators (containing e.g. DbSet as the root), and the expression tree representing the result
            // of evaluating those operators (containing e.g. EntityQueryRootExpression).
            // However, we must not evaluate the last operator, since that would cause the query to get executed rather than produce the
            // query tree. The exception to this is if the last operator returns IQueryable - this happens when the query is terminated by
            // e.g. ToList(), which has already removed above by PrecompiledQueryRewriter (it isn't part of the query tree at all).
            if (queryTree.Type.IsGenericType
                && queryTree.Type.GetGenericTypeDefinition().IsAssignableTo(typeof(IQueryable)))
            {
                var queryable = Expression.Lambda<Func<IQueryable>>(queryTree).Compile(preferInterpretation: true)();
                queryTree = queryable.Expression;
                returnType = (async ? typeof(IAsyncEnumerable<>) : typeof(IEnumerable<>))
                    .MakeGenericType(queryTree.Type.GetGenericArguments()[0]);
            }
            else
            {
                // The terminating operator doesn't return IQueryable, but rather a scalar (e.g. Max/Sum).
                // Evaluate the penultimate operator to get the expression tree, then recompose the terminating operator on top of that.
                var terminatingOperator = ((MethodCallExpression)queryTree);
                var penultimateOperator = terminatingOperator.Arguments[0];
                var queryable = Expression.Lambda<Func<IQueryable>>(penultimateOperator).Compile(preferInterpretation: true)();

                queryTree = terminatingOperator.Update(
                    terminatingOperator.Object,
                    [queryable.Expression, .. terminatingOperator.Arguments.Skip(1)]);

                // For async terminating operators, we replaced them with their sync counterparts above in PrecompiledQueryRewriter, since
                // that's what the query pipeline expects. Rewrite the return type back to async.
                returnType = async ? typeof(Task<>).MakeGenericType(queryTree.Type) : queryTree.Type;
            }

            // We have the query as a LINQ expression tree.

            // We now need to figure out the return type of the query's executor.
            // Non-scalar query expressions return an IQueryable; the query executor will return an enumerable (sync or async).
            // Scalar query expressions just return the scalar type, wrap that in a Task for async.

            // Compile the query, invoking CompileQueryToExpression on the IQueryCompiler from the user's context instance.
            try
            {
                var queryExecutor = (Expression)_queryCompiler.GetType()
                    .GetMethod(nameof(IQueryCompiler.CompileQueryToExpression))
                    .MakeGenericMethod(returnType)
                    .Invoke(_queryCompiler, [queryTree, async])!;

                ShaperPublicMethodVerifier.Visit(queryExecutor);

                return queryExecutor;
            }
            catch (TargetInvocationException e) when (e.InnerException is not null)
            {
                // Unwrap the TargetInvocationException wrapper we get from Invoke()
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }

        void ProcessQueryOperator(
            List<SyntaxNode> interceptors,
            InvocationExpressionSyntax queryOperator,
            int queryNum,
            out int operatorNum,
            bool isTerminatingOperator,
            Expression? queryExecutor = null)
        {
            var statements = new List<SyntaxNode>();
            var memberAccess = (MemberAccessExpressionSyntax)queryOperator.Expression;

            // Create the parameter list for our interceptor method from the LINQ operator method's parameter list
            if (semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol is not IMethodSymbol interceptedMethodSymbol)
            {
                // TODO: Skip query gracefully
                throw new InvalidOperationException("Couldn't find method symbol for: " + memberAccess);
            }

            // For extension methods, this provides the form which has the "this" as its first parameter.
            // TODO: Validate the below, throw informative (e.g. top-level TVF fails here because non-generic)
            var reducedInterceptedMethodSymbol = interceptedMethodSymbol.GetConstructedReducedFrom() ?? interceptedMethodSymbol;
            var sourceParameterSymbol = reducedInterceptedMethodSymbol.Parameters[0];
            var queryableElementTypeParameter = interceptedMethodSymbol.OriginalDefinition.TypeParameters[0];
            var sourceParameterIdentifier = _g.IdentifierName(sourceParameterSymbol.Name);
            // var returnType = ((INamedTypeSymbol)interceptedMethodSymbol.OriginalDefinition.ReturnType);
            var returnType = interceptedMethodSymbol.OriginalDefinition.ReturnType;

            // TODO: Move out, cache
            // Unwrap Task<T> to get the element type (e.g. Task<List<int>>)
            var genericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var returnTypeWithoutTask = returnType is INamedTypeSymbol namedReturnType
                && returnType.OriginalDefinition.Equals(genericTaskSymbol, SymbolEqualityComparer.Default)
                    ? namedReturnType.TypeArguments[0]
                    : returnType;

            var returnElementType =
                returnTypeWithoutTask is INamedTypeSymbol namedReturnType2
                && namedReturnType2.AllInterfaces.Any(i => i.OriginalDefinition.Equals(_genericEnumerableSymbol, SymbolEqualityComparer.Default))
                    ? namedReturnType2.TypeArguments[0]
                    : null;

            // TODO: Move out, cache
            var precompiledQueryContextSymbol = compilation
                .GetTypeByMetadataName("Microsoft.EntityFrameworkCore.Query.Internal.PrecompiledQueryContext`1")
                .Construct(queryableElementTypeParameter);

            if (TryGetNestedQueryOperator(queryOperator, out var nestedOperator))
            {
                // This isn't the first query operator in the chain.
                // First recurse into the nested operator, to generate its interceptor first.
                ProcessQueryOperator(interceptors, nestedOperator, queryNum, out operatorNum, isTerminatingOperator: false);
                operatorNum++;

                // Then, when generating our interceptor, we'll need to receive the PrecompiledQueryContext from the nested operator and
                // flow it forward.

                // var precompiledQueryContext = (PrecompiledQueryContext<Blog>)source;
                statements.Add(
                    _g.LocalDeclarationStatement(
                        "precompiledQueryContext",
                        _g.CastExpression(precompiledQueryContextSymbol, sourceParameterIdentifier)));
            }
            else
            {
                // This is the first query operator in the chain. Cast the input source to IDbContextContainer and extract the EF
                // service provider, create a new QueryContext, and wrap it all in a PrecompiledQueryContext that will flow through to the
                // terminating operator, where the query will actually get executed.
                operatorNum = 1;

                // TODO: Move out, cache
                var dbContextContainerSymbol =
                    compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.Internal.IDbContextContainer")!;

                // var dbContext = ((IDbContextContainer)source).DbContext;
                statements.Add(
                    _g.LocalDeclarationStatement(
                        "dbContext",
                        _g.MemberAccessExpression(
                            _g.CastExpression(dbContextContainerSymbol, sourceParameterIdentifier),
                            nameof(InternalDbSet<string>.DbContext))));

                // var precompiledQueryContext = new PrecompiledQueryContext<Blog>();
                statements.Add(
                    _g.LocalDeclarationStatement(
                        "precompiledQueryContext",
                        _g.ObjectCreationExpression(precompiledQueryContextSymbol, _g.IdentifierName("dbContext"))));
            }

            // Go over the operator's arguments. For those which have captured variables, run them through our funcletizer, which
            // will return code for extracting any captured variables from them.
            var declaredQueryContextVariable = false;
            var arguments = queryOperator.ArgumentList.Arguments;
            var variableCounter = 0;
            // TODO: Skip 1st argument (source)?
            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var parameterSymbol = argument.NameColon is null
                    ? interceptedMethodSymbol.Parameters[i]
                    : interceptedMethodSymbol.Parameters.Single(p => p.Name == argument.NameColon.Name.Identifier.Text);

                if (!SymbolEqualityComparer.Default.Equals(parameterSymbol.Type.OriginalDefinition, _linqGenericLambdaType))
                {
                    continue;
                }

                // TODO: It may be possible to use Roslyn data flow analysis here, but that seems to also look at nested operators
                // of this operator, even though we give it only the argument...
                // This is necessary if we want to know the reference nullability of captured variables, so we can optimize the SQLs
                // pregenerated (depends on the number of nullable parameters).
                // var captured = semanticModel.AnalyzeDataFlow(argument.Expression).Captured;
                // if (captured.Length > 0)
                // {
                //     var argumentAsLinq = _csharpToLinqTranslator.Translate(argument, semanticModel);
                //     var boo = funcletizer.Funcletize(argumentAsLinq);
                // }

                var argumentAsLinq = _csharpToLinqTranslator.Translate(argument.Expression, semanticModel);
                var evaluatableRootPaths = _funcletizer.CalculatePathsToEvaluatableRoots(argumentAsLinq);

                if (evaluatableRootPaths is null)
                {
                    // There are no captured variables in this lambda argument - skip the argument
                    continue;
                }

                // We have a lambda argument with captured variables. Use the information returned by the funcletizer to generate code
                // which extracts them and sets them on our query context.
                if (!declaredQueryContextVariable)
                {
                    // var queryContext = precompiledQueryContext.QueryContext;
                    statements.Add(
                        _g.LocalDeclarationStatement(
                            "queryContext",
                            _g.MemberAccessExpression(
                                _g.IdentifierName("precompiledQueryContext"), nameof(PrecompiledQueryContext<int>.QueryContext))));

                    declaredQueryContextVariable = true;
                }

                var parameterType = _csharpToLinqTranslator.TranslateType(parameterSymbol.Type);
                foreach (var child in evaluatableRootPaths.Children)
                {
                    GenerateCapturedVariableExtractors(parameterSymbol.Name, parameterType, child);

                    void GenerateCapturedVariableExtractors(string currentIdentifier, Type currentType, ExpressionTreeFuncletizer.PathNode capturedVariablesPathTree)
                    {
                        var linqPathSegment = capturedVariablesPathTree.PathFromParent(Expression.Parameter(currentType, currentIdentifier));
                        var collectedNamespaces = new HashSet<string>();
                        var roslynPathSegment = _linqToCSharpTranslator.TranslateExpression(
                            linqPathSegment, constantReplacements: null, collectedNamespaces);

                        var cast = _g.CastExpression(
                            capturedVariablesPathTree.ExpressionType.GetTypeSyntax(),
                            roslynPathSegment);

                        var variableName = capturedVariablesPathTree.ExpressionType.Name;
                        variableName = char.ToLower(variableName[0]) + variableName[1..^"Expression".Length] + ++variableCounter;
                        statements.Add(_g.LocalDeclarationStatement(variableName, cast));

                        if (capturedVariablesPathTree.Children.Count > 0)
                        {
                            // This is an intermediate node which has captured variables in the children. Continue recursing down.
                            foreach (var child in capturedVariablesPathTree.Children)
                            {
                                GenerateCapturedVariableExtractors(variableName, capturedVariablesPathTree.ExpressionType, child);
                            }

                            return;
                        }

                        // We've reached a leaf, meaning that it's an evaluatable node that contains captured variables.
                        // Generate code to evaluate this node and assign the result to the parameters dictionary:

                        // try
                        // {
                        //     parameters.Add("__p_0", Expression.Lambda<Func<object>>(
                        //             Expression.Convert(expression, typeof(object)))
                        //         .Compile(preferInterpretation: true)
                        //         .Invoke());
                        // }
                        // catch (Exception exception)
                        // {
                        //     throw new InvalidOperationException(
                        //         _logger.ShouldLogSensitiveData()
                        //             ? CoreStrings.ExpressionParameterizationExceptionSensitive(expression)
                        //             : CoreStrings.ExpressionParameterizationException,
                        //         exception);
                        // }

                        // Expression.Convert(expression, typeof(object))
                        var evaluator =
                            _g.InvocationExpression(
                                _g.MemberAccessExpression(
                                    _g.IdentifierName(nameof(Expression)), // TODO: This should be a type symbol
                                    nameof(Expression.Convert)),
                                _g.IdentifierName(variableName),
                                _g.TypeOfExpression(_g.TypeExpression(SpecialType.System_Object)));

                        // Expression.Lambda<Func<object?>>(Expression.Convert(right1, typeof(object)))
                        evaluator =
                            _g.InvocationExpression(
                                _g.MemberAccessExpression(
                                    _g.IdentifierName(nameof(Expression)), // TODO: This should be a type symbol
                                    _g.GenericName(
                                        nameof(Expression.Lambda),
                                        _g.GenericName(
                                            "Func",
                                            _g.TypeExpression(SpecialType.System_Object)))),
                                evaluator);

                        // TODO: Remove the convert to object. We can flow out the actual type of the evaluatable root, and just stick it
                        //       in Func<> instead of object.
                        // TODO: For specific cases, don't go through the interpreter, but just integrate code that extracts the value directly.
                        //       (see ExpressionTreeFuncletizer.Evaluate()).
                        // TODO: Basically this means that the evaluator should come from ExpressionTreeFuncletizer itself, as part of its outputs
                        // TODO: Integrate try/catch around the evaluation?
                        // Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile(preferInterpretation: true).Invoke();
                        evaluator =
                            _g.InvocationExpression(
                                _g.MemberAccessExpression(
                                    _g.InvocationExpression(
                                        _g.MemberAccessExpression(
                                            evaluator,
                                            nameof(Expression<int>.Compile)),
                                        _g.Argument("preferInterpretation", RefKind.None, _g.TrueLiteralExpression())),
                                    "Invoke"));

                        // queryContext.Add("__p_0", Expression.Lambda<Func<object>>(
                        //         Expression.Convert(expression, typeof(object)))
                        //     .Compile(preferInterpretation: true)
                        //     .Invoke());
                        statements.Add(
                            _g.InvocationExpression(
                                _g.MemberAccessExpression(_g.IdentifierName("queryContext"), nameof(QueryContext.AddParameter)),
                                _g.LiteralExpression(capturedVariablesPathTree.ParameterName!),
                                evaluator));
                    }
                }
            }

            if (isTerminatingOperator)
            {
                // We're intercepting the query's terminating operator - this is where the query actually gets executed.
                if (!declaredQueryContextVariable)
                {
                    // var queryContext = precompiledQueryContext.QueryContext;
                    statements.Add(
                        _g.LocalDeclarationStatement(
                            "queryContext",
                            _g.MemberAccessExpression(
                                _g.IdentifierName("precompiledQueryContext"), nameof(PrecompiledQueryContext<int>.QueryContext))));
                }

                // if (Query1_Executor == null) {
                //     Query1_Executor = Query1_GenerateExecutor(precompiledQueryContext.DbContext, precompiledQueryContext.QueryContext);
                // }
                var executorFieldIdentifier = _g.IdentifierName($"Query{queryNum}_Executor");
                statements.Add(
                    _g.IfStatement(
                        _g.ReferenceEqualsExpression(
                            executorFieldIdentifier,
                            _g.NullLiteralExpression()),
                        new[]
                        {
                            _g.AssignmentStatement(
                                executorFieldIdentifier,
                                _g.InvocationExpression(
                                    _g.IdentifierName($"Query{queryNum}_GenerateExecutor"),
                                    _g.MemberAccessExpression(_g.IdentifierName("precompiledQueryContext"), "DbContext"),
                                    _g.IdentifierName("queryContext")))
                        }));

                // TODO: Look at merging the two code paths a bit more once everything works
                if (returnElementType is null)
                {
                    // The query returns a scalar, not an enumerable (e.g. the terminating operator is Max()).
                    // The executor directly returns the needed result (e.g. int), so just return that.

                    // Func<QueryContext, TSource>
                    var executorTypeSymbol = _g.TypeExpression(
                        compilation.GetTypeByMetadataName(typeof(Func<,>).FullName!)
                            .Construct(
                                compilation.GetTypeByMetadataName(typeof(QueryContext).FullName!)!,
                                returnType));

                    // return ((Func<QueryContext, TSource>)(Query1_Executor))(queryContext);
                    statements.Add(
                        _g.ReturnStatement(
                            (ExpressionSyntax)_g.InvocationExpression(
                                _g.CastExpression(executorTypeSymbol, executorFieldIdentifier),
                                _g.IdentifierName("queryContext"))));
                }
                else
                {
                    // The query returns an IEnumerable, which is a bit trickier: the executor doesn't return a simple value as in the
                    // scalar case, but rather e.g. SingleQueryingEnumerable; we need to compose the terminating operator (e.g. ToList())
                    // on top of that. Cast the executor delegate to Func<QueryContext, IEnumerable<T>> (contravariance).

                    // Func<QueryContext, TSource>
                    var executorTypeSymbol = _g.TypeExpression(
                        compilation.GetTypeByMetadataName(typeof(Func<,>).FullName!)
                            .Construct(
                                compilation.GetTypeByMetadataName(typeof(QueryContext).FullName!)!,
                                _genericEnumerableSymbol.Construct(returnElementType)));

                    // return ((Func<QueryContext, IEnumerable<TSource>>)(Query1_Executor))(queryContext);
                    statements.Add(
                        _g.ReturnStatement(
                            queryOperator.WithExpression(
                                memberAccess.WithExpression(
                                    (ExpressionSyntax)_g.InvocationExpression(
                                        _g.CastExpression(executorTypeSymbol, executorFieldIdentifier),
                                        _g.IdentifierName("queryContext"))))));

                    // statements.Add(
                    //     _g.ReturnStatement(
                    //         queryOperator.WithExpression(
                    //             memberAccess.WithExpression(
                    //                 (ExpressionSyntax)_g.CastExpression(
                    //                     compilation.GetTypeByMetadataName(typeof(IQueryable<>).FullName!)!.Construct(returnElementType),
                    //                     queryResult)))));
                }
            }
            else
            {
                // Non-terminating operator - we need to flow precompiledQueryContext forward.
                Check.DebugAssert(returnElementType is not null, "Non-terminating operator must return IEnumerable<T>");

                if (SymbolEqualityComparer.Default.Equals(queryableElementTypeParameter, returnElementType))
                {
                    // The operator returns the same IQueryable type as its source (e.g. Where, OrderBy), simply return
                    // precompiledQueryContext as-is.
                    statements.Add(_g.ReturnStatement(_g.IdentifierName("precompiledQueryContext")));
                }
                else
                {
                    // The operator returns a different IQueryable type as its source (e.g. Select), convert the precompiledQueryContext
                    // before returning it.

                    // return precompiledQueryContext.ChangeType<TResult>();
                    statements.Add(
                        _g.ReturnStatement(
                            _g.InvocationExpression(
                                _g.MemberAccessExpression(
                                    _g.IdentifierName("precompiledQueryContext"),
                                    _g.GenericName(
                                        nameof(PrecompiledQueryContext<int>.ChangeType),
                                        returnElementType)))));
                }
            }

            // We're done generating the interceptor statements. Create a method declaration for it and return.

            var startPosition = syntaxTree.GetLineSpan(memberAccess.Name.Span, cancellationToken).StartLinePosition;
            var interceptorName = $"Query{queryNum}_{memberAccess.Name}{operatorNum}";

            // To create the interceptor method declaration, we copy the method definition of the original intercepted method, replacing the
            // name and adding our interceptor statements.

            // [InterceptsLocation("Program.cs", 15, 15)]
            // public static IQueryable<TSource> Query1_Where2<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            var interceptorMethodDeclaration =
                _g.AddAttributes(
                    _g.WithName(
                        _g.MethodDeclaration(reducedInterceptedMethodSymbol.OriginalDefinition, statements),
                        interceptorName),
                    _g.Attribute(
                        "InterceptsLocation",
                        _g.LiteralExpression(syntaxTree.FilePath),
                        _g.LiteralExpression(startPosition.Line + 1),
                        _g.LiteralExpression(startPosition.Character + 1)));

            interceptors.Add(interceptorMethodDeclaration);

            if (isTerminatingOperator)
            {
                var variableNames = new HashSet<string>(); // TODO
                GenerateQueryExecutor(
                    queryNum, queryExecutor!, _namespaces, variableNames,
                    out var queryExecutorFieldDeclaration,
                    out var queryExecutorGeneratorMethodDeclaration);

                interceptors.Add(queryExecutorGeneratorMethodDeclaration);
                interceptors.Add(queryExecutorFieldDeclaration);
            }
        }

        void GenerateQueryExecutor(
            int queryNum,
            Expression queryExecutor,
            HashSet<string> namespaces,
            HashSet<string> variableNames,
            out SyntaxNode queryExecutorFieldDeclaration,
            out SyntaxNode queryExecutorGeneratorMethodDeclaration)
        {
            var statements = new List<SyntaxNode>
            {
                // var relationalModel = dbContext.Model.GetRelationalModel();
                _g.LocalDeclarationStatement(
                    "relationalModel",
                    _g.InvocationExpression(
                        _g.MemberAccessExpression(
                            _g.MemberAccessExpression(_g.IdentifierName("dbContext"), nameof(DbContext.Model)),
                            nameof(RelationalModelExtensions.GetRelationalModel)))),

                // var relationalTypeMappingSource = dbContext.GetService<IRelationalTypeMappingSource>();
                _g.LocalDeclarationStatement(
                    "relationalTypeMappingSource",
                    GenerateGetService(compilation.GetTypeByMetadataName(typeof(IRelationalTypeMappingSource).FullName!)!)),

                // var materializerLiftableConstantContext = new RelationalMaterializerLiftableConstantContext(
                //     dbContext.GetService<ShapedQueryCompilingExpressionVisitorDependencies>(),
                //     dbContext.GetService<RelationalShapedQueryCompilingExpressionVisitorDependencies>())
                _g.LocalDeclarationStatement(
                    "materializerLiftableConstantContext",
                    _g.ObjectCreationExpression(
                        compilation.GetTypeByMetadataName(typeof(RelationalMaterializerLiftableConstantContext).FullName!)!,
                        GenerateGetService(
                            compilation.GetTypeByMetadataName(typeof(ShapedQueryCompilingExpressionVisitorDependencies).FullName!)!),
                        GenerateGetService(
                            compilation.GetTypeByMetadataName(
                                typeof(RelationalShapedQueryCompilingExpressionVisitorDependencies).FullName!)!)))
            };

            variableNames.UnionWith(new[] { "relationalModel", "relationalTypeMappingSource", "materializerLiftableConstantContext" });

            var materializerLiftableConstantContext =
                Expression.Parameter(typeof(RelationalMaterializerLiftableConstantContext), "materializerLiftableConstantContext");

            // The materializer expression tree contains LiftedConstantExpression nodes, which contain instructions on how to resolve
            // constant values which need to be lifted.
            var queryExecutorAfterLiftingExpression =
                _liftableConstantProcessor.LiftConstants(queryExecutor, materializerLiftableConstantContext, variableNames);

            var sqlTreeCounter = 0;

            foreach (var liftedConstant in _liftableConstantProcessor.LiftedConstants)
            {
                var (parameter, variableValue) = liftedConstant;

                // TODO: Somewhat hacky, special handling for the SQL tree argument of RelationalCommandCache (since it requires
                // very special rendering logic
                if (parameter.Type == typeof(RelationalCommandCache))
                {
                    if (variableValue is NewExpression newRelationalCommandCacheExpression
                        && newRelationalCommandCacheExpression.Arguments.FirstOrDefault(a => a.Type == typeof(Expression)) is
                            ConstantExpression { Value: Expression queryExpression })
                    {
                        if (queryExpression is not IRelationalQuotableExpression quotableExpression)
                        {
                            throw new InvalidOperationException("SQL tree expression isn't quotable: " + queryExpression.GetType().Name);
                        }

                        var quotedSqlTree = quotableExpression.Quote();

                        // Render out the SQL tree, preceded by an ExpressionPrinter dump of it in a comment for easier debugging.
                        // Note that since the SQL tree is a graph (columns reference their SelectExpression's tables), rendering happens
                        // in multiple statements.
                        var sqlTreeVariable = "sqlTree" + (++sqlTreeCounter);
                        variableNames.Add(sqlTreeVariable);

                        var sqlTreeAssignment =
                            _g.LocalDeclarationStatement(
                                sqlTreeVariable,
                                _linqToCSharpTranslator.TranslateExpression(quotedSqlTree, constantReplacements: null, namespaces));

                        sqlTreeAssignment = sqlTreeAssignment.WithLeadingTrivia(
                            Comment(
                                _stringBuilder
                                    .Clear()
                                    .AppendLine("/*")
                                    .AppendLine(_sqlExpressionPrinter.PrintExpression(queryExpression))
                                    .AppendLine("*/")
                                    .ToString()));

                        statements.Add(sqlTreeAssignment);

                        // We've rendered the SQL tree, assigning it to variable "sqlTree". Update the RelationalCommandCache to point
                        // to it
                        variableValue = newRelationalCommandCacheExpression.Update(newRelationalCommandCacheExpression.Arguments
                            .Select(a => a.Type == typeof(Expression)
                                ? Expression.Parameter(typeof(Expression), sqlTreeVariable)
                                : a));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Could not find SQL query in lifted {nameof(RelationalCommandCache)}");
                    }
                }

                statements.Add(
                    _g.LocalDeclarationStatement(
                        parameter.Name!,
                        _linqToCSharpTranslator.TranslateExpression(variableValue, constantReplacements: null, namespaces)));
            }

            var queryExecutorSyntaxTree =
                (AnonymousFunctionExpressionSyntax)_linqToCSharpTranslator.TranslateExpression(
                    queryExecutorAfterLiftingExpression,
                    constantReplacements: null,
                    namespaces);

            // return (QueryContext queryContext) => SingleQueryingEnumerable.Create(......);
            statements.Add(_g.ReturnStatement(queryExecutorSyntaxTree));

            // We're done generating the method which will create the query executor (Func<QueryContext, TResult>).
            // Note that the we store the executor itself (and return it) as object, not as a typed Func<QueryContext, TResult>.
            // We can't strong-type it since it may return an anonymous type, which is unspeakable; so instead we cast down from object to
            // the real strongly-typed signature inside the interceptor, where the return value is represented as a generic type parameter
            // (which can be an anonymous type).
            // TODO: We can use strong types instead of object (and avoid the downcast) for cases where there are no unspeakable types.

            // private static void Query1_GenerateExecutor(BlogContext dbContext)
            queryExecutorGeneratorMethodDeclaration = _g.MethodDeclaration(
                accessibility: Accessibility.Private,
                modifiers: DeclarationModifiers.Static,
                returnType: _g.TypeExpression(SpecialType.System_Object),
                name: $"Query{queryNum}_GenerateExecutor",
                parameters:
                [
                    _g.ParameterDeclaration(
                        "dbContext", _g.TypeExpression(compilation.GetTypeByMetadataName(typeof(DbContext).FullName!)!)),
                    _g.ParameterDeclaration(
                        "queryContext", _g.TypeExpression(compilation.GetTypeByMetadataName(typeof(QueryContext).FullName!)!))
                ],
                statements: statements);

            // private static readonly object Query1_Executor;
            queryExecutorFieldDeclaration =
                _g.FieldDeclaration(
                    accessibility: Accessibility.Private,
                    modifiers: DeclarationModifiers.Static,
                    name: $"Query{queryNum}_Executor",
                    type: _g.TypeExpression(SpecialType.System_Object));

            SyntaxNode GenerateGetService(INamedTypeSymbol serviceType)
                => _g.InvocationExpression(
                    _g.MemberAccessExpression(
                        _g.IdentifierName("dbContext"), _g.GenericName(nameof(IServiceProvider.GetService), serviceType)));
        }
    }

    /// <summary>
    /// Detects whether the invocation represents the first queryable operator in a LINQ operator chain.
    /// </summary>
    private bool TryGetNestedQueryOperator(
        InvocationExpressionSyntax queryOperator,
        [NotNullWhen(true)] out InvocationExpressionSyntax? nestedOperator)
    {
        var memberAccess = (MemberAccessExpressionSyntax)queryOperator.Expression;

        // TODO: Also need to detect DbContext.Set<T>() invocation
        if (memberAccess.Expression is InvocationExpressionSyntax nested)
        {
            nestedOperator = nested;
            return true;
        }

        nestedOperator = null;
        return false;
    }
}
