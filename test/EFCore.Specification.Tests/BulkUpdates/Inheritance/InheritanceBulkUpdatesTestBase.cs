// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class InheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture) : BulkUpdatesTestBase<TFixture>(fixture)
    where TFixture : InheritanceBulkUpdatesFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Delete_on_root()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => e.RootInt == 8),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_on_root_with_subquery()
        => AssertDelete(
            ss => ss.Set<Root>().Where(e => e.RootInt == 8).OrderBy(e => e.RootInt).Skip(0).Take(3),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_on_leaf()
        => AssertDelete(
            ss => ss.Set<Leaf1>().Where(e => e.Leaf1Int == 1000),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_entity_type_referencing_hierarchy()
        => AssertDelete(
            ss => ss.Set<RootReferencingEntity>().Where(e => e.Root!.RootInt == 8),
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
            rowsAffectedCount: 3);

    [ConditionalFact]
    public virtual Task Update_root()
        => AssertUpdate(
            ss => ss.Set<Root>().Where(e => e.RootInt == 8),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 1,
            (b, a) => a.ForEach(e => Assert.Equal(999, e.RootInt)));

    [ConditionalFact]
    public virtual Task Update_with_OfType_leaf()
        => AssertUpdate(
            ss => ss.Set<Root>().OfType<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 2,
            (b, a) => a.ForEach(e => Assert.Equal(999, e.RootInt)));

    [ConditionalTheory(Skip = "InnerJoin"), MemberData(nameof(IsAsyncData))]
    public virtual Task Update_root_with_subquery()
        => AssertUpdate(
            ss => ss.Set<Root>().Where(e => e.RootInt == 8).OrderBy(e => e.UniqueId).Skip(0).Take(3),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_root_property_on_leaf()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.RootInt, 999),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_leaf_property()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s.SetProperty(e => e.Leaf1Int, 999),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_both_root_and_leaf_properties()
        => AssertUpdate(
            ss => ss.Set<Leaf1>(),
            e => e,
            s => s
                .SetProperty(e => e.RootInt, 998)
                .SetProperty(e => e.Leaf1Int, 999),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_entity_type_referencing_hierarchy()
        => AssertUpdate(
            ss => ss.Set<RootReferencingEntity>().Where(e => e.Root!.RootInt == 8),
            e => e,
            s => s.SetProperty(e => e.Int, 999),
            rowsAffectedCount: 1);

    // [ConditionalFact]
    // public virtual Task Update_with_interface_in_property_expression()
    //     => AssertUpdate(
    //         ss => ss.Set<Coke>(),
    //         e => e,
    //         s => s.SetProperty(c => ((ISugary)c).SugarGrams, 0),
    //         rowsAffectedCount: 1);

    // [ConditionalFact]
    // public virtual Task Update_with_interface_in_EF_Property_in_property_expression()
    //     => AssertUpdate(
    //         ss => ss.Set<Coke>(),
    //         e => e,
    //         // ReSharper disable once RedundantCast
    //         s => s.SetProperty(c => EF.Property<int>((ISugary)c, nameof(ISugary.SugarGrams)), 0),
    //         rowsAffectedCount: 1);
}
