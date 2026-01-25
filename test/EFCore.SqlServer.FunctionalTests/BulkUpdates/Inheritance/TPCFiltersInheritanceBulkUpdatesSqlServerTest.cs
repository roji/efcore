// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPCFiltersInheritanceBulkUpdatesSqlServerTest(
    TPCFiltersInheritanceBulkUpdatesSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPCFiltersInheritanceBulkUpdatesTestBase<TPCFiltersInheritanceBulkUpdatesSqlServerFixture>(fixture, testOutputHelper)
{
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

        AssertSql(
            """
DELETE FROM [l]
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] <> 8 AND [l].[Leaf1Int] = 1001
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
    SELECT [u].[RootInt], [u].[RootReferencingEntityId]
    FROM (
        SELECT [r0].[RootInt], [r0].[RootReferencingEntityId]
        FROM [Roots] AS [r0]
        UNION ALL
        SELECT [c].[RootInt], [c].[RootReferencingEntityId]
        FROM [ConcreteIntermediate] AS [c]
        UNION ALL
        SELECT [i].[RootInt], [i].[RootReferencingEntityId]
        FROM [Intermediate] AS [i]
        UNION ALL
        SELECT [l].[RootInt], [l].[RootReferencingEntityId]
        FROM [Leaf3] AS [l]
        UNION ALL
        SELECT [l0].[RootInt], [l0].[RootReferencingEntityId]
        FROM [Leaf1] AS [l0]
        UNION ALL
        SELECT [l1].[RootInt], [l1].[RootReferencingEntityId]
        FROM [Leaf2] AS [l1]
    ) AS [u]
    WHERE [u].[RootInt] <> 8
) AS [u0] ON [r].[Id] = [u0].[RootReferencingEntityId]
WHERE [u0].[RootInt] = 9
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

        AssertSql();
    }

    public override async Task Update_root()
    {
        await base.Update_root();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_with_OfType_leaf()
    {
        await base.Update_with_OfType_leaf();

        AssertExecuteUpdateSql();
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

UPDATE [l]
SET [l].[RootInt] = @p
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] <> 8
""");
    }

    public override async Task Update_leaf_property()
    {
        await base.Update_leaf_property();

        AssertExecuteUpdateSql(
            """
@p='999'

UPDATE [l]
SET [l].[Leaf1Int] = @p
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] <> 8
""");
    }

    public override async Task Update_both_root_and_leaf_properties()
    {
        await base.Update_both_root_and_leaf_properties();

        AssertExecuteUpdateSql(
            """
@p='998'
@p1='999'

UPDATE [l]
SET [l].[RootInt] = @p,
    [l].[Leaf1Int] = @p1
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] <> 8
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
    SELECT [u].[RootInt], [u].[RootReferencingEntityId]
    FROM (
        SELECT [r0].[RootInt], [r0].[RootReferencingEntityId]
        FROM [Roots] AS [r0]
        UNION ALL
        SELECT [c].[RootInt], [c].[RootReferencingEntityId]
        FROM [ConcreteIntermediate] AS [c]
        UNION ALL
        SELECT [i].[RootInt], [i].[RootReferencingEntityId]
        FROM [Intermediate] AS [i]
        UNION ALL
        SELECT [l].[RootInt], [l].[RootReferencingEntityId]
        FROM [Leaf3] AS [l]
        UNION ALL
        SELECT [l0].[RootInt], [l0].[RootReferencingEntityId]
        FROM [Leaf1] AS [l0]
        UNION ALL
        SELECT [l1].[RootInt], [l1].[RootReferencingEntityId]
        FROM [Leaf2] AS [l1]
    ) AS [u]
    WHERE [u].[RootInt] <> 8
) AS [u0] ON [r].[Id] = [u0].[RootReferencingEntityId]
WHERE [u0].[RootInt] = 9
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
