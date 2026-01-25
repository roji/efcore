// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class InheritanceQueryCosmosTest : InheritanceQueryTestBase<InheritanceQueryCosmosFixture>
{
    public InheritanceQueryCosmosTest(InheritanceQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Query_root()
    {
        await base.Query_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task Query_intermediate()
    {
        await base.Query_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Intermediate", "Leaf1", "Leaf2")
""");
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task Query_leaf2()
    {
        await base.Query_leaf2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Leaf2")
""");
    }

    public override async Task Filter_root()
    {
        await base.Filter_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["RootInt"] = 8))
""");
    }

    public override async Task Project_scalar_from_leaf()
    {
        await base.Project_scalar_from_leaf();

        AssertSql(
            """
SELECT VALUE c["Leaf1Int"]
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
    {
        await base.Project_root_scalar_via_root_with_EF_Property_and_downcast();

        AssertSql(
            """
SELECT VALUE c["RootInt"]
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task Project_scalar_from_root_via_leaf()
    {
        await base.Project_scalar_from_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c["RootInt"]
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task Project_scalar_from_root_via_root()
    {
        await base.Project_scalar_from_root_via_root();

        AssertSql(
            """
SELECT VALUE c["RootInt"]
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task OfType_root_via_leaf()
    {
        await base.OfType_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task OfType_intermediate()
    {
        await base.OfType_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND c["Discriminator"] IN ("Intermediate", "Leaf1", "Leaf2"))
""");
    }

    public override async Task OfType_leaf1()
    {
        await base.OfType_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task OfType_leaf2()
    {
        await base.OfType_leaf2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf2"))
""");
    }

    public override async Task OfType_leaf_with_predicate_on_leaf()
    {
        await base.OfType_leaf_with_predicate_on_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1")) AND (c["Leaf1Int"] = 1000))
""");
    }

    public override async Task OfType_leaf_with_predicate_on_root()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1")) AND (c["RootInt"] = 8))
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.Predicate_on_root_and_OfType_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["RootInt"] = 8)) AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT VALUE c["Leaf1Int"]
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task OfType_OrderBy_First()
    {
        await base.OfType_OrderBy_First();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
ORDER BY c["Leaf1Int"]
OFFSET 0 LIMIT 1
""");
    }

    public override Task OfType_in_subquery()
        => AssertTranslationFailedWithDetails(
            () => base.OfType_in_subquery(),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Is_root_via_root()
    {
        await base.Is_root_via_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task Is_root_via_leaf()
    {
        await base.Is_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task Is_intermediate()
    {
        await base.Is_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND c["Discriminator"] IN ("Intermediate", "Leaf1", "Leaf2"))
""");
    }

    public override async Task Is_leaf_via_root()
    {
        await base.Is_leaf_via_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task Is_leaf_via_leaf()
    {
        await base.Is_leaf_via_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Leaf1")
""");
    }

    public override async Task Is_in_projection()
    {
        await base.Is_in_projection();

        AssertSql(
            """
SELECT VALUE (c["Discriminator"] = "Leaf1")
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND ((c["Discriminator"] = "Leaf1") AND (c["RootInt"] = 8)))
""");
    }

    [ConditionalFact(Skip = "Issue#17246 subquery usage")]
    public override Task Is_on_subquery_result()
        => base.Is_on_subquery_result();

    [ConditionalFact(Skip = "Issue#17246 Non-embedded Include")]
    public override Task Include_root()
        => base.Include_root();

    public override async Task Filter_on_discriminator()
    {
        await base.Filter_on_discriminator();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task Project_discriminator()
    {
        await base.Project_discriminator();

        AssertSql(
            """
SELECT VALUE c["Discriminator"]
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task GetType_abstract_root()
    {
        await base.GetType_abstract_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Root"))
""");
    }

    public override async Task GetType_abstract_intermediate()
    {
        await base.GetType_abstract_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Intermediate"))
""");
    }

    public override async Task GetType_leaf1()
    {
        await base.GetType_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task GetType_leaf2()
    {
        await base.GetType_leaf2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf2"))
""");
    }

    public override async Task GetType_leaf_reverse_equality()
    {
        await base.GetType_leaf_reverse_equality();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] = "Leaf1"))
""");
    }

    public override async Task GetType_not_leaf1()
    {
        await base.GetType_not_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (c["Discriminator"] != "Leaf1"))
""");
    }

    public override async Task OfType_Union_OfType_on_same_type_Where()
    {
        await base.OfType_Union_OfType_on_same_type_Where();

        AssertSql();
    }

    public override async Task OfType_leaf_Union_intermediate_OfType_leaf()
    {
        await base.OfType_leaf_Union_intermediate_OfType_leaf();

        AssertSql();
    }

    public override async Task Union_entity_equality()
    {
        await base.Union_entity_equality();

        AssertSql();
    }

    public override async Task Conditional_with_is_and_downcast_in_projection()
    {
        await base.Conditional_with_is_and_downcast_in_projection();

        AssertSql(
            """
SELECT VALUE ((c["Discriminator"] = "Leaf1") ? (c["Leaf1Int"] = 50) : false)
FROM root c
WHERE c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2")
""");
    }

    public override async Task Is_on_multiple_contradictory_types()
    {
        await base.Is_on_multiple_contradictory_types();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND ((c["Discriminator"] = "Leaf1") AND (c["Discriminator"] = "Leaf2")))
""");
    }

    public override async Task Primitive_collection_on_subtype()
    {
        await base.Primitive_collection_on_subtype();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Root", "ConcreteIntermediate", "Intermediate", "Leaf3", "Leaf1", "Leaf2") AND (ARRAY_LENGTH(c["Ints"]) > 0))
""");
    }

    protected override bool EnforcesFkConstraints
        => false;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}

