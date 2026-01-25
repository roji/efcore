// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class InheritanceContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Root> Roots { get; set; } = null!;
    public DbSet<RootReferencingEntity> RootReferencingEntities { get; set; } = null!;

    public static Task SeedAsync(InheritanceContext context, bool useGeneratedKeys)
    {
        var rootReferencingEntities = InheritanceData.CreateRootReferencingEntities();
        var roots = InheritanceData.CreateRoots(useGeneratedKeys);

        InheritanceData.WireUp(roots, rootReferencingEntities);

        context.Roots.AddRange(roots);
        context.RootReferencingEntities.AddRange(rootReferencingEntities);

        return context.SaveChangesAsync();
    }
}
