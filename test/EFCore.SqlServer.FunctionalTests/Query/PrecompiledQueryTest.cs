// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

// TODO: Move to EFCore.Specification.Tests, TestBase and so on
public class PrecompiledQueryTest(
    PrecompiledQueryTest.PrecompiledQueryFixture fixture,
    ITestOutputHelper testOutputHelper) : IClassFixture<PrecompiledQueryTest.PrecompiledQueryFixture>
{
    #region Expression types

    [ConditionalFact]
    public virtual Task BinaryExpression()
        => Test("""
var id = 3;
var blogs = ctx.Blogs.Where(b => b.Id > id).ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_no_evaluatable()
        => Test("""
var id = 3;
var blogs = ctx.Blogs.Select(b => b.Id == 2 ? "yes" : "no").ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_contains_captured_variable()
        => Test("""
var yes = "yes";
var blogs = ctx.Blogs.Select(b => b.Id == 2 ? yes : "no").ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_evaluatable_as_constant()
        => Test("""
var blogs = ctx.Blogs.Select(b => Funcs.ReturnSame(4) == 3 ? "yes" : "no").ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_evaluatable_as_parameter()
        => Test("""
var yes = "yes";
var blogs = ctx.Blogs.Select(b => Funcs.ReturnSame(4) == 3 ? yes : "no").ToList();

int foo() => 3;
""");

    [ConditionalFact]
    public virtual Task Index_no_evaluatable()
        => Test("""
var blogs = ctx.Blogs.Select(b => b.IntArray[b.Id] == 2).ToList();
""");

    // We do not support embedding Expression builder API calls into the query; this would require CSharpToLinqTranslator to actually
    // evaluate those APIs and embed the results into the tree. It's (at least potentially) a form of dynamic query, unsupported for now.
    [ConditionalFact]
    public virtual Task Invoke_no_evaluatability()
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => Test(
                """
Expression<Func<Blog, bool>> lambda = b => b.Name == "foo";
var parameter = Expression.Parameter(typeof(Blog), "b");

var blogs = ctx.Blogs
    .Where(Expression.Lambda<Func<Blog, bool>>(Expression.Invoke(lambda, parameter), parameter))
    .ToList();
"""));

    [ConditionalFact]
    public virtual Task ListInit_no_evaluatability()
        => Test("""_ = ctx.Blogs.Select(b => new List<int> { b.Id, b.Id + 1 }).ToList();""");

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_with_captured_variable()
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => Test(
                """
var i = 1;
_ = ctx.Blogs.Select(b => new List<int> { b.Id, i }).ToList();
"""));

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_without_captured_variable()
        => Test(
                """
var i = 1;
_ = ctx.Blogs.Select(b => new List<int> { b.Id, 8 }).ToList();
""");

    [ConditionalFact]
    public virtual Task ListInit_fully_evaluatable()
        => Test("_ = ctx.Blogs.Select(b => new List<int> { 1, 2 }).ToList();");

    [ConditionalFact]
    public virtual Task MethodCallExpression_no_evaluatability()
        => Test("""_ = ctx.Blogs.Where(b => b.Name.StartsWith(b.Name)).ToList();""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_with_captured_variable()
        => Test("""
var pattern = "foo";
_ = ctx.Blogs.Where(b => b.Name.StartsWith(pattern)).ToList();
""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_without_captured_variable()
        => Test("""_ = ctx.Blogs.Where(b => b.Name.StartsWith("foo")).ToList();""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_fully_evaluatable()
        => Test("""_ = ctx.Blogs.Where(b => "foobar".StartsWith("foo")).ToList();""");

    [ConditionalFact]
    public virtual Task New_with_no_arguments()
        => Test(
            """
var i = 8;
_ = ctx.Blogs.Where(b => b == new Blog()).ToList();
""");

    // This doesn't translate via the regular pipeline either, but check the specific exception etc.
    // Note that the current behavior is to parameterize the NewExpression, not sure that's right for all cases.
    [ConditionalFact]
    public virtual Task Where_New_with_captured_variable()
        => Assert.ThrowsAsync<InvalidOperationException>(() => Test(
            """
var i = 8;
_ = ctx.Blogs.Where(b => b == new Blog(i, b.Name)).ToList();
"""));

    [ConditionalFact]
    public virtual Task Select_New_with_captured_variable()
        => Test(
            """
var i = 8;
_ = ctx.Blogs.Select(b => new Blog(i, b.Name)).ToList();
""");

    [ConditionalFact]
    public virtual Task MemberInit_no_evaluatable()
        => Test("_ = ctx.Blogs.Select(b => new Blog { Id = b.Id, Name = b.Name }).ToList();");

    [ConditionalFact]
    public virtual Task MemberInit_contains_captured_variable()
        => Test(
            """
var id = 8;
_ = ctx.Blogs.Select(b => new Blog { Id = id, Name = b.Name }).ToList();
""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_constant()
        => Test("""_ = ctx.Blogs.Select(b => new Blog { Id = 1, Name = "foo" }).ToList();""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_parameter()
        => Test(
            """
var id = 8;
var name = "foo";
_ = ctx.Blogs.Select(b => new Blog { Id = id, Name = name }).ToList();
""");

    [ConditionalFact]
    public virtual Task NewArray()
        => Test(
            """
var i = 8;
_ = ctx.Blogs.Select(b => new[] { b.Id, b.Id + i }).ToList();
""");

    [ConditionalFact]
    public virtual Task Unary()
        => Test("_ = ctx.Blogs.Where(b => (short)b.Id == (short)8).ToList();");

    #endregion Expression types

    #region Terminating operators

    [ConditionalFact]
    public virtual Task AsEnumerable()
        => Test("""
var blogs = ctx.Blogs.AsEnumerable();
foreach (var blog in blogs)
{
}
""");

    // Note that foreach/await foreach directly over DbSet properties doesn't get actually get precompiled, since we can't intercept
    // property accesses.
    [ConditionalFact]
    public virtual Task Foreach_sync()
        => Test("""
foreach (var blog in ctx.Blogs)
{
}
""");

    // Note that foreach/await foreach directly over DbSet properties doesn't get actually get precompiled, since we can't intercept
    // property accesses.
    [ConditionalFact]
    public virtual Task Foreach_async()
        => Test("""
await foreach (var blog in ctx.Blogs)
{
}
""");

    [ConditionalFact]
    public virtual Task Foreach_AsAsyncEnumerable()
        => Test("""
await foreach (var blog in ctx.Blogs.AsAsyncEnumerable())
{
}
""");

    [ConditionalFact]
    public virtual Task ToList()
        => Test("_ = ctx.Blogs.ToList();");

    [ConditionalFact]
    public virtual Task ToListAsync()
        => Test("_ = await ctx.Blogs.ToListAsync();");

    [ConditionalFact]
    public virtual Task Sum_without_selector()
        => Test("_ = ctx.Blogs.Select(b => b.Id).Sum();");

    [ConditionalFact]
    public virtual Task Sum_with_selector()
        => Test("_ = ctx.Blogs.Sum(b => b.Id);");

    [ConditionalFact]
    public virtual Task SumAsync_without_selector()
        => Test("_ = await ctx.Blogs.Select(b => b.Id).SumAsync();");

    [ConditionalFact]
    public virtual Task SumAsync_with_selector()
        => Test("_ = await ctx.Blogs.SumAsync(b => b.Id);");

    #endregion Terminating operators

    [ConditionalFact]
    public virtual Task Select_changes_type()
        => Test("_ = ctx.Blogs.Select(b => b.Name).ToList();");

    [ConditionalFact]
    public virtual Task IOrderedQueryable()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task Project_anonymous_object()
        => Test("""_ = ctx.Blogs.Select(b => new { Foo = b.Name + "Foo" }).ToList();""");

    [ConditionalFact]
    public virtual Task Two_captured_variables()
        => Test("""
var yes = "yes";
var no = "no";
var blogs = ctx.Blogs.Select(b => b.Id == 3 ? yes : no).ToList();

int foo() => 3;
""");

    [ConditionalFact]
    public virtual Task Split_query()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task ExecuteDelete()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task ExecuteUpdate()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task Foo()
        => Test(
            """
var id = 3;
var blogs = ctx.Blogs.Where(b => b.Id > id).Select(b => b.Name).ToList();
""");

    protected virtual async Task Test(string sourceCode, [CallerMemberName] string callerName = "")
    {
        // The overall end-to-end testing for precompiled queries is as follows:
        // 1. Compile the user code, produce an assembly from it and load it. We need to do this since precompiled query generation requires
        //    an actual DbContext instance, from which we get the model, services, ec.
        // 2. Do precompiled query generation. This outputs additional source files (syntax trees) containing interceptors for the located
        //    EF LINQ queries.
        // 3. Integrate the additional syntax trees into the compilation, and again, produce an assembly from it and load it.
        // 4. Use reflection to find the EntryPoint (Main method) on this assembly, and invoke it.
        var source = """
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.PrecompiledQueryTest;

await using var ctx = new BlogContext();

""" + sourceCode;

        var modelSourceCode = $$"""
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// TODO: Do this for extra-careful isolation (but there's some codegen issue)
//namespace Microsoft.EntityFrameworkCore.PrecompiledQueryTest;

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

// TODO: Ideally, introduce a way to disable JIT query processing so that if we generate the wrong thing and it doesn't get picked up,
// the query fails. This way we ensure that if the test passes, it used interception.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("{{fixture.ConnectionString}}");
}

public class Blog
{
    public Blog()
    {
    }

    public Blog(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int[] IntArray { get; set; }
}

public class Funcs
{
    public static T ReturnSame<T>(T p) => p;
}
""";

        // This turns on the interceptors feature for the designated namespace(s).
        var interceptorsFeature =
            new[]
            {
                new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.EntityFrameworkCore.GeneratedInterceptors")
            };

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source, path: "Test.cs", options: new CSharpParseOptions().WithFeatures(interceptorsFeature));
        var modelSyntaxTree = CSharpSyntaxTree.ParseText(
            modelSourceCode, path: "Model.cs", options: new CSharpParseOptions().WithFeatures(interceptorsFeature));

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            syntaxTrees: new[] { syntaxTree, modelSyntaxTree },
            references: MetadataReferences);

        IReadOnlyList<SyntaxTree> generatedSyntaxTrees;

        // The test code compiled - emit and assembly and load it.
        var (assemblyLoadContext, assembly) = EmitAndLoadAssembly(compilation, callerName + "_Original");
        try
        {
            var workspace = new AdhocWorkspace();
            var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);

            // TODO: Look up as regular dependencies
            var precompiledQueryCodeGenerator = new PrecompiledQueryCodeGenerator(new QueryLocator(), new CSharpToLinqTranslator());

            var dbContextType = assembly.GetType("BlogContext")!;

            await using var dbContext = (DbContext)Activator.CreateInstance(dbContextType);

            // Perform precompilation
            generatedSyntaxTrees = await precompiledQueryCodeGenerator.GeneratePrecompiledQueries(compilation, syntaxGenerator, dbContext);
        }
        finally
        {
            assemblyLoadContext.Unload();
        }

        foreach (var generatedSyntaxTree in generatedSyntaxTrees)
        {
            testOutputHelper.WriteLine($"Generated file {generatedSyntaxTree.FilePath}: ");
            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine((await generatedSyntaxTree.GetRootAsync()).ToFullString());
        }

        // We now have the code-generated interceptors; add them to the compilation and re-emit.
        compilation = compilation.AddSyntaxTrees(
            generatedSyntaxTrees.Select(t => t.WithRootAndOptions(
                t.GetRoot(),
                t.Options.WithFeatures(interceptorsFeature))));

        // We have the final compilation, including the interceptors. Emit and load it, and then invoke its entry point, which contains
        // the original test code with the EF LINQ query, etc.
        (assemblyLoadContext, assembly) = EmitAndLoadAssembly(compilation, callerName + "_WithInterceptors");
        try
        {
            assembly.EntryPoint!.Invoke(obj: null, parameters: new object[] { Array.Empty<string>() });
        }
        finally
        {
            assemblyLoadContext.Unload();
        }

        static (AssemblyLoadContext, Assembly) EmitAndLoadAssembly(Compilation compilation, string assemblyLoadContextName)
        {
            var errorDiagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorDiagnostics.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Compilation failed:").AppendLine();

                foreach (var errorDiagnostic in errorDiagnostics)
                {
                    stringBuilder.AppendLine(errorDiagnostic.ToString());

                    var textLines = errorDiagnostic.Location.SourceTree!.GetText().Lines;
                    var startLine = errorDiagnostic.Location.GetLineSpan().StartLinePosition.Line;
                    var endLine = errorDiagnostic.Location.GetLineSpan().EndLinePosition.Line;

                    if (startLine == endLine)
                    {
                        stringBuilder.Append("Line: ").AppendLine(textLines[startLine].ToString().TrimStart());
                    }
                    else
                    {
                        stringBuilder.AppendLine("Lines:");
                        for (var i = startLine; i <= endLine; i++)
                        {
                            stringBuilder.AppendLine(textLines[i].ToString());
                        }
                    }
                }

                throw new InvalidOperationException("Compilation failed:" + stringBuilder);
            }

            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);
            memoryStream.Position = 0;

            errorDiagnostics = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorDiagnostics.Count > 0)
            {
                throw new InvalidOperationException(
                    "Compilation emit failed:" + Environment.NewLine + string.Join(Environment.NewLine, errorDiagnostics));
            }

            var assemblyLoadContext = new AssemblyLoadContext(assemblyLoadContextName, isCollectible: true);
            var assembly = assemblyLoadContext.LoadFromStream(memoryStream);
            return (assemblyLoadContext, assembly);
        }
    }

    private static readonly MetadataReference[] MetadataReferences;

    static PrecompiledQueryTest()
    {
        var metadataReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Queryable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(RelationalOptionsExtension).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SqlServerOptionsExtension).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbConnection).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IListSource).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IMemoryCache).Assembly.Location),
        };

        var netAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "mscorlib.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Core.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Runtime.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Collections.dll")));

        MetadataReferences = metadataReferences.ToArray();
    }

    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

    public class PrecompiledQueryFixture : IAsyncLifetime
    {
        protected virtual string StoreName
            => nameof(PrecompiledQueryTest);

        protected virtual ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        private TestStore _testStore;
        public string ConnectionString { get; private set; }

        public Task InitializeAsync()
        {
            _testStore = TestStoreFactory.GetOrCreate(StoreName);
            ConnectionString = ((RelationalTestStore)_testStore).ConnectionString;

            _testStore.Initialize(
                serviceProvider: null, createContext: (Func<DbContext>)null, seed: c =>
                {
                    c.Database.ExecuteSql(
                        $"""
CREATE TABLE [Blogs] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
);
""");
                });

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
            => _testStore.DisposeAsync();
    }
}
