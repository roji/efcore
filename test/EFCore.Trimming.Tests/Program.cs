// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using EFCore.Trimming.Tests;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Execute any query to make sure the basic query pipeline works
_ = ctx.Blogs.Where(b => b.Name.StartsWith("foo")).ToList();

// _ = ctx.Blogs.ToList();

// _ = ctx.Set<Blog>().ToList();

Console.WriteLine("Database query executed successfully.");

public class BlogContext : DbContext
{
    public BlogContext()
        => Blogs = Set<Blog>();

    private static readonly string ConnectionString;

    public DbSet<Blog> Blogs { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Should be able to remove this.
        // modelBuilder.Entity<Blog>();
    }

    static BlogContext()
    {
        var builder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection) { InitialCatalog = "TrimmingTests" };

        ConnectionString = builder.ToString();
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}
