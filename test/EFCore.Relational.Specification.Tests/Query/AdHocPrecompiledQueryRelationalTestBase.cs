// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

// TODO: Remove Select() to Id after JSON is supported in materialization
public abstract class AdHocPrecompiledQueryRelationalTestBase(ITestOutputHelper testOutputHelper) : NonSharedModelTestBase
{
    [ConditionalFact]
    public virtual async Task Index_no_evaluatability()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

var blogs = context.JsonEntities.Where(b => b.IntList[b.Id] == 2).Select(b => b.Id).ToList();
""",
        typeof(JsonContext),
        options);
    }

    [ConditionalFact]
    public virtual async Task Index_with_captured_variable()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

var id = 1;
var blogs = context.JsonEntities.Where(b => b.IntList[id] == 2).Select(b => b.Id).ToList();
""",
            typeof(JsonContext),
            options);
    }

    [ConditionalFact]
    public virtual async Task JsonScalar()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

_ = context.JsonEntities.Where(b => b.JsonThing.StringProperty == "foo").Select(b => b.Id).ToList();
""",
            typeof(JsonContext),
            options);
    }

    public class JsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonEntity> JsonEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<JsonEntity>().OwnsOne(j => j.JsonThing, n => n.ToJson());
    }

    public class JsonEntity
    {
        public int Id { get; set; }
        public List<int> IntList { get; set; }
        public JsonThing JsonThing { get; set; }
    }

    public class JsonThing
    {
        public string StringProperty { get; set; }
    }

//     [ConditionalFact]
//     public virtual Task JsonScalar()
//         => Test(
//             // TODO: Remove Select() to Id after JSON is supported in materialization
//             """_ = context.Blogs.Where(b => b.JsonThing.SomeProperty == "foo").Select(b => b.Id).ToList();""",
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
//         => modelBuilder.Entity<Blog>().OwnsOne(b => b.JsonThing, n => n.ToJson());
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public JsonThing JsonThing { get; set; }
// }
//
// public class JsonThing
// {
//     public string SomeProperty { get; set; }
// }
// """);

    protected virtual Task Test(
        string sourceCode,
        Type dbContextType,
        DbContextOptions dbContextOptions,
        [CallerMemberName] string callerName = "")
        => PrecompiledQueryTestHelpers.Test(sourceCode, dbContextOptions, dbContextType, testOutputHelper, callerName);

    protected abstract PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers { get; }

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddScoped<IQueryCompiler, NonCompilingQueryCompiler>();

    protected override string StoreName
        => "AdHocPrecompiledQueryTest";
}
