// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

/// <summary>
///     Contains tests exercising complex type support in inheritance scenarios.
/// </summary>
/// <remarks>
///     These are split out into their own class because (a) some providers don't support complex types (e.g. Cosmos, InMemory),
///     and (b) some providers have multiple mapping options for them (relational can map them via table splitting or JSON), and
///     so can extend this test class twice.
/// </remarks>
public abstract class InheritanceComplexTypesQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Filter_on_complex_type_property_on_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().Where(d => d.ChildComplexType!.Int == 9));

    [ConditionalFact]
    public virtual Task Filter_on_complex_type_property_on_root()
        => AssertQuery(ss => ss.Set<Root>().Where(d => d.ParentComplexType!.Int == 8));

    [ConditionalFact]
    public virtual Task Filter_on_nested_complex_type_property_on_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().Where(d => d.ChildComplexType!.Nested!.Int == 51));

    [ConditionalFact]
    public virtual Task Filter_on_nested_complex_type_property_on_root()
        => AssertQuery(ss => ss.Set<Root>().Where(d => d.ParentComplexType!.Nested!.Int == 50));

    [ConditionalFact]
    public virtual Task Project_complex_type_on_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().Select(d => d.ChildComplexType));

    [ConditionalFact]
    public virtual Task Project_complex_type_on_root()
        => AssertQuery(ss => ss.Set<Root>().Select(d => d.ParentComplexType));

    [ConditionalFact]
    public virtual Task Project_nested_complex_type_on_leaf()
        => AssertQuery(ss => ss.Set<Leaf1>().Select(d => d.ChildComplexType!.Nested));

    [ConditionalFact]
    public virtual Task Project_nested_complex_type_on_root()
        => AssertQuery(ss => ss.Set<Root>().Select(d => d.ParentComplexType!.Nested));

    [ConditionalFact]
    public virtual Task Subquery_over_complex_collection()
        => AssertQuery(
            ss => ss.Set<Root>().Where(d => d.ComplexTypeCollection.Count(c => c.Int > 59) == 2),
            ss => ss.Set<Root>().Where(d => d.ComplexTypeCollection != null && d.ComplexTypeCollection.Count(c => c.Int > 59) == 2));
}
