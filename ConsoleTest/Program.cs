// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var context = new BlogContext();
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

context.Blogs.Add(new()
{
    Name = "Test Blog",
    Reference = new Reference
    {
        Foo = 8,
        NestedReference = new NestedReference
        {
            Bar = 16
        }
    },
    Collection = new List<Collection>
    {
        new Collection
        {
            Title = "foo",
            NestedCollection = new NestedCollection
            {
                Description = "First"
            }
        },
        new Collection
        {
            Title = "bar",
            NestedCollection = new NestedCollection
            {
                Description = "Second"
            }
        }
    }
});
await context.SaveChangesAsync();

// _ = await context.Blogs.Where(b => b.Details.Title == "EF Core").ToListAsync();
// _ = await context.Blogs.Include(b => b.Details).ThenInclude(d => d.DoubleNested).ToListAsync();

// _ = await context.Blogs.Include(b => b.Reference).Where(b => b.Reference.Foo == 8).Distinct().ToListAsync();
// _ = await context.Blogs.Include(b => b.Reference).Where(b => b.Reference.Foo == 8).ToListAsync();

// Don't have two JOINs when the same shaper is projected twice (maybe do both reference and collection)
var results = await context.Blogs
    .Include(x => x.Reference)
    .Select(x => new { X = x, Y = x })
    .ToListAsync();

// _ = await context.Blogs
//     .Where(b => b.Reference.Foo == 8)
//     .OrderBy(b => b.Id)
//     .Take(1)
//     .Where(b => b.Reference.Id == 9)
//     .ToListAsync();

    // .Include(b => b.Collection)
    // .Select(b => new
    // {
    //     X = b,
    //     Y = b.Reference.Blog
    // })
    // .ThenInclude(b => b.NestedCollection)
    // .Select(b => new
    // {
    //     X = b.Collection.Where(c => c.Title == "foo").Distinct().ToList(),
    //     Y = b.Collection.Where(c => c.Title == "bar").ToList()
    // })
    // .Select(b => b.Collection)

// WORKS
// _ = await context.Blogs.Include(b => b.Details).ToListAsync();
// _ = await context.Blogs.Where(b => b.Details.Sum(i => i.Id) == 8).ToListAsync();

;

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
        // modelBuilder.Entity<Blog>().Navigation(b => b.Details).AutoInclude();
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Reference Reference { get; set; }
    public List<Collection> Collection { get; set; }
}

public class Reference
{
    public int Id { get; set; }
    public int Foo { get; set; }

    public NestedReference NestedReference { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
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

    public NestedCollection NestedCollection { get; set; }

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

