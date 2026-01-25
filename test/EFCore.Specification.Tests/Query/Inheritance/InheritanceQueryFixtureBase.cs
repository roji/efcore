// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class InheritanceQueryFixtureBase : SharedStoreFixtureBase<InheritanceContext>, IFilteredQueryFixtureBase
{
    private readonly Dictionary<bool, ISetSource> _expectedDataCache = new();

    protected override string StoreName
        => "InheritanceTest";

    public virtual bool EnableFilters
        => false;

    public virtual bool IsDiscriminatorMappingComplete
        => true;

    public virtual bool HasDiscriminator
        => true;

    public virtual bool UseGeneratedKeys
        => true;

    public virtual bool EnableComplexTypes
        => true;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(
            CoreEventId.MappedEntityTypeIgnoredWarning,
            CoreEventId.MappedPropertyIgnoredWarning,
            CoreEventId.MappedNavigationIgnoredWarning));

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public virtual ISetSource GetExpectedData()
        => UseGeneratedKeys
            ? InheritanceData.GeneratedKeysInstance
            : InheritanceData.Instance;

    public virtual ISetSource GetFilteredExpectedData(DbContext context)
    {
        if (_expectedDataCache.TryGetValue(EnableFilters, out var cachedResult))
        {
            return cachedResult;
        }

        var expectedData = new InheritanceData(UseGeneratedKeys);
        if (EnableFilters)
        {
            var roots = expectedData.Roots.Where(a => a.RootInt != 8).ToList();
            expectedData = new InheritanceData(roots, expectedData.RootReferencingEntities);
        }

        _expectedDataCache[EnableFilters] = expectedData;

        return expectedData;
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, object>
    {
        { typeof(Root), object? (Root e) => e?.UniqueId },
        { typeof(Intermediate), object? (Intermediate e) => e?.UniqueId },
        { typeof(Leaf1), object? (Leaf1 e) => e?.UniqueId },
        { typeof(Leaf2), object? (Leaf2 e) => e?.UniqueId },
        { typeof(ConcreteIntermediate), object? (ConcreteIntermediate e) => e?.UniqueId },
        { typeof(Leaf3), object? (Leaf3 e) => e?.UniqueId },

        { typeof(ComplexType), object? (ComplexType e) => e?.UniqueId },
        { typeof(NestedComplexType), object? (NestedComplexType e) => e?.UniqueId },

        { typeof(RootReferencingEntity), object? (RootReferencingEntity e) => e?.Id }
    }.ToDictionary(e => e.Key, e => e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    public InheritanceQueryFixtureBase()
        => EntityAsserters = new Dictionary<Type, object>
        {
            [typeof(Root)] = (Root e, Root a)
                => NullSafeAssert<Root>(e, a, AssertRoot),
            [typeof(Intermediate)] = (Intermediate e, Intermediate a)
                => NullSafeAssert<Intermediate>(e, a, AssertIntermediate),
            [typeof(ConcreteIntermediate)] = (ConcreteIntermediate e, ConcreteIntermediate a)
                => NullSafeAssert<ConcreteIntermediate>(e, a, AssertConcreteIntermediate),

            [typeof(Leaf1)] = (Leaf1 e, Leaf1 a)
                => NullSafeAssert<Leaf1>(e, a, (e, a) =>
                {
                    AssertIntermediate(e, a);
                    Assert.Equal(e.Leaf1Int, a.Leaf1Int);
                    AssertComplexType(e.ChildComplexType, a.ChildComplexType);
                }),

            [typeof(Leaf2)] = (Leaf2 e, Leaf2 a)
                => NullSafeAssert<Leaf2>(e, a, (e, a) =>
                {
                    AssertIntermediate(e, a);
                    Assert.Equal(e.Leaf2Int, a.Leaf2Int);
                    AssertComplexType(e.ChildComplexType, a.ChildComplexType);
                }),

            [typeof(Leaf3)] = (Leaf3 e, Leaf3 a)
                => NullSafeAssert<Leaf3>(e, a, (e, a) =>
                {
                    AssertConcreteIntermediate(e, a);
                    Assert.Equal(e.Leaf3Int, a.Leaf3Int);
                }),

            [typeof(RootReferencingEntity)] = (RootReferencingEntity e, RootReferencingEntity a)
                => NullSafeAssert<RootReferencingEntity>(e, a, (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    NullSafeAssert<Root>(e.Root, a.Root, AssertRoot);
                }),

            [typeof(ComplexType)] = (ComplexType e, ComplexType a)
                => NullSafeAssert<ComplexType>(e, a, AssertComplexType),

            [typeof(NestedComplexType)] = (NestedComplexType e, NestedComplexType a)
                => NullSafeAssert<NestedComplexType>(e, a, AssertNestedComplexType)
        }.ToDictionary(e => e.Key, e => e.Value);

    private void NullSafeAssert<T>(object? e, object? a, Action<T, T> assertAction)
    {
        if (e is T ee && a is T aa)
        {
            assertAction(ee, aa);
            return;
        }

        Assert.Equal(e, a);
    }

    protected virtual void AssertRoot(Root e, Root a)
    {
        Assert.Equal(e.UniqueId, a.UniqueId);
        Assert.Equal(e.RootInt, a.RootInt);

        AssertComplexType(e.ParentComplexType, a.ParentComplexType);
        AssertComplexTypes(e.ComplexTypeCollection, a.ComplexTypeCollection);
    }

    protected virtual void AssertIntermediate(Intermediate e, Intermediate a)
    {
        AssertRoot(e, a);

        Assert.Equal(e.IntermediateInt, a.IntermediateInt);
    }

    protected virtual void AssertConcreteIntermediate(ConcreteIntermediate e, ConcreteIntermediate a)
    {
        AssertRoot(e, a);

        Assert.Equal(e.ConcreteIntermediateInt, a.ConcreteIntermediateInt);
    }

    private void AssertComplexType(ComplexType? e, ComplexType? a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.Equal(e is null, a is null);

        if (e is not null)
        {
            Assert.Equal(e.UniqueId, a!.UniqueId);
            Assert.Equal(e.Int, a.Int);

            Assert.Equal(e.Nested is null, a.Nested is null);
            if (e.Nested is not null)
            {
                AssertNestedComplexType(e.Nested, a.Nested);
            }
        }
    }

    private void AssertNestedComplexType(NestedComplexType? e, NestedComplexType? a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.Equal(e is null, a is null);

        if (e is not null)
        {
            Assert.Equal(e.UniqueId, a!.UniqueId);
            Assert.Equal(e.Int, a.Int);
        }
    }

    private void AssertComplexTypes(List<ComplexType> e, List<ComplexType> a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.NotNull(e);
        Assert.NotNull(a);

        Assert.Equal(e.Count, a.Count);
        for (var i = 0; i < e.Count; i++)
        {
            AssertComplexType(e[i], a[i]);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Root>();
        modelBuilder.Entity<Intermediate>();
        modelBuilder.Entity<ConcreteIntermediate>();
        modelBuilder.Entity<Leaf1>();
        modelBuilder.Entity<Leaf2>();
        modelBuilder.Entity<Leaf3>();

        // Note that the foreign key is on Root (Root is the dependent); this is to allow the foreign key to exist
        // with TPC, where the opposite direction isn't possible (multiple tables).
        // We configure the navigation to cascade deletes to allow exercising deletion.
        modelBuilder.Entity<RootReferencingEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Root>()
            .HasOne(e => e.RootReferencingEntity)
            .WithOne(r => r.Root)
            .HasForeignKey<Root>(e => e.RootReferencingEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        if (HasDiscriminator)
        {
            modelBuilder.Entity<Root>().HasDiscriminator<string>("Discriminator").IsComplete(IsDiscriminatorMappingComplete);

            // modelBuilder.Entity<Drink>()
            //     .HasDiscriminator(e => e.Discriminator)
            //     .HasValue<Drink>(DrinkType.Drink)
            //     .HasValue<Coke>(DrinkType.Coke)
            //     .HasValue<Lilt>(DrinkType.Lilt)
            //     .HasValue<Tea>(DrinkType.Tea)
            //     .IsComplete(IsDiscriminatorMappingComplete);
        }
        else
        {
            // modelBuilder.Entity<Root>().Ignore(e => e.Discriminator);
        }

        // modelBuilder.Entity<KiwiQuery>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);

        if (EnableFilters)
        {
            modelBuilder.Entity<Root>().HasQueryFilter(a => a.RootInt != 8);
        }

        // modelBuilder.Entity<AnimalQuery>().HasNoKey();
        // modelBuilder.Entity<BirdQuery>();
        // modelBuilder.Entity<KiwiQuery>();

        if (EnableComplexTypes)
        {
            modelBuilder.Entity<Root>(b =>
            {
                b.ComplexProperty(d => d.ParentComplexType);
                b.ComplexCollection(d => d.ComplexTypeCollection);
            });

            modelBuilder.Entity<Leaf1>().ComplexProperty(c => c.ChildComplexType);
            modelBuilder.Entity<Leaf2>().ComplexProperty(t => t.ChildComplexType);
        }
        else
        {
            modelBuilder.Entity<Root>(b =>
            {
                b.Ignore(d => d.ParentComplexType);
                b.Ignore(d => d.ComplexTypeCollection);
            });

            modelBuilder.Entity<Leaf1>().Ignore(c => c.ChildComplexType);
            modelBuilder.Entity<Leaf2>().Ignore(t => t.ChildComplexType);
        }
    }

    protected override Task SeedAsync(InheritanceContext context)
        => InheritanceContext.SeedAsync(context, UseGeneratedKeys);
}
