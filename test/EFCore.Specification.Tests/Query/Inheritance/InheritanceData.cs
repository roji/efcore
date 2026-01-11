// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class InheritanceData : ISetSource
{
    public static readonly InheritanceData Instance = new(useGeneratedKeys: false);
    public static readonly InheritanceData GeneratedKeysInstance = new(useGeneratedKeys: true);

    public IReadOnlyList<Root> Roots { get; }
    public IReadOnlyList<RootReferencingEntity> RootReferencingEntities { get; }

    public IReadOnlyList<Animal> Animals { get; }
    public IReadOnlyList<AnimalQuery> AnimalQueries { get; }
    public IReadOnlyList<Country> Countries { get; }
    public IReadOnlyList<Drink> Drinks { get; }
    public IReadOnlyList<Plant> Plants { get; }

    public InheritanceData(bool useGeneratedKeys)
    {
        RootReferencingEntities = CreateRootReferencingEntities();
        Roots = CreateRoots(useGeneratedKeys);

        WireUp(Roots, RootReferencingEntities);

        Animals = CreateAnimals(useGeneratedKeys);
        Countries = CreateCountries();
        Drinks = CreateDrinks(useGeneratedKeys);
        Plants = CreatePlants();

        WireUp(Animals, Countries);

        AnimalQueries = Animals.Select(a => a switch
        {
            Eagle eagle => (AnimalQuery)new EagleQuery
            {
                Name = a.Name,
                CountryId = a.CountryId,
                EagleId = ((Bird)a).EagleId,
                IsFlightless = ((Bird)a).IsFlightless,
                Group = eagle.Group,
            },

            Kiwi kiwi => new KiwiQuery
            {
                Name = a.Name,
                CountryId = a.CountryId,
                EagleId = ((Bird)a).EagleId,
                IsFlightless = ((Bird)a).IsFlightless,
                FoundOn = ((Kiwi)a).FoundOn,
            },

            _ => throw new UnreachableException()
        }).ToList();
    }

    public InheritanceData(
        IReadOnlyList<Root> roots,
        IReadOnlyList<RootReferencingEntity> rootReferencingEntities,
        IReadOnlyList<Animal> animals,
        IReadOnlyList<AnimalQuery> animalQueries,
        IReadOnlyList<Country> countries,
        IReadOnlyList<Drink> drinks,
        IReadOnlyList<Plant> plants)
    {
        Roots = roots;
        RootReferencingEntities = rootReferencingEntities;

        Animals = animals;
        AnimalQueries = animalQueries;
        Countries = countries;
        Drinks = drinks;
        Plants = plants;
    }

    public virtual IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(Root))
        {
            return (IQueryable<TEntity>)Roots.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Intermediate))
        {
            return (IQueryable<TEntity>)Roots.OfType<Intermediate>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Leaf1))
        {
            return (IQueryable<TEntity>)Roots.OfType<Leaf1>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Leaf2))
        {
            return (IQueryable<TEntity>)Roots.OfType<Leaf2>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(ConcreteIntermediate))
        {
            return (IQueryable<TEntity>)Roots.OfType<ConcreteIntermediate>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Leaf3))
        {
            return (IQueryable<TEntity>)Roots.OfType<Leaf3>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(RootReferencingEntity))
        {
            return (IQueryable<TEntity>)RootReferencingEntities.AsQueryable();
        }

        // TODO: Remove
        if (typeof(TEntity) == typeof(Animal))
        {
            return (IQueryable<TEntity>)Animals.AsQueryable();
        }

        if (typeof(TEntity) == typeof(AnimalQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Bird))
        {
            return (IQueryable<TEntity>)Animals.OfType<Bird>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(BirdQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<BirdQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Eagle))
        {
            return (IQueryable<TEntity>)Animals.OfType<Eagle>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(EagleQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<EagleQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Kiwi))
        {
            return (IQueryable<TEntity>)Animals.OfType<Kiwi>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(KiwiQuery))
        {
            return (IQueryable<TEntity>)AnimalQueries.OfType<KiwiQuery>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Country))
        {
            return (IQueryable<TEntity>)Countries.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Drink))
        {
            return (IQueryable<TEntity>)Drinks.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Coke))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Coke>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Lilt))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Lilt>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Tea))
        {
            return (IQueryable<TEntity>)Drinks.OfType<Tea>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Plant))
        {
            return (IQueryable<TEntity>)Plants.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Flower))
        {
            return (IQueryable<TEntity>)Plants.OfType<Flower>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Daisy))
        {
            return (IQueryable<TEntity>)Plants.OfType<Daisy>().AsQueryable();
        }

        if (typeof(TEntity) == typeof(Rose))
        {
            return (IQueryable<TEntity>)Plants.OfType<Rose>().AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<Root> CreateRoots(bool useGeneratedKeys)
    {
        var incrementingId = 1;

        return
        [
            new Leaf1
            {
                Id = useGeneratedKeys ? 0 : incrementingId++,
                UniqueId = incrementingId,
                RootInt = 8,
                IntermediateInt = 100,
                Leaf1Int = 1000,
                Ints = [8, 9],

                ParentComplexType = new ComplexType
                {
                    UniqueId = incrementingId++,
                    Int = 8,
                    Nested = new NestedComplexType
                    {
                        UniqueId = incrementingId++,
                        Int = 50
                    }
                },
                ChildComplexType = new ComplexType
                {
                    UniqueId = incrementingId++,
                    Int = 9,
                    Nested = new NestedComplexType { UniqueId = incrementingId++, Int = 51 }
                },
                ComplexTypeCollection =
                [
                    new ComplexType { UniqueId = incrementingId++, Int = 52 },
                    new ComplexType { UniqueId = incrementingId++, Int = 53 }
                ]
            },

            new Leaf1
            {
                Id = useGeneratedKeys ? 0 : incrementingId++,
                UniqueId = incrementingId,
                RootInt = 9,
                IntermediateInt = 101,
                Leaf1Int = 1001,
                Ints = [10, 11]
            },

            new Leaf2
            {
                Id = useGeneratedKeys ? 0 : incrementingId++,
                UniqueId = incrementingId,
                RootInt = 10,
                IntermediateInt = 102,
                Leaf2Int = 1002,

                ChildComplexType = new ComplexType
                {
                    UniqueId = incrementingId++,
                    Int = 10,
                    Nested = new NestedComplexType { UniqueId = incrementingId++, Int = 58 }
                },
                ComplexTypeCollection =
                [
                    new ComplexType { UniqueId = incrementingId++, Int = 59 },
                    new ComplexType { UniqueId = incrementingId++, Int = 60 },
                    new ComplexType { UniqueId = incrementingId++, Int = 61 }
                ]
            },

            // A Leaf2 that's identical to the second Leaf1 in all but type
            new Leaf2
            {
                Id = useGeneratedKeys ? 0 : incrementingId++,
                UniqueId = incrementingId,
                RootInt = 9,
                IntermediateInt = 101,
                Leaf2Int = 1001
            }
        ];
    }

    public static IReadOnlyList<RootReferencingEntity> CreateRootReferencingEntities() =>
    [
        new()
        {
            Id = 1,
            Int = 42
        },
        new()
        {
            Id = 2,
            Int = 43
        }
    ];

    public static IReadOnlyList<Animal> CreateAnimals(bool useGeneratedKeys) =>
    [
        new Kiwi
        {
            Id = useGeneratedKeys ? 0 : 1,
            Species = "Apteryx haastii",
            Name = "Great spotted kiwi",
            IsFlightless = true,
            FoundOn = Island.South
        },
        new Eagle
        {
            Id = useGeneratedKeys ? 0 : 2,
            Species = "Aquila chrysaetos canadensis",
            Name = "American golden eagle",
            Group = EagleGroup.Booted
        },
    ];

    public static IReadOnlyList<Country> CreateCountries() =>
    [
        new() { Id = 1, Name = "New Zealand" }, new() { Id = 2, Name = "USA" },
    ];

    public static IReadOnlyList<Drink> CreateDrinks(bool useGeneratedKeys) =>
    [
        // new Tea
        // {
        //     Id = useGeneratedKeys ? 0 : 1,
        //     SortIndex = 1,
        //     HasMilk = true,
        //     CaffeineGrams = 1,
        //     ParentComplexType = new ComplexType
        //     {
        //         UniqueId = 1,
        //         ComplexTypeInt = 8,
        //         NestedComplexType = new NestedComplexType
        //         {
        //             UniqueId = 2,
        //             NestedComplexTypeInt = 50
        //         }
        //     },
        //     ChildComplexType = new ComplexType
        //     {
        //         UniqueId = 3,
        //         ComplexTypeInt = 9,
        //         NestedComplexType = new NestedComplexType { UniqueId = 4, NestedComplexTypeInt = 51 }
        //     },
        //     ComplexTypeCollection =
        //     [
        //         new ComplexType { UniqueId = 5, ComplexTypeInt = 52 },
        //         new ComplexType { UniqueId = 6, ComplexTypeInt = 53 }
        //     ]
        // },
        // new Lilt
        // {
        //     Id = useGeneratedKeys ? 0 : 2,
        //     SortIndex = 2,
        //     SugarGrams = 4,
        //     Carbonation = 7
        // },
        // new Coke
        // {
        //     Id = useGeneratedKeys ? 0 : 3,
        //     SortIndex = 3,
        //     SugarGrams = 6,
        //     CaffeineGrams = 4,
        //     Carbonation = 5,
        //     Ints = [8, 9],
        //     ChildComplexType = new ComplexType
        //     {
        //         UniqueId = 100,
        //         ComplexTypeInt = 10,
        //         NestedComplexType = new NestedComplexType { UniqueId = 101, NestedComplexTypeInt = 58 }
        //     },
        //     ComplexTypeCollection =
        //     [
        //         new ComplexType { UniqueId = 102, ComplexTypeInt = 59 },
        //         new ComplexType { UniqueId = 103, ComplexTypeInt = 60 },
        //         new ComplexType { UniqueId = 104, ComplexTypeInt = 61 }
        //     ]
        // }
    ];

    public static IReadOnlyList<Plant> CreatePlants() =>
    [
        new Rose
        {
            Genus = PlantGenus.Rose,
            Species = "Rosa canina",
            Name = "Dog-rose",
            HasThorns = true
        },
        new Daisy
        {
            Genus = PlantGenus.Daisy,
            Species = "Bellis perennis",
            Name = "Common daisy"
        },
        new Daisy
        {
            Genus = PlantGenus.Daisy,
            Species = "Bellis annua",
            Name = "Annual daisy"
        }
    ];

    public static void WireUp(IReadOnlyList<Root> roots, IReadOnlyList<RootReferencingEntity> rootReferencingEntities)
    {
        rootReferencingEntities[0].Root = roots[0];
        roots[0].RootReferencingEntity = rootReferencingEntities[0];
    }

    public static void WireUp(
        IReadOnlyList<Animal> animals,
        IReadOnlyList<Country> countries)
    {
        ((Eagle)animals[1]).Prey.Add((Bird)animals[0]);

        countries[0].Animals.Add(animals[0]);
        animals[0].CountryId = countries[0].Id;

        countries[1].Animals.Add(animals[1]);
        animals[1].CountryId = countries[1].Id;
    }
}
