// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

// All the tests below run with a query filter excluding RootInt == 8
public abstract class FiltersInheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture) : BulkUpdatesTestBase<TFixture>(fixture)
    where TFixture : InheritanceBulkUpdatesFixtureBase, new()
{
    // Note: Only the first Leaf1 row has RootInt == 8; when filters are enabled, this row is excluded from queries.
    // All tests below verify that operations with filters enabled do not affect this excluded row.
    // Data: Root[0]=Leaf1(RootInt=8), Root[1]=Leaf1(RootInt=9), Root[2]=Leaf2(RootInt=10), Root[3]=Leaf2(RootInt=9)
    // With filter active (RootInt!=8): 3 roots remain (Root[1], Root[2], Root[3])
    // 2 roots have RootInt=9 (Root[1]=Leaf1, Root[3]=Leaf2)
    // 1 Leaf1 remains (Root[1])

    [ConditionalFact]
    public virtual Task Delete_on_root()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => e.RootInt == 9),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Delete_on_root_with_subquery()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => e.RootInt == 9).OrderBy(e => e.RootInt).Skip(0).Take(3),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Delete_on_leaf()
        => AssertDelete(
            ss => ss.Set<Leaf1>().Where(e => e.Leaf1Int == 1001),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_entity_type_referencing_hierarchy()
        => AssertDelete(
            ss => ss.Set<RootReferencingEntity>().Where(e => e.Root!.RootInt == 9),
            rowsAffectedCount: 1);

    [ConditionalFact(Skip = "Issue#28525")]
    public virtual Task Delete_GroupBy_Where_Select_First()
        => AssertDelete(
            ss => ss.Set<Root>()
                .GroupBy(e => e.RootInt)
                .Where(g => g.Count() < 3)
                .Select(g => g.First()),
            rowsAffectedCount: 2);

    [ConditionalFact(Skip = "Issue#26753")]
    public virtual Task Delete_GroupBy_Where_Select_First_2()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => e
                == ss.Set<Root>().GroupBy(e => e.RootInt)
                    .Where(g => g.Count() < 3).Select(g => g.First()).FirstOrDefault()),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Delete_GroupBy_Where_Select_First_3()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => ss.Set<Root>().GroupBy(e => e.RootInt)
                .Where(g => g.Count() < 3).Select(g => g.First()).Any(i => i == e)),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_root()
        => AssertUpdate(
            ss => ss.Set<Root>().Where(e => e.RootInt == 9),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 2,
            (b, a) => a.ForEach(e => Assert.Equal(999, e.RootInt)));

    [ConditionalFact]
    public virtual Task Update_with_OfType_leaf()
        => AssertUpdate(
            ss => ss.Set<Root>().OfType<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 1,
            (b, a) => a.ForEach(e => Assert.Equal(999, e.RootInt)));

    [ConditionalTheory(Skip = "InnerJoin"), MemberData(nameof(IsAsyncData))]
    public virtual Task Update_root_with_subquery()
        => AssertUpdate(
            ss => ss.Set<Root>().Where(e => e.RootInt == 9).OrderBy(e => e.UniqueId).Skip(0).Take(3),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_root_property_on_leaf()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_leaf_property()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.Leaf1Int, 999),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_both_root_and_leaf_properties()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s
                .SetProperty(e => e.RootInt, 998)
                .SetProperty(e => e.Leaf1Int, 999),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_entity_type_referencing_hierarchy()
        => AssertUpdate(
            ss => ss.Set<RootReferencingEntity>().Where(e => e.Root!.RootInt == 9),
            e => e,
            s => s.SetProperty(e => e.Int, 999),
            rowsAffectedCount: 1);
}
