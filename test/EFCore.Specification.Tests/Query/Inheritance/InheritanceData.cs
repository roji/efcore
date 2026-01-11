// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class InheritanceData : ISetSource
{
    public static readonly InheritanceData Instance = new(useGeneratedKeys: false);
    public static readonly InheritanceData GeneratedKeysInstance = new(useGeneratedKeys: true);

    public IReadOnlyList<Root> Roots { get; }
    public IReadOnlyList<RootReferencingEntity> RootReferencingEntities { get; }

    public InheritanceData(bool useGeneratedKeys)
    {
        RootReferencingEntities = CreateRootReferencingEntities();
        Roots = CreateRoots(useGeneratedKeys);

        WireUp(Roots, RootReferencingEntities);

        // AnimalQueries = Animals.Select(a => a switch
        // {
        //     Eagle eagle => (AnimalQuery)new EagleQuery
        //     {
        //         Name = a.Name,
        //         CountryId = a.CountryId,
        //         EagleId = ((Bird)a).EagleId,
        //         IsFlightless = ((Bird)a).IsFlightless,
        //         Group = eagle.Group,
        //     },

        //     Kiwi kiwi => new KiwiQuery
        //     {
        //         Name = a.Name,
        //         CountryId = a.CountryId,
        //         EagleId = ((Bird)a).EagleId,
        //         IsFlightless = ((Bird)a).IsFlightless,
        //         FoundOn = ((Kiwi)a).FoundOn,
        //     },

        //     _ => throw new UnreachableException()
        // }).ToList();
    }

    public InheritanceData(
        IReadOnlyList<Root> roots,
        IReadOnlyList<RootReferencingEntity> rootReferencingEntities)
        // IReadOnlyList<AnimalQuery> animalQueries,
    {
        Roots = roots;
        RootReferencingEntities = rootReferencingEntities;

        // AnimalQueries = animalQueries;
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

    public static void WireUp(IReadOnlyList<Root> roots, IReadOnlyList<RootReferencingEntity> rootReferencingEntities)
    {
        rootReferencingEntities[0].Root = roots[0];
        roots[0].RootReferencingEntity = rootReferencingEntities[0];
    }
}
