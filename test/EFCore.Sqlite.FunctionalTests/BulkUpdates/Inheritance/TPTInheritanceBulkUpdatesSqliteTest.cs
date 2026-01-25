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

    public override async Task Delete_on_root()
    {
        await base.Delete_on_root();

        AssertSql();
    }

    public override async Task Delete_on_root_with_subquery()
    {
        await base.Delete_on_root_with_subquery();

        AssertSql();
    }

    public override async Task Delete_on_leaf()
    {
        await base.Delete_on_leaf();

        AssertSql();
    }

    public override async Task Delete_entity_type_referencing_hierarchy()
    {
        await base.Delete_entity_type_referencing_hierarchy();

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

    public override async Task Update_root()
    {
        await base.Update_root();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Roots" AS "r0"
SET "RootInt" = @p
FROM (
    SELECT "r"."Id"
    FROM "Roots" AS "r"
    WHERE "r"."RootInt" = 8
) AS "s"
WHERE "r0"."Id" = "s"."Id"
""");
    }

    // #31402 - SQLite doesn't support complex UPDATE FROM with OfType query
    public override Task Update_with_OfType_leaf()
        => Assert.ThrowsAsync<SqliteException>(() => base.Update_with_OfType_leaf());

    public override async Task Update_root_with_subquery()
    {
        await base.Update_root_with_subquery();

        AssertExecuteUpdateSql();
    }

    // #31402 - SQLite doesn't support complex UPDATE FROM for TPT hierarchies
    public override Task Update_root_property_on_leaf()
        => Assert.ThrowsAsync<SqliteException>(() => base.Update_root_property_on_leaf());

    public override async Task Update_leaf_property()
    {
        await base.Update_leaf_property();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Leaf1" AS "l"
SET "Leaf1Int" = @p
FROM "Roots" AS "r"
INNER JOIN "Intermediate" AS "i" ON "r"."Id" = "i"."Id"
WHERE "r"."Id" = "l"."Id"
""");
    }

    public override async Task Update_both_root_and_leaf_properties()
    {
        await base.Update_both_root_and_leaf_properties();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_entity_type_referencing_hierarchy()
    {
        await base.Update_entity_type_referencing_hierarchy();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "RootReferencingEntities" AS "r1"
SET "Int" = @p
FROM (
    SELECT "r"."Id"
    FROM "RootReferencingEntities" AS "r"
    LEFT JOIN (
        SELECT "r0"."RootInt", "r0"."RootReferencingEntityId"
        FROM "Roots" AS "r0"
    ) AS "s" ON "r"."Id" = "s"."RootReferencingEntityId"
    WHERE "s"."RootInt" = 8
) AS "s0"
WHERE "r1"."Id" = "s0"."Id"
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
