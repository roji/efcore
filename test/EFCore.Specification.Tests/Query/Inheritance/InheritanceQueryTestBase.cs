// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class InheritanceQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceQueryFixtureBase, new()
{
    #region General querying

    [ConditionalFact]
    public virtual Task Query_root() // TODO: Abstract root?
        => AssertQuery(ss => ss.Set<Root>());

    [ConditionalFact]
    public virtual Task Query_intermediate() // TODO: Abstract intermediate?
        => AssertQuery(ss => ss.Set<Intermediate>());

    [ConditionalFact]
    public virtual Task Query_leaf1()
        => AssertQuery(ss => ss.Set<Leaf1>());

    [ConditionalFact]
    public virtual Task Query_leaf2()
        => AssertQuery(ss => ss.Set<Leaf2>());

    [ConditionalFact]
    public virtual Task Filter_root()
        => AssertQuery(ss => ss.Set<Root>().Where(r => r.RootInt == 8));

    [ConditionalFact]
    public virtual Task Project_scalar_from_leaf()
        => AssertQueryScalar(ss => ss.Set<Leaf1>().Select(k => k.Leaf1Int));

    [ConditionalFact]
    public virtual Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
        => AssertQuery(
            ss => ss.Set<Root>().Select(r => EF.Property<int>((Leaf1)r, nameof(Root.RootInt))),
            ss => ss.Set<Root>().Select(r => r.RootInt));

    [ConditionalFact]
    public virtual Task Project_scalar_from_root_via_leaf()
        => AssertQuery(
            ss => ss.Set<Leaf1>().Select(l => l.RootInt));

    [ConditionalFact]
    public virtual Task Project_scalar_from_root_via_root()
        => AssertQuery(
            ss => ss.Set<Root>().Select(r => r.RootInt));

    #endregion General querying

    #region OfType

    [ConditionalFact]
    public virtual Task OfType_root_via_root()
        => AssertQuery(ss => ss.Set<Root>().OfType<Root>());

    [ConditionalFact]
    public virtual Task OfType_root_via_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().OfType<Root>());

    // TODO: Exercise OrderBy discriminator?
    // [ConditionalFact]
    // public virtual Task Can_use_of_type_animal()
    //     => AssertQuery(
    //         ss => ss.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species),
    //         assertOrder: true);

    [ConditionalFact]
    public virtual Task OfType_intermediate()
        => AssertQuery(ss => ss.Set<Root>().OfType<Intermediate>());

    [ConditionalFact]
    public virtual Task OfType_leaf1()
        => AssertQuery(ss => ss.Set<Root>().OfType<Leaf1>());

    [ConditionalFact]
    public virtual Task OfType_leaf2()
        => AssertQuery(ss => ss.Set<Root>().OfType<Leaf2>());

    [ConditionalFact]
    public virtual Task OfType_leaf_with_predicate_on_leaf()
        => AssertQuery(
            ss => ss.Set<Root>().OfType<Leaf1>().Where(x => x.Leaf1Int == 1000));

    [ConditionalFact]
    public virtual Task OfType_leaf_with_predicate_on_root()
        => AssertQuery(
            ss => ss.Set<Root>().OfType<Leaf1>().Where(r => r.RootInt == 8));

    [ConditionalFact]
    public virtual Task Predicate_on_root_and_OfType_leaf()
        => AssertQuery(
            ss => ss.Set<Root>().Where(r => r.RootInt == 8).OfType<Leaf1>());

    [ConditionalFact]
    public virtual Task OfType_leaf_and_project_scalar()
        => AssertQuery(
            ss => ss.Set<Root>().OfType<Leaf1>().Select(l => l.Leaf1Int));

    [ConditionalFact]
    public virtual Task OfType_OrderBy_First()
        => AssertFirst(
            ss => ss.Set<Root>().OfType<Leaf1>().OrderBy(a => a.Leaf1Int));

    [ConditionalFact]
    public virtual Task OfType_in_subquery()
        => AssertQuery(
            ss => ss.Set<Intermediate>()
                .OrderBy(b => b.Id)
                .Take(5)
                .Distinct()
                .OfType<Leaf1>());

    #endregion OfType

    #region Is operator

    [ConditionalFact]
    public virtual Task Is_root_via_root()
        => AssertQuery(ss => ss.Set<Root>().Where(a => a is Root));

    [ConditionalFact]
    public virtual Task Is_root_via_leaf()
        => AssertQuery(
            // ReSharper disable once IsExpressionAlwaysTrue
            // ReSharper disable once ConvertTypeCheckToNullCheck
            ss => ss.Set<Leaf1>().Where(a => a is Root));

    [ConditionalFact]
    public virtual Task Is_intermediate()
        => AssertQuery(ss => ss.Set<Root>().Where(a => a is Intermediate));

    [ConditionalFact]
    public virtual Task Is_leaf()
        => AssertQuery(ss => ss.Set<Root>().Where(a => a is Leaf1));

    [ConditionalFact]
    public virtual Task Is_leaf_via_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().Where(a => a is Leaf1));

    [ConditionalFact]
    public virtual Task Is_in_projection()
        => AssertQueryScalar(
            ss => ss.Set<Root>().Select(a => a is Leaf1));

    [ConditionalFact]
    public virtual Task Is_with_other_predicate()
        => AssertQuery(
            ss => ss.Set<Root>().Where(a => a is Leaf1 && a.RootInt == 8));

    [ConditionalFact]
    public virtual Task Is_on_subquery_result()
        => AssertQuery(
            ss => ss.Set<Root>().Where(r1 =>
                ss.Set<Root>().FirstOrDefault(r2 => r2.UniqueId == r1.UniqueId) is Leaf1));

    #endregion Is operator

    // [ConditionalFact]
    // public virtual Task Can_query_all_animals()
    //     => AssertQuery(
    //         ss => ss.Set<Animal>().OrderBy(a => a.Species),
    //         assertOrder: true);

    // [ConditionalFact]
    // public virtual Task Can_query_all_plants()
    //     => AssertQuery(
    //         ss => ss.Set<Plant>().OrderBy(a => a.Species),
    //         assertOrder: true);

    // [ConditionalFact]
    // public virtual Task Can_query_all_birds()
    //     => AssertQuery(
    //         ss => ss.Set<Bird>().OrderBy(a => a.Species),
    //         assertOrder: true);

    // [ConditionalFact]
    // public virtual Task Can_query_just_kiwis()
    //     => AssertSingle(
    //         ss => ss.Set<Kiwi>());

    // [ConditionalFact]
    // public virtual Task Can_query_just_roses()
    //     => AssertSingle(
    //         ss => ss.Set<Rose>());

    #region Include

    [ConditionalFact]
    public virtual Task Include_root()
        => AssertQuery(
            ss => ss.Set<RootReferencingEntity>().Include(c => c.Root),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootReferencingEntity>(x => x.Root)));

    // [ConditionalFact]
    // public virtual Task Can_include_prey()
    //     => AssertSingle(
    //         ss => ss.Set<Eagle>().Include(e => e.Prey),
    //         asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Eagle>(x => x.Prey)));

    #endregion Include

    #region Discriminator

    [ConditionalFact]
    public virtual Task Filter_on_discriminator()
        => AssertQuery(
            ss => ss.Set<Root>().Where(b => EF.Property<string>(b, "Discriminator") == nameof(Leaf1)),
            ss => ss.Set<Root>().Where(b => b is Leaf1));

    [ConditionalFact]
    public virtual Task Project_discriminator()
        => AssertQuery(
            ss => ss.Set<Root>().Select(b => EF.Property<string>(b, "Discriminator")),
            ss => ss.Set<Root>().Select(b => b.GetType().Name));

    #endregion Discriminator

    // [ConditionalFact]
    // public virtual Task Can_insert_update_delete()
    // {
    //     int? eagleId = null;
    //     return TestHelpers.ExecuteWithStrategyInTransactionAsync(
    //         CreateContext,
    //         UseTransaction, async context =>
    //         {
    //             eagleId = (await context.Set<Bird>().AsNoTracking().SingleAsync(e => e.Species == "Aquila chrysaetos canadensis")).Id;

    //             var kiwi = new Kiwi
    //             {
    //                 Species = "Apteryx owenii",
    //                 Name = "Little spotted kiwi",
    //                 IsFlightless = true,
    //                 FoundOn = Island.North
    //             };

    //             var nz = await context.Set<Country>().SingleAsync(c => c.Id == 1);

    //             nz.Animals.Add(kiwi);

    //             await context.SaveChangesAsync();
    //         }, async context =>
    //         {
    //             var kiwi = await context.Set<Kiwi>().SingleAsync(k => k.Species.EndsWith("owenii"));

    //             kiwi.EagleId = eagleId;

    //             await context.SaveChangesAsync();
    //         }, async context =>
    //         {
    //             var kiwi = await context.Set<Kiwi>().SingleAsync(k => k.Species.EndsWith("owenii"));

    //             context.Set<Bird>().Remove(kiwi);

    //             await context.SaveChangesAsync();
    //         }, async context =>
    //         {
    //             var count = await context.Set<Kiwi>().CountAsync(k => k.Species.EndsWith("owenii"));

    //             Assert.Equal(0, count);
    //         });
    // }

    // [ConditionalFact]
    // public virtual async Task Setting_foreign_key_to_a_different_type_throws()
    // {
    //     using var context = CreateContext();
    //     var kiwi = await context.Set<Kiwi>().SingleAsync();

    //     var eagle = new Eagle
    //     {
    //         Species = "Haliaeetus leucocephalus",
    //         Name = "Bald eagle",
    //         Group = EagleGroup.Booted,
    //         EagleId = kiwi.Id
    //     };

    //     await context.AddAsync(eagle);

    //     // No fixup, because no principal with this key of the correct type is loaded.
    //     Assert.Empty(eagle.Prey);

    //     if (EnforcesFkConstraints)
    //     {
    //         // Relational database throws due to constraint violation
    //         await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
    //     }
    // }

    // [ConditionalFact]
    // public virtual async Task Member_access_on_intermediate_type_works()
    // {
    //     using var context = CreateContext();
    //     var query = context.Set<Kiwi>().Select(k => new Kiwi { Name = k.Name, Species = k.Species });

    //     var parameter = Expression.Parameter(query.ElementType, "p");
    //     var property = Expression.Property(parameter, "Name");
    //     var getProperty = Expression.Lambda(property, parameter);

    //     var expression = Expression.Call(
    //         typeof(Queryable), nameof(Queryable.OrderBy),
    //         [query.ElementType, typeof(string)], query.Expression, Expression.Quote(getProperty));

    //     query = query.Provider.CreateQuery<Kiwi>(expression);

    //     var result = await query.ToListAsync();

    //     var kiwi = Assert.Single(result);
    //     Assert.Equal("Great spotted kiwi", kiwi.Name);
    // }

    #region GetType

    [ConditionalFact]
    public virtual Task GetType_abstract_root()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => e.GetType() == typeof(Root)),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task GetType_abstract_intermediate()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => e.GetType() == typeof(Intermediate)),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task GetType_leaf1()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => e.GetType() == typeof(Leaf1)));

    [ConditionalFact]
    public virtual Task GetType_leaf2()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => e.GetType() == typeof(Leaf2)));

    [ConditionalFact]
    public virtual Task GetType_leaf_reverse_equality()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => typeof(Leaf1) == e.GetType()));

    [ConditionalFact]
    public virtual Task GetType_not_leaf1()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => typeof(Leaf1) != e.GetType()));

    #endregion GetType

    #region Union scenarios

    [ConditionalFact(Skip = "Issue#16298")] // TODO: Assert failure
    public virtual Task OfType_Union_OfType_on_same_type_Where()
        => AssertQuery(
            ss => ss.Set<Root>().OfType<Leaf1>()
                .Union(ss.Set<Root>().OfType<Leaf1>())
                .Where(o => o.Leaf1Int == 50));

    [ConditionalFact(Skip = "Issue#16298")] // TODO: Assert failure
    public virtual Task OfType_leaf_Union_intermediate_OfType_leaf()
        => AssertQuery(
            ss => ss.Set<Root>()
                .OfType<Leaf1>()
                .Union(ss.Set<Intermediate>())
                .OfType<Leaf1>());

    [ConditionalFact(Skip = "Issue#16298")]
    public virtual Task Union_siblings_with_duplicate_property_in_subquery()
        // Coke and Tea both have CaffeineGrams, which both need to be projected out on each side and so
        // requiring alias uniquification. They also have a different number of properties.
        => AssertQuery(
            ss => ss.Set<Coke>().Cast<Drink>()
                .Union(ss.Set<Tea>())
                .Where(d => d.SortIndex > 0));

    [ConditionalFact(Skip = "Issue#16298")]
    public virtual Task Union_entity_equality()
        => AssertQuery(
            ss => ss.Set<Leaf1>()
                .Union(ss.Set<Leaf2>().Cast<Intermediate>())
                .Where(b => b == null));

    #endregion Union scenarios

    [ConditionalFact]
    public virtual Task Conditional_with_is_and_downcast_in_projection()
        => AssertQuery(
            ss => ss.Set<Root>().Select(a => new { Value = a is Leaf1 ? ((Leaf1)a).Leaf1Int == 50 : default }));

    #region Filter on multiple types

    [ConditionalFact]
    public virtual Task Is_on_multiple_contradictory_types()
        => AssertQuery(
            ss => ss.Set<Root>().Where(e => e is Leaf1 && e is Leaf2),
            assertEmpty: true);

    [ConditionalFact]
    public virtual Task OfType_on_multiple_contradictory_types()
        => AssertTranslationFailed(() => AssertQuery(
            ss => ss.Set<Animal>().OfType<Eagle>().OfType<Kiwi>(),
            elementSorter: e => e.Name));

    [ConditionalFact]
    public virtual Task Is_and_OfType_with_multiple_contradictory_types()
        => AssertQuery(
            ss => ss.Set<Animal>().Where(e => e is Kiwi).OfType<Eagle>(),
            assertEmpty: true);

    #endregion Filter on multiple types

    [ConditionalFact]
    public virtual Task Primitive_collection_on_subtype()
        => AssertQuery(
            ss => ss.Set<Root>().Where(d => ((Leaf1)d).Ints!.Any()),
            ss => ss.Set<Root>().Where(d => d is Leaf1 && ((Leaf1)d).Ints != null && ((Leaf1)d).Ints!.Any()));

    protected InheritanceContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected virtual bool EnforcesFkConstraints
        => true;

    protected virtual void ClearLog()
    {
    }
}
