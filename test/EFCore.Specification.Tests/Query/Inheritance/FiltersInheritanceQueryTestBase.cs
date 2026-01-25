// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

// All the tests below run with a query filter excluding Root == 8
public abstract class FiltersInheritanceQueryTestBase<TFixture>(TFixture fixture) : FilteredQueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Query_leaf1()
        => AssertFilteredQuery(ss => ss.Set<Leaf1>());

    [ConditionalFact]
    public virtual Task OfType_root_via_root()
        => AssertFilteredQuery(ss => ss.Set<Root>().OfType<Root>());

    [ConditionalFact]
    public virtual Task OfType_leaf1()
        => AssertFilteredQuery(ss => ss.Set<Root>().OfType<Leaf1>());

    [ConditionalFact]
    public virtual Task OfType_leaf_and_project_scalar()
        => AssertFilteredQueryScalar(
            ss => ss.Set<Root>().OfType<Leaf1>().Select(l => l.Leaf1Int));

    [ConditionalFact]
    public virtual Task Predicate_on_root_and_OfType_leaf()
        => AssertFilteredQuery(
            ss => ss.Set<Root>().Where(r => r.RootInt == 8).OfType<Leaf1>(),
            assertEmpty: true); // Query filter specifically excludes RootInt == 8

    [ConditionalFact]
    public virtual Task Is_leaf()
        => AssertFilteredQuery(ss => ss.Set<Root>().Where(a => a is Leaf1));

    [ConditionalFact]
    public virtual Task Is_with_other_predicate()
        => AssertFilteredQuery(
            ss => ss.Set<Root>().Where(a => a is Leaf1 && a.RootInt == 8),
            assertEmpty: true); // Query filter specifically excludes RootInt == 8

    [ConditionalFact]
    public virtual Task Is_in_projection()
        => AssertFilteredQueryScalar(ss => ss.Set<Root>().Select(a => a is Leaf1));

    [ConditionalFact]
    public virtual async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        using var context = Fixture.CreateContext();

        var leaf = context.Set<Leaf1>().IgnoreQueryFilters().Single(l => l.RootInt == 8);

        Assert.Single(context.ChangeTracker.Entries());
        Assert.NotNull(await context.Entry(leaf).GetDatabaseValuesAsync());
    }
}
