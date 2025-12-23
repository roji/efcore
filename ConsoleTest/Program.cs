// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new BlogContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

// _ = await context.Blogs
//     .Where(b => b.Posts.Single().ComplexThing.Foo == 8)
//     .ToListAsync();

_ = await context.Blogs
    .Select(b => b.Posts.SingleOrDefault()!.ComplexThing)
    .ToListAsync();

// _ = await context.Blogs
//     .Select(b => b.Posts.Single().ComplexThings)
//     .ToListAsync();

// _ = await context.Blogs
//     .Select(b => b.Posts.Single())
//     .ToListAsync();

// var complexThing = new ComplexThing { Foo = 8, Bar = 16 };

// _ = await context.Blogs
//     .Where(b => b.Posts.Single().ComplexThing == complexThing)
//     .ToListAsync();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer(Environment.GetEnvironmentVariable("Test__SqlServer__DefaultConnection"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>().ComplexProperty(b => b.ComplexThing);
        modelBuilder.Entity<Post>().ComplexCollection(b => b.ComplexThings, b => b.ToJson());
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Post> Posts { get; set; }
}

public class Post
{
    public int Id { get; set; }

    public ComplexThing ComplexThing { get; set; }
    public List<ComplexThing> ComplexThings { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}

public class ComplexThing
{
    public int Foo { get; set; }
    public int Bar { get; set; }
}
