// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPHInheritanceBulkUpdatesSqliteTest(
    TPHInheritanceBulkUpdatesSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceBulkUpdatesTestBase<TPHInheritanceBulkUpdatesSqliteFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_on_root()
    {
        await base.Delete_on_root();

        AssertSql(
            """
DELETE FROM "Roots" AS "r"
WHERE "r"."RootInt" = 8
""");
    }

    public override async Task Delete_on_root_with_subquery()
    {
        await base.Delete_on_root_with_subquery();

        AssertSql(
            """
@p1='3'
@p='0'

DELETE FROM "Roots" AS "r"
WHERE "r"."Id" IN (
    SELECT "r0"."Id"
    FROM "Roots" AS "r0"
    WHERE "r0"."RootInt" = 8
    ORDER BY "r0"."RootInt"
    LIMIT @p1 OFFSET @p
)
""");
    }

    public override async Task Delete_on_leaf()
    {
        await base.Delete_on_leaf();

        AssertSql(
            """
DELETE FROM "Roots" AS "r"
WHERE "r"."Discriminator" = 'Leaf1' AND "r"."Leaf1Int" = 1000
""");
    }

    public override async Task Delete_entity_type_referencing_hierarchy()
    {
        await base.Delete_entity_type_referencing_hierarchy();

        AssertSql(
            """
DELETE FROM "RootReferencingEntities" AS "r"
WHERE "r"."Id" IN (
    SELECT "r0"."Id"
    FROM "RootReferencingEntities" AS "r0"
    LEFT JOIN "Roots" AS "r1" ON "r0"."Id" = "r1"."RootReferencingEntityId"
    WHERE "r1"."RootInt" = 8
)
""");
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

        AssertSql(
            """
DELETE FROM "Roots" AS "r"
WHERE "r"."Id" IN (
    SELECT (
        SELECT "r1"."Id"
        FROM "Roots" AS "r1"
        WHERE "r0"."RootInt" = "r1"."RootInt"
        LIMIT 1)
    FROM "Roots" AS "r0"
    GROUP BY "r0"."RootInt"
    HAVING COUNT(*) < 3
)
""");
    }

    public override async Task Update_root()
    {
        await base.Update_root();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Roots" AS "r"
SET "RootInt" = @p
WHERE "r"."RootInt" = 8
""");
    }

    public override async Task Update_with_OfType_leaf()
    {
        await base.Update_with_OfType_leaf();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Roots" AS "r"
SET "RootInt" = @p
WHERE "r"."Discriminator" = 'Leaf1'
""");
    }

    public override async Task Update_root_with_subquery()
    {
        await base.Update_root_with_subquery();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_root_property_on_leaf()
    {
        await base.Update_root_property_on_leaf();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Roots" AS "r"
SET "RootInt" = @p
WHERE "r"."Discriminator" = 'Leaf1'
""");
    }

    public override async Task Update_leaf_property()
    {
        await base.Update_leaf_property();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE "Roots" AS "r"
SET "Leaf1Int" = @p
WHERE "r"."Discriminator" = 'Leaf1'
""");
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
    LEFT JOIN "Roots" AS "r0" ON "r"."Id" = "r0"."RootReferencingEntityId"
    WHERE "r0"."RootInt" = 8
) AS "s"
WHERE "r1"."Id" = "s"."Id"
""");
    }

    public override async Task Update_both_root_and_leaf_properties()
    {
        await base.Update_both_root_and_leaf_properties();

        AssertExecuteUpdateSql(
            """
@p='998'
@p1='999'

UPDATE "Roots" AS "r"
SET "RootInt" = @p,
    "Leaf1Int" = @p1
WHERE "r"."Discriminator" = 'Leaf1'
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
