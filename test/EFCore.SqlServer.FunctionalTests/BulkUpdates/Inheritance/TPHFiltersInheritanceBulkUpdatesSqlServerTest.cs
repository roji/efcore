// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPHFiltersInheritanceBulkUpdatesSqlServerTest(
    TPHFiltersInheritanceBulkUpdatesSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : FiltersInheritanceBulkUpdatesRelationalTestBase<
        TPHFiltersInheritanceBulkUpdatesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Delete_on_root()
    {
        await base.Delete_on_root();

        AssertSql(
            """
DELETE FROM [r]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[RootInt] = 9
""");
    }

    public override async Task Delete_on_root_with_subquery()
    {
        await base.Delete_on_root_with_subquery();

        AssertSql(
            """
@p='0'
@p1='3'

DELETE FROM [r]
FROM [Roots] AS [r]
WHERE [r].[Id] IN (
    SELECT [r0].[Id]
    FROM [Roots] AS [r0]
    WHERE [r0].[RootInt] <> 8 AND [r0].[RootInt] = 9
    ORDER BY [r0].[RootInt]
    OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
)
""");
    }

    public override async Task Delete_on_leaf()
    {
        await base.Delete_on_leaf();

        AssertSql(
            """
DELETE FROM [r]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] <> 8 AND [r].[Leaf1Int] = 1001
""");
    }

    public override async Task Delete_entity_type_referencing_hierarchy()
    {
        await base.Delete_entity_type_referencing_hierarchy();

        AssertSql(
            """
DELETE FROM [r]
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RootInt], [r0].[RootReferencingEntityId]
    FROM [Roots] AS [r0]
    WHERE [r0].[RootInt] <> 8
) AS [r1] ON [r].[Id] = [r1].[RootReferencingEntityId]
WHERE [r1].[RootInt] = 9
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
DELETE FROM [r]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Id] IN (
    SELECT (
        SELECT TOP(1) [r1].[Id]
        FROM [Roots] AS [r1]
        WHERE [r1].[RootInt] <> 8 AND [r0].[RootInt] = [r1].[RootInt])
    FROM [Roots] AS [r0]
    WHERE [r0].[RootInt] <> 8
    GROUP BY [r0].[RootInt]
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

UPDATE [r]
SET [r].[RootInt] = @p
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[RootInt] = 9
""");
    }

    public override async Task Update_with_OfType_leaf()
    {
        await base.Update_with_OfType_leaf();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE [r]
SET [r].[RootInt] = @p
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Discriminator] = N'Leaf1'
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

UPDATE [r]
SET [r].[RootInt] = @p
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] <> 8
""");
    }

    public override async Task Update_leaf_property()
    {
        await base.Update_leaf_property();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE [r]
SET [r].[Leaf1Int] = @p
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] <> 8
""");
    }

    public override async Task Update_both_root_and_leaf_properties()
    {
        await base.Update_both_root_and_leaf_properties();

        AssertExecuteUpdateSql(
            """
@p='998'
@p1='999'

UPDATE [r]
SET [r].[RootInt] = @p,
    [r].[Leaf1Int] = @p1
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] <> 8
""");
    }

    public override async Task Update_entity_type_referencing_hierarchy()
    {
        await base.Update_entity_type_referencing_hierarchy();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE [r]
SET [r].[Int] = @p
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RootInt], [r0].[RootReferencingEntityId]
    FROM [Roots] AS [r0]
    WHERE [r0].[RootInt] <> 8
) AS [r1] ON [r].[Id] = [r1].[RootReferencingEntityId]
WHERE [r1].[RootInt] = 9
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
