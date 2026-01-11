// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

#nullable disable

public abstract class InheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture) : BulkUpdatesTestBase<TFixture>(fixture)
    where TFixture : InheritanceBulkUpdatesFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Delete_where_hierarchy()
        => AssertDelete(
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_where_hierarchy_subquery()
        => AssertDelete(
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi").OrderBy(e => e.Name).Skip(0).Take(3),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_where_hierarchy_derived()
        => AssertDelete(
            ss => ss.Set<Kiwi>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Delete_where_using_hierarchy()
        => AssertDelete(
            ss => ss.Set<Country>().Where(e => e.Animals.Where(a => a.CountryId > 0).Count() > 0),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Delete_where_using_hierarchy_derived()
        => AssertDelete(
            ss => ss.Set<Country>().Where(e => e.Animals.OfType<Kiwi>().Where(a => a.CountryId > 0).Count() > 0),
            rowsAffectedCount: 1);

    [ConditionalFact(Skip = "Issue#28525")]
    public virtual Task Delete_GroupBy_Where_Select_First()
        => AssertDelete(
            ss => ss.Set<Animal>()
                .GroupBy(e => e.CountryId)
                .Where(g => g.Count() < 3)
                .Select(g => g.First()),
            rowsAffectedCount: 2);

    [ConditionalFact(Skip = "Issue#26753")]
    public virtual Task Delete_GroupBy_Where_Select_First_2()
        => AssertDelete(
            ss => ss.Set<Animal>().Where(e => e
                == ss.Set<Animal>().GroupBy(e => e.CountryId)
                    .Where(g => g.Count() < 3).Select(g => g.First()).FirstOrDefault()),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Delete_GroupBy_Where_Select_First_3()
        => AssertDelete(
            ss => ss.Set<Animal>().Where(e => ss.Set<Animal>().GroupBy(e => e.CountryId)
                .Where(g => g.Count() < 3).Select(g => g.First()).Any(i => i == e)),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_base_type()
        => AssertUpdate(
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi"),
            e => e,
            s => s.SetProperty(e => e.Name, "Animal"),
            rowsAffectedCount: 1,
            (b, a) => a.ForEach(e => Assert.Equal("Animal", e.Name)));

    [ConditionalFact]
    public virtual Task Update_base_type_with_OfType()
        => AssertUpdate(
            ss => ss.Set<Animal>().OfType<Kiwi>(),
            e => e,
            s => s.SetProperty(e => e.Name, "NewBird"),
            rowsAffectedCount: 1,
            (b, a) => a.ForEach(e => Assert.Equal("NewBird", e.Name)));

    [ConditionalTheory(Skip = "InnerJoin"), MemberData(nameof(IsAsyncData))]
    public virtual Task Update_where_hierarchy_subquery()
        => AssertUpdate(
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi").OrderBy(e => e.Name).Skip(0).Take(3),
            e => e,
            s => s.SetProperty(e => e.Name, "Animal"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_base_property_on_derived_type()
        => AssertUpdate(
            ss => ss.Set<Kiwi>(),
            e => e,
            s => s.SetProperty(e => e.Name, "SomeOtherKiwi"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_derived_property_on_derived_type()
        => AssertUpdate(
            ss => ss.Set<Kiwi>(),
            e => e,
            s => s.SetProperty(e => e.FoundOn, Island.North),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_base_and_derived_types()
        => AssertUpdate(
            ss => ss.Set<Kiwi>(),
            e => e,
            s => s
                .SetProperty(e => e.Name, "Kiwi")
                .SetProperty(e => e.FoundOn, Island.North),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_where_using_hierarchy()
        => AssertUpdate(
            ss => ss.Set<Country>().Where(e => e.Animals.Where(a => a.CountryId > 0).Count() > 0),
            e => e,
            s => s.SetProperty(e => e.Name, "Monovia"),
            rowsAffectedCount: 2);

    [ConditionalFact]
    public virtual Task Update_where_using_hierarchy_derived()
        => AssertUpdate(
            ss => ss.Set<Country>().Where(e => e.Animals.OfType<Kiwi>().Where(a => a.CountryId > 0).Count() > 0),
            e => e,
            s => s.SetProperty(e => e.Name, "Monovia"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_with_interface_in_property_expression()
        => AssertUpdate(
            ss => ss.Set<Coke>(),
            e => e,
            s => s.SetProperty(c => ((ISugary)c).SugarGrams, 0),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_with_interface_in_EF_Property_in_property_expression()
        => AssertUpdate(
            ss => ss.Set<Coke>(),
            e => e,
            // ReSharper disable once RedundantCast
            s => s.SetProperty(c => EF.Property<int>((ISugary)c, nameof(ISugary.SugarGrams)), 0),
            rowsAffectedCount: 1);
}
