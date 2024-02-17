// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Xunit.Sdk;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

// TODO: Make all the below async (ToListAsync) once that works (except for the specific terminating operator tests)
public class PrecompiledQueryRelationalTestBase(PrecompiledQueryRelationalFixture fixture, ITestOutputHelper testOutputHelper)
{
    #region Expression types

    [ConditionalFact]
    public virtual Task BinaryExpression()
        => Test("""
var id = 3;
var blogs = context.Blogs.Where(b => b.Id > id).ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_no_evaluatable()
        => Test("""
var id = 3;
var blogs = context.Blogs.Select(b => b.Id == 2 ? "yes" : "no").ToList();
""");

    [ConditionalFact]
    public virtual Task Conditional_contains_captured_variable()
        => Test("""
var yes = "yes";
var blogs = context.Blogs.Select(b => b.Id == 2 ? yes : "no").ToList();
""");

//     [ConditionalFact]
//     public virtual Task Conditional_evaluatable_as_constant()
//         => Test(
//             """var blogs = context.Blogs.Select(b => Funcs.ReturnSame(4) == 3 ? "yes" : "no").ToList();""",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public string Name { get; set; }
// }
//
// public class Funcs
// {
//     public static T ReturnSame<T>(T p) => p;
// }
// """);
//
//     [ConditionalFact]
//     public virtual Task Conditional_evaluatable_as_parameter()
//         => Test("""
// var yes = "yes";
// var blogs = context.Blogs.Select(b => Funcs.ReturnSame(4) == 3 ? yes : "no").ToList();
//
// int foo() => 3;
// """,
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public string Name { get; set; }
// }
//
// public class Funcs
// {
//     public static T ReturnSame<T>(T p) => p;
// }
// """);
//
    // We do not support embedding Expression builder API calls into the query; this would require CSharpToLinqTranslator to actually
    // evaluate those APIs and embed the results into the tree. It's (at least potentially) a form of dynamic query, unsupported for now.
    [ConditionalFact]
    public virtual Task Invoke_no_evaluatability()
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => Test(
                """
Expression<Func<Blog, bool>> lambda = b => b.Name == "foo";
var parameter = Expression.Parameter(typeof(Blog), "b");

var blogs = context.Blogs
    .Where(Expression.Lambda<Func<Blog, bool>>(Expression.Invoke(lambda, parameter), parameter))
    .ToList();
"""));

     [ConditionalFact]
     public virtual Task ListInit_no_evaluatability()
         => Test("_ = context.Blogs.Select(b => new List<int> { b.Id, b.Id + 1 }).ToList();");

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_with_captured_variable()
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => Test(
                """
var i = 1;
_ = context.Blogs.Select(b => new List<int> { b.Id, i }).ToList();
"""));

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_without_captured_variable()
        => Test(
                """
var i = 1;
_ = context.Blogs.Select(b => new List<int> { b.Id, 8 }).ToList();
""");

    [ConditionalFact]
    public virtual Task ListInit_fully_evaluatable()
        => Test("""_ = context.Blogs.Select(b => new List<int> { 1, 2 }).ToList();""");

     [ConditionalFact]
     public virtual Task MethodCallExpression_no_evaluatability()
         => Test("_ = context.Blogs.Where(b => b.Name.StartsWith(b.Name)).ToList();");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_with_captured_variable()
        => Test("""
var pattern = "foo";
_ = context.Blogs.Where(b => b.Name.StartsWith(pattern)).ToList();
""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_without_captured_variable()
        => Test("""_ = context.Blogs.Where(b => b.Name.StartsWith("foo")).ToList();""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_fully_evaluatable()
        => Test("""_ = context.Blogs.Where(b => "foobar".StartsWith("foo")).ToList();""");

    [ConditionalFact]
    public virtual Task New_with_no_arguments()
        => Test(
            """
var i = 8;
_ = context.Blogs.Where(b => b == new Blog()).ToList();
""");

    // This doesn't translate via the regular pipeline either, but check the specific exception etc.
    // Note that the current behavior is to parameterize the NewExpression, not sure that's right for all cases.
    [ConditionalFact]
    public virtual Task Where_New_with_captured_variable()
        => Assert.ThrowsAsync<InvalidOperationException>(() => Test(
            """
var i = 8;
_ = context.Blogs.Where(b => b == new Blog(i, b.Name)).ToList();
"""));

    [ConditionalFact]
    public virtual Task Select_New_with_captured_variable()
        => Test(
            """
var i = 8;
_ = context.Blogs.Select(b => new Blog(i, b.Name)).ToList();
""");

    [ConditionalFact]
    public virtual Task MemberInit_no_evaluatable()
        => Test("_ = context.Blogs.Select(b => new Blog { Id = b.Id, Name = b.Name }).ToList();");

    [ConditionalFact]
    public virtual Task MemberInit_contains_captured_variable()
        => Test(
            """
var id = 8;
_ = context.Blogs.Select(b => new Blog { Id = id, Name = b.Name }).ToList();
""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_constant()
        => Test("""_ = context.Blogs.Select(b => new Blog { Id = 1, Name = "foo" }).ToList();""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_parameter()
        => Test(
            """
var id = 8;
var foo = "foo";
_ = context.Blogs.Select(b => new Blog { Id = id, Name = foo }).ToList();
""");

    [ConditionalFact]
    public virtual Task NewArray()
        => Test(
            """
var i = 8;
_ = context.Blogs.Select(b => new[] { b.Id, b.Id + i }).ToList();
""");

    [ConditionalFact]
    public virtual Task Unary()
        => Test("_ = context.Blogs.Where(b => (short)b.Id == (short)8).ToList();");

    #endregion Expression types

    #region Terminating operators

    [ConditionalFact]
    public virtual Task AsEnumerable()
        => Test("""
var blogs = context.Blogs.AsEnumerable();
foreach (var blog in blogs)
{
}
""");

    // Note that foreach/await foreach directly over DbSet properties doesn't get actually get precompiled, since we can't intercept
    // property accesses.
    [ConditionalFact]
    public virtual async Task Foreach_sync()
    {
        var exception = await Assert.ThrowsAsync<FailException>(
            () => Test(
                """
foreach (var blog in context.Blogs)
{
}
"""));
        Assert.Equal(NonCompilingQueryCompiler.ErrorMessage, exception.Message);
    }

    // Note that foreach/await foreach directly over DbSet properties doesn't get actually get precompiled, since we can't intercept
    // property accesses.
    [ConditionalFact]
    public virtual async Task Foreach_async()
    {
        var exception = await Assert.ThrowsAsync<FailException>(
            () => Test(
                """
await foreach (var blog in context.Blogs)
{
}
"""));
        Assert.Equal(NonCompilingQueryCompiler.ErrorMessage, exception.Message);
    }

    [ConditionalFact]
    public virtual Task Foreach_AsAsyncEnumerable()
        => Test("""
await foreach (var blog in context.Blogs.AsAsyncEnumerable())
{
}
""");

    [ConditionalFact]
    public virtual Task ToDictionary()
        => Test("_ = context.Blogs.Select(b => new { b.Id, b.Name }).ToDictionary(x => x.Id, x => x.Name);");

    [ConditionalFact]
    public virtual Task ToDictionaryAsync()
        => Test("_ = await context.Blogs.Select(b => new { b.Id, b.Name }).ToDictionaryAsync(x => x.Id, x => x.Name);");

    [ConditionalFact]
    public virtual Task ToList()
        => Test("_ = context.Blogs.ToList();");

    [ConditionalFact]
    public virtual Task ToListAsync()
        => Test("_ = await context.Blogs.ToListAsync();");

    // TODO: Go over all terminating operators in Enumerable/Queryable, EntityFrameworkQueryableExtensions etc.

    #endregion Terminating operators

    #region Reducing terminating operators

    [ConditionalFact]
    public virtual Task Max_without_selector()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var maxId = context.Blogs.Select(b => b.Id).Max();
Assert.Equal(9, maxId);
""");

    [ConditionalFact]
    public virtual Task Max_with_selector()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var maxId = context.Blogs.Max(b => b.Id);
Assert.Equal(9, maxId);
""");

    [ConditionalFact]
    public virtual Task MaxAsync_without_selector()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var maxId = await context.Blogs.Select(b => b.Id).MaxAsync();
Assert.Equal(9, maxId);
""");

    [ConditionalFact]
    public virtual Task MaxAsync_with_selector()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var maxId = await context.Blogs.MaxAsync(b => b.Id);
Assert.Equal(9, maxId);
""");

    [ConditionalFact]
    public virtual Task Sum_without_selector()
        => Test("_ = context.Blogs.Select(b => b.Id).Sum();");

    [ConditionalFact]
    public virtual Task Sum_with_selector()
        => Test("_ = context.Blogs.Sum(b => b.Id);");

    [ConditionalFact]
    public virtual Task SumAsync_without_selector()
        => Test("_ = await context.Blogs.Select(b => b.Id).SumAsync();");

    [ConditionalFact]
    public virtual Task SumAsync_with_selector()
        => Test("_ = await context.Blogs.SumAsync(b => b.Id);");

    [ConditionalFact]
    public virtual Task ExecuteDelete()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var rowsAffected = context.Blogs.Where(b => b.Id > 8).ExecuteDelete();
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync());
""");

    [ConditionalFact]
    public virtual Task ExecuteDeleteAsync()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var rowsAffected = await context.Blogs.Where(b => b.Id > 8).ExecuteDeleteAsync();
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync());
""");

    [ConditionalFact]
    public virtual Task ExecuteUpdate()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var suffix = "Suffix";
