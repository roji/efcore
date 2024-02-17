// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

public class PrecompiledQuerySqlServerTest(
    PrecompiledQuerySqlServerTest.PrecompiledQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : PrecompiledQueryRelationalTestBase(fixture, testOutputHelper),
        IClassFixture<PrecompiledQuerySqlServerTest.PrecompiledQuerySqlServerFixture>
{
    [ConditionalFact]
    public virtual Task Collate()
        => Test("""_ = context.Blogs.Where(b => EF.Functions.Collate(b.Name, "German_PhoneBook_CI_AS") == "foo").ToList();""");

    [ConditionalFact]
    public virtual Task SqlServerAggregateFunctionExpression()
        => Test(
            """
_ = context.Blogs
    .GroupBy(b => b.Id)
    .Select(g => string.Join(", ", g.OrderBy(b => b.Name).Select(b => b.Name)))
    .ToList();
""");

    // SqlServerOpenJsonExpression is covered by PrecompiledQueryRelationalTestBase.Contains_with_parameterized_collection

//     [ConditionalFact]
//     public virtual Task TableValuedFunctionExpression_toplevel()
//         => Test(
//             "_ = context.GetBlogsWithAtLeast(9).ToList();",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.HasDbFunction(typeof(BlogContext).GetMethod(nameof(GetBlogsWithAtLeast)));
//     }
//
//     public IQueryable<Blog> GetBlogsWithAtLeast(int minBlogId) => FromExpression(() => GetBlogsWithAtLeast(minBlogId));
// }
//
// public class Blog
// {
//     [DatabaseGenerated(DatabaseGeneratedOption.None)]
//     public int Id { get; set; }
//     public string StringProperty { get; set; }
// }
// """,
//             setupSql: """
// CREATE FUNCTION dbo.GetBlogsWithAtLeast(@minBlogId int)
// RETURNS TABLE AS RETURN
// (
//     SELECT [b].[Id], [b].[Name] FROM [Blogs] AS [b] WHERE [b].[Id] >= @minBlogId
// )
// """,
//             cleanupSql: "DROP FUNCTION dbo.GetBlogsWithAtLeast;");
//
//     [ConditionalFact]
//     public virtual Task TableValuedFunctionExpression_non_toplevel()
//         => Test(
//             "_ = context.Blogs.Where(b => context.GetPosts(b.Id).Count() == 2).ToList();",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.HasDbFunction(typeof(BlogContext).GetMethod(nameof(GetPosts)));
//     }
//
//     public IQueryable<Post> GetPosts(int blogId) => FromExpression(() => GetPosts(blogId));
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public string StringProperty { get; set; }
//     public List<Post> Post { get; set; }
// }
//
// public class Post
// {
//     public int Id { get; set; }
//     public string Title { get; set; }
//
//     public Blog Blog { get; set; }
// }
// """,
//             setupSql: """
// CREATE FUNCTION dbo.GetPosts(@blogId int)
// RETURNS TABLE AS RETURN
// (
//     SELECT [p].[Id], [p].[Title], [p].[BlogId] FROM [Posts] AS [p] WHERE [p].[BlogId] = @blogId
// )
// """,
//             cleanupSql: "DROP FUNCTION dbo.GetPosts;");

    public class PrecompiledQuerySqlServerFixture : PrecompiledQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            // TODO: Figure out if there's a nice way to continue using the retrying strategy
            var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
            sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
            return builder;
        }

        public override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers => SqlServerPrecompiledQueryTestHelpers.Instance;
    }
}
