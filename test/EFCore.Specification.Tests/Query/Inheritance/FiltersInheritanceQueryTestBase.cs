// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class FiltersInheritanceQueryTestBase<TFixture>(TFixture fixture) : FilteredQueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Can_use_of_type_animal()
        => AssertFilteredQuery(
            ss => ss.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Can_use_is_kiwi()
        => AssertFilteredQuery(ss => ss.Set<Animal>().Where(a => a is Kiwi));

    [ConditionalFact]
    public virtual Task Can_use_is_kiwi_with_other_predicate()
        => AssertFilteredQuery(ss => ss.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1));

    [ConditionalFact]
    public virtual Task Can_use_is_kiwi_in_projection()
        => AssertFilteredQueryScalar(ss => ss.Set<Animal>().Select(a => a is Kiwi));

    [ConditionalFact]
    public virtual Task Can_use_of_type_bird()
        => AssertFilteredQuery(
            ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Can_use_of_type_bird_predicate()
        => AssertFilteredQuery(
            ss => ss.Set<Animal>()
                .Where(a => a.CountryId == 1)
                .OfType<Bird>()
                .OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalFact]
    public virtual Task Can_use_of_type_bird_with_projection()
        => AssertFilteredQuery(
            ss => ss.Set<Animal>()
                .OfType<Bird>()
                .Select(b => new { b.Name }),
            elementSorter: e => e.Name);

    [ConditionalFact]
    public virtual Task Can_use_of_type_bird_first()
        => AssertFirst(ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species));

    [ConditionalFact]
    public virtual Task Can_use_of_type_kiwi()
        => AssertFilteredQuery(ss => ss.Set<Animal>().OfType<Kiwi>());

    [ConditionalFact]
    public virtual Task Can_use_derived_set()
        => AssertFilteredQuery(
            ss => ss.Set<Eagle>(),
            assertEmpty: true);

    [ConditionalFact]
    public virtual async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        using var context = Fixture.CreateContext();

        var eagle = context.Set<Eagle>().IgnoreQueryFilters().Single();

        Assert.Single(context.ChangeTracker.Entries());
        Assert.NotNull(await context.Entry(eagle).GetDatabaseValuesAsync());
    }
}