var rowsAffected = context.Blogs.Where(b => b.Id > 8).ExecuteUpdate(setters => setters.SetProperty(b => b.Name, b => b.Name + suffix));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "Blog2Suffix"));
""");

    [ConditionalFact]
    public virtual Task ExecuteUpdateAsync()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var suffix = "Suffix";
var rowsAffected = await context.Blogs.Where(b => b.Id > 8).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Name, b => b.Name + suffix));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "Blog2Suffix"));
""");

    #endregion Reducing terminating operators

    #region SQL expression quotability

    [ConditionalFact]
    public virtual Task Union()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var blogs = context.Blogs.Where(b => b.Id > 7)
    .Union(context.Blogs.Where(b => b.Id < 10))
    .OrderBy(b => b.Id)
    .ToList();

Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Concat()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var blogs = context.Blogs.Where(b => b.Id > 7)
    .Concat(context.Blogs.Where(b => b.Id < 10))
    .OrderBy(b => b.Id)
    .ToList();

Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Intersect()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var blogs = context.Blogs.Where(b => b.Id > 7)
    .Intersect(context.Blogs.Where(b => b.Id > 8))
    .OrderBy(b => b.Id)
    .ToList();

Assert.Collection(blogs, b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Except()
        => Test(
            """
context.Blogs.AddRange(
    new Blog { Id = 8, Name = "Blog1" },
    new Blog { Id = 9, Name = "Blog2" });
await context.SaveChangesAsync();

var blogs = context.Blogs.Where(b => b.Id > 7)
    .Except(context.Blogs.Where(b => b.Id > 8))
    .OrderBy(b => b.Id)
    .ToList();

Assert.Collection(blogs, b => Assert.Equal(8, b.Id));
""");

    [ConditionalFact]
    public virtual Task ValuesExpression()
        => Test("_ = context.Blogs.Where(b => new[] { 7, b.Id }.Count(i => i > 8) == 2).ToList();");

    // Tests e.g. OPENJSON on SQL Server
    [ConditionalFact]
    public virtual Task Contains_with_parameterized_collection()
        => Test(
            """
int[] ids = [1, 2, 3];
_ = context.Blogs.Where(b => ids.Contains(b.Id)).ToList();
""");

    // TODO: SQL Server-specific
    [ConditionalFact]
    public virtual Task FromSqlRaw()
        => Test("""_ = context.Blogs.FromSqlRaw("SELECT * FROM Blogs").OrderBy(b => b.Id).ToList();""");

    [ConditionalFact]
    public virtual Task FromSql_with_interpolated_parameters()
        => Test("""_ = await context.Blogs.FromSql($"SELECT * FROM Blogs").OrderBy(b => b.Id).ToListAsync();""");

    #endregion SQL expression quotability

    [ConditionalFact]
    public virtual Task Select_changes_type()
        => Test("_ = context.Blogs.Select(b => b.Name).ToList();");

    [ConditionalFact]
    public virtual Task OrderBy()
        => Test("_ = context.Blogs.OrderBy(b => b.Name).ToList();");

    [ConditionalFact]
    public virtual Task Project_anonymous_object()
        => Test("""_ = context.Blogs.Select(b => new { Foo = b.Name + "Foo" }).ToList();""");

    [ConditionalFact]
    public virtual Task Two_captured_variables()
        => Test("""
var yes = "yes";
var no = "no";
var blogs = context.Blogs.Select(b => b.Id == 3 ? yes : no).ToList();

int foo() => 3;
""");

    [ConditionalFact]
    public virtual Task Split_query()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task Final_GroupBy()
        => throw new NotImplementedException();

    [ConditionalFact]
    public virtual Task Foo()
        => Test(
            """
var id = 3;
var blogs = context.Blogs.Where(b => b.Id > id).Select(b => b.Name).ToList();
""");


    public class PrecompiledQueryContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
    }

    protected virtual Task Test(string sourceCode, [CallerMemberName] string callerName = "")
        => fixture.PrecompiledQueryTestHelpers.Test(
            """
await using var context = new PrecompiledQueryContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

""" + sourceCode,
            fixture.ServiceProvider.GetRequiredService<DbContextOptions>(),
            typeof(PrecompiledQueryContext),
            testOutputHelper,
            callerName);

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

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<Post> Post { get; set; } = new();
    }

    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public Blog? Blog { get; set; }
    }

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };
}
