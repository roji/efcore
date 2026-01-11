// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class InheritanceContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Root> Roots { get; set; } = null!;
    public DbSet<RootReferencingEntity> RootReferencingEntities { get; set; } = null!;

    public DbSet<Animal> Animals { get; set; } = null!;
    public DbSet<AnimalQuery> AnimalQueries { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<Drink> Drinks { get; set; } = null!;
    public DbSet<Coke> Coke { get; set; } = null!;
    public DbSet<Lilt> Lilt { get; set; } = null!;
    public DbSet<Tea> Tea { get; set; } = null!;
    public DbSet<Plant> Plants { get; set; } = null!;

    public static Task SeedAsync(InheritanceContext context, bool useGeneratedKeys)
    {
        var rootReferencingEntities = InheritanceData.CreateRootReferencingEntities();
        var roots = InheritanceData.CreateRoots(useGeneratedKeys);

        InheritanceData.WireUp(roots, rootReferencingEntities);

        var animals = InheritanceData.CreateAnimals(useGeneratedKeys);
        var countries = InheritanceData.CreateCountries();
        var drinks = InheritanceData.CreateDrinks(useGeneratedKeys);
        var plants = InheritanceData.CreatePlants();

        InheritanceData.WireUp(animals, countries);

        context.Roots.AddRange(roots);
        context.RootReferencingEntities.AddRange(rootReferencingEntities);
        context.Animals.AddRange(animals);
        context.Countries.AddRange(countries);
        context.Drinks.AddRange(drinks);
        context.Plants.AddRange(plants);

        return context.SaveChangesAsync();
    }
}
