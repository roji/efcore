// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new BlogContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

// _ = await context.Blogs
//     .Where(b => b.Collection.Single().NestedReference2.Bar == 8)
//     .ToListAsync();

// _ = await context.Blogs
//     .Select(b => b.Collection.Single().NestedReference2)
//     .ToListAsync();

_ = await context.Blogs
    .Select(b => b.Collection.Single().NestedReference2)
    .ToListAsync();

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
        // modelBuilder.Entity<Collection>().ComplexProperty(b => b.NestedReference2);

        // modelBuilder.Entity<Blog>().Navigation(b => b.Details).AutoInclude();
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    // public Reference Reference { get; set; }
    public List<Collection> Collection { get; set; }
}

public class Reference
{
    public int Id { get; set; }
    public int Foo { get; set; }

    // public NestedReference NestedReference { get; set; }

    // public int BlogId { get; set; }
    // public Blog Blog { get; set; }
}

public class NestedReference
{
    public int Id { get; set; }
    public int Bar { get; set; }

    public int ReferenceId { get; set; }
    public Reference Reference { get; set; }
}

public class Collection
{
    public int Id { get; set; }
    public string Title { get; set; }

    // public NestedCollection NestedCollection { get; set; }
    public NestedReference2 NestedReference2 { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}

public class NestedCollection
{
    public int Id { get; set; }
    public string Description { get; set; }

    public int CollectionId { get; set; }
    public Collection Collection { get; set; }
}

public class NestedReference2
{
    public int Id { get; set; }
    public int Bar { get; set; }

    public int CollectionId { get; set; }
    public Collection Collection { get; set; }
}
