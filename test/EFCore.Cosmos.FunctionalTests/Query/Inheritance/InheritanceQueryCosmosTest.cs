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

    public override async Task OfType_on_multiple_contradictory_types()
    {
        await base.OfType_on_multiple_contradictory_types();

        AssertSql();
    }

    public override async Task Can_query_when_shared_column()
    {
        await base.Can_query_when_shared_column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = 1)
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = 2)
OFFSET 0 LIMIT 2
""",
            //
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = 3)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Query_root()
    {
        await base.Query_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN (0, 1, 2, 3)
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
    }

    public override async Task Can_use_is_kiwi()
    {
        await base.Can_use_is_kiwi();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task Conditional_with_is_and_downcast_in_projection()
    {
        await base.Conditional_with_is_and_downcast_in_projection();

        AssertSql(
            """
SELECT VALUE ((c["Discriminator"] = "Kiwi") ? c["FoundOn"] : 0)
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Is_root_via_leaf()
    {
        await base.Is_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND ((c["Discriminator"] = "Kiwi") AND (c["CountryId"] = 1)))
""");
    }

    public override async Task Is_in_projection()
    {
        await base.Is_in_projection();

        AssertSql(
            """
SELECT VALUE (c["Discriminator"] = "Kiwi")
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task OfType_intermediate()
    {
        await base.OfType_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["CountryId"] = 1)) AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT VALUE c["EagleId"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
""");
    }

    public override async Task OfType_OrderBy_First()
    {
        await base.OfType_OrderBy_First();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
OFFSET 0 LIMIT 1
""");
    }

    public override async Task Can_use_of_type_kiwi()
    {
        await base.Can_use_of_type_kiwi();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task OfType_root_via_leaf()
    {
        await base.OfType_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
    }

    public override async Task Can_use_of_type_rose()
    {
        await base.Can_use_of_type_rose();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] IN ("Daisy", "Rose") AND (c["$type"] = "Rose"))
""");
    }

    public override async Task Can_query_all_animals()
    {
        await base.Can_query_all_animals();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
    }

    [ConditionalFact(Skip = "Issue#17246 Views are not supported")]
    public override async Task Can_query_all_animal_views()
    {
        await base.Can_query_all_animal_views();

        AssertSql(" ");
    }

    public override async Task Can_query_all_plants()
    {
        await base.Can_query_all_plants();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["$type"] IN ("Daisy", "Rose")
ORDER BY c["id"]
""");
    }

    public override async Task Can_filter_all_animals()
    {
        await base.Can_filter_all_animals();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Name"] = "Great spotted kiwi"))
ORDER BY c["Species"]
""");
    }

    public override async Task Can_query_all_birds()
    {
        await base.Can_query_all_birds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
    }

    public override async Task Can_query_just_kiwis()
    {
        await base.Can_query_just_kiwis();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Can_query_just_roses()
    {
        await base.Can_query_just_roses();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["$type"] = "Rose")
OFFSET 0 LIMIT 2
""");
    }

    [ConditionalFact(Skip = "Issue#17246 Non-embedded Include")]
    public override async Task Include_root()
    {
        await base.Include_root();

        AssertSql(" ");
    }

    [ConditionalFact(Skip = "Issue#17246 Non-embedded Include")]
    public override async Task Can_include_prey()
    {
        await base.Can_include_prey();

        AssertSql(" ");
    }

    public override async Task OfType_leaf_with_predicate_on_leaf()
    {
        await base.OfType_leaf_with_predicate_on_leaf();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["FoundOn"] = 1))
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property()
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["FoundOn"] = 0))
""");
    }

    public override async Task Project_scalar_from_leaf()
    {
        await base.Project_scalar_from_leaf();

        AssertSql(
            """
SELECT VALUE c["FoundOn"]
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2()
    {
        await base.Discriminator_used_when_projection_over_derived_type2();

        AssertSql(
            """
SELECT c["IsFlightless"], c["Discriminator"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
    {
        await base.Project_root_scalar_via_root_with_EF_Property_and_downcast();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND ("Kiwi" = c["Discriminator"]))
""");
    }

    public override async Task Discriminator_used_when_projection_over_of_type()
    {
        await base.Discriminator_used_when_projection_over_of_type();

        AssertSql(
            """
SELECT VALUE c["FoundOn"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    [ConditionalFact(Skip = "Issue#17246 Transations not supported")]
    public override async Task Can_insert_update_delete()
    {
        await base.Can_insert_update_delete();

        AssertSql(" ");
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery()
    {
        await base.Union_siblings_with_duplicate_property_in_subquery();

        AssertSql(" ");
    }

    public override async Task OfType_Union_OfType_Where()
    {
        await base.OfType_Union_OfType_Where();

        AssertSql(" ");
    }

    public override async Task OfType_leaf_Union_intermediate_OfType_leaf()
    {
        await base.OfType_leaf_Union_intermediate_OfType_leaf();

        AssertSql(" ");
    }

    public override Task OfType_in_subquery()
        => AssertTranslationFailedWithDetails(
            () => base.OfType_in_subquery(),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Union_entity_equality()
    {
        await base.Union_entity_equality();

        AssertSql(" ");
    }

    public override async Task Setting_foreign_key_to_a_different_type_throws()
    {
        await base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Byte_enum_value_constant_used_in_projection()
    {
        await base.Byte_enum_value_constant_used_in_projection();

        AssertSql(
            """
SELECT VALUE (c["IsFlightless"] ? 0 : 1)
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
ORDER BY c["Name"]
""");
    }

    [ConditionalFact(Skip = "Issue#17246 subquery usage")]
    public override async Task Is_on_subquery_result()
    {
        await base.Is_on_subquery_result();

        AssertSql(" ");
    }

    public override async Task Project_scalar_from_root_via_root()
    {
        await base.Project_scalar_from_root_via_root();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Project_scalar_from_root_via_leaf()
    {
        await base.Project_scalar_from_root_via_leaf();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task GetType_abstract_root()
    {
        await base.GetType_abstract_root();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
    }

    public override async Task GetType_abstract_intermediate()
    {
        await base.GetType_abstract_intermediate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
    }

    public override async Task GetType_leaf1()
    {
        await base.GetType_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Eagle"))
""");
    }

    public override async Task GetType_leaf2()
    {
        await base.GetType_leaf2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task GetType_leaf_reverse_equality()
    {
        await base.GetType_leaf_reverse_equality();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task GetType_not_leaf1()
    {
        await base.GetType_not_leaf1();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] != "Kiwi"))
""");
    }

    public override async Task Is_on_multiple_contradictory_types()
    {
        await base.Is_on_multiple_contradictory_types();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["Discriminator"] = "Eagle"))
""");
    }

    public override async Task Is_and_OfType_with_multiple_contradictory_types()
    {
        await base.Is_and_OfType_with_multiple_contradictory_types();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["Discriminator"] = "Eagle"))
""");
    }

    public override async Task Primitive_collection_on_subtype()
    {
        await base.Primitive_collection_on_subtype();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN (0, 1, 2, 3) AND (ARRAY_LENGTH(c["Ints"]) > 0))
""");
    }

    protected override bool EnforcesFkConstraints
        => false;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
