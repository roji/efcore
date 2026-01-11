// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPTInheritanceBulkUpdatesSqliteTest(
    TPTInheritanceBulkUpdatesSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPTInheritanceBulkUpdatesTestBase<TPTInheritanceBulkUpdatesSqliteFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy()
    {
        await base.Delete_where_hierarchy();

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived()
    {
        await base.Delete_where_hierarchy_derived();

        AssertSql();
    }

    public override async Task Delete_where_using_hierarchy()
    {
        await base.Delete_where_using_hierarchy();

        AssertSql();
    }

    public override async Task Delete_where_using_hierarchy_derived()
    {
        await base.Delete_where_using_hierarchy_derived();

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First()
    {
        await base.Delete_GroupBy_Where_Select_First();

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_2()
    {
        await base.Delete_GroupBy_Where_Select_First_2();

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_3()
    {
        await base.Delete_GroupBy_Where_Select_First_3();

        AssertSql();
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query()
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query();

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_subquery()
    {
        await base.Delete_where_hierarchy_subquery();

        AssertSql();
    }

    public override async Task Update_base_type()
    {
        await base.Update_base_type();

        AssertExecuteUpdateSql(
            """
@p='Animal' (Size = 6)

UPDATE "Animals" AS "a0"
SET "Name" = @p
FROM (
    SELECT "a"."Id"
    FROM "Animals" AS "a"
    WHERE "a"."Name" = 'Great spotted kiwi'
) AS "s"
WHERE "a0"."Id" = "s"."Id"
""");
    }

    // #31402
    public override Task Update_base_type_with_OfType()
        => Assert.ThrowsAsync<SqliteException>(() => base.Update_base_property_on_derived_type());

    public override async Task Update_where_hierarchy_subquery()
    {
        await base.Update_where_hierarchy_subquery();

        AssertExecuteUpdateSql();
    }

    // #31402
    public override Task Update_base_property_on_derived_type()
        => Assert.ThrowsAsync<SqliteException>(() => base.Update_base_property_on_derived_type());

    public override async Task Update_derived_property_on_derived_type()
    {
        await base.Update_derived_property_on_derived_type();

        AssertExecuteUpdateSql(
            """
@p='0'

UPDATE "Kiwi" AS "k"
SET "FoundOn" = @p
FROM "Animals" AS "a"
INNER JOIN "Birds" AS "b" ON "a"."Id" = "b"."Id"
WHERE "a"."Id" = "k"."Id"
""");
    }

    public override async Task Update_where_using_hierarchy()
    {
        await base.Update_where_using_hierarchy();

        AssertExecuteUpdateSql(
            """
@p='Monovia' (Size = 7)

UPDATE "Countries" AS "c"
SET "Name" = @p
WHERE (
    SELECT COUNT(*)
    FROM "Animals" AS "a"
    WHERE "c"."Id" = "a"."CountryId" AND "a"."CountryId" > 0) > 0
""");
    }

    public override async Task Update_base_and_derived_types()
    {
        await base.Update_base_and_derived_types();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_using_hierarchy_derived()
    {
        await base.Update_where_using_hierarchy_derived();

        AssertExecuteUpdateSql(
            """
@p='Monovia' (Size = 7)

UPDATE "Countries" AS "c"
SET "Name" = @p
WHERE (
    SELECT COUNT(*)
    FROM "Animals" AS "a"
    LEFT JOIN "Kiwi" AS "k" ON "a"."Id" = "k"."Id"
    WHERE "c"."Id" = "a"."CountryId" AND "k"."Id" IS NOT NULL AND "a"."CountryId" > 0) > 0
""");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query()
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_with_interface_in_property_expression()
    {
        await base.Update_with_interface_in_property_expression();

        AssertExecuteUpdateSql(
            """
@p='0'

UPDATE "Coke" AS "c"
SET "SugarGrams" = @p
FROM "Drinks" AS "d"
WHERE "d"."Id" = "c"."Id"
""");
    }

    public override async Task Update_with_interface_in_EF_Property_in_property_expression()
    {
        await base.Update_with_interface_in_EF_Property_in_property_expression();

        AssertExecuteUpdateSql(
            """
@p='0'

UPDATE "Coke" AS "c"
SET "SugarGrams" = @p
FROM "Drinks" AS "d"
WHERE "d"."Id" = "c"."Id"
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
