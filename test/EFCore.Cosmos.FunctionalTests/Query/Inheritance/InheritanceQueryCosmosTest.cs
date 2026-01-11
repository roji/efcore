// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

#nullable disable

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

    public override async Task Using_OfType_on_multiple_type_with_no_result()
    {
        await base.Using_OfType_on_multiple_type_with_no_result();

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

    public override async Task Can_query_all_types_when_shared_column()
    {
        await base.Can_query_all_types_when_shared_column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Discriminator"] IN (0, 1, 2, 3)
""");
    }

    public override async Task Can_use_of_type_animal()
    {
        await base.Can_use_of_type_animal();

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

    public override async Task Can_use_is_kiwi_with_cast()
    {
        await base.Can_use_is_kiwi_with_cast();

        AssertSql(
            """
SELECT VALUE ((c["Discriminator"] = "Kiwi") ? c["FoundOn"] : 0)
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Can_use_backwards_is_animal()
    {
        await base.Can_use_backwards_is_animal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate()
    {
        await base.Can_use_is_kiwi_with_other_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND ((c["Discriminator"] = "Kiwi") AND (c["CountryId"] = 1)))
""");
    }

    public override async Task Can_use_is_kiwi_in_projection()
    {
        await base.Can_use_is_kiwi_in_projection();

        AssertSql(
            """
SELECT VALUE (c["Discriminator"] = "Kiwi")
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Can_use_of_type_bird()
    {
        await base.Can_use_of_type_bird();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
    }

    public override async Task Can_use_of_type_bird_predicate()
    {
        await base.Can_use_of_type_bird_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["CountryId"] = 1)) AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection()
    {
        await base.Can_use_of_type_bird_with_projection();

        AssertSql(
            """
SELECT VALUE c["EagleId"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
""");
    }

    public override async Task Can_use_of_type_bird_first()
    {
        await base.Can_use_of_type_bird_first();

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

    public override async Task Can_use_backwards_of_type_animal()
    {
        await base.Can_use_backwards_of_type_animal();

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
    public override async Task Can_include_animals()
    {
        await base.Can_include_animals();

        AssertSql(" ");
    }

    [ConditionalFact(Skip = "Issue#17246 Non-embedded Include")]
    public override async Task Can_include_prey()
    {
        await base.Can_include_prey();

        AssertSql(" ");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property()
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property();

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

    public override async Task Discriminator_used_when_projection_over_derived_type()
    {
        await base.Discriminator_used_when_projection_over_derived_type();

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

    public override async Task Discriminator_with_cast_in_shadow_property()
    {
        await base.Discriminator_with_cast_in_shadow_property();

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

    public override async Task OfType_Union_subquery()
    {
        await base.OfType_Union_subquery();

        AssertSql(" ");
    }

    public override async Task OfType_Union_OfType()
    {
        await base.OfType_Union_OfType();

        AssertSql(" ");
    }

    public override Task Subquery_OfType()
        => AssertTranslationFailedWithDetails(
            () => base.Subquery_OfType(),
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
    public override async Task Is_operator_on_result_of_FirstOrDefault()
    {
        await base.Is_operator_on_result_of_FirstOrDefault();

        AssertSql(" ");
    }

    public override async Task Selecting_only_base_properties_on_base_type()
    {
        await base.Selecting_only_base_properties_on_base_type();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type()
    {
        await base.Selecting_only_base_properties_on_derived_type();

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
    }

    public override async Task GetType_in_hierarchy_in_abstract_base_type()
    {
        await base.GetType_in_hierarchy_in_abstract_base_type();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type()
    {
        await base.GetType_in_hierarchy_in_intermediate_type();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling()
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Eagle"))
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2()
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse()
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal()
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] != "Kiwi"))
""");
    }

    public override async Task Using_is_operator_on_multiple_type_with_no_result()
    {
        await base.Using_is_operator_on_multiple_type_with_no_result();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["Discriminator"] = "Eagle"))
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result()
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result();

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
}
