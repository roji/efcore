// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPTInheritanceBulkUpdatesSqlServerTest : TPTInheritanceBulkUpdatesTestBase<TPTInheritanceBulkUpdatesSqlServerFixture>
{
    public TPTInheritanceBulkUpdatesSqlServerTest(TPTInheritanceBulkUpdatesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
        => ClearLog();

    public override async Task Delete_on_root()
    {
        await base.Delete_on_root();

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

    // public override async Task Delete_where_keyless_entity_mapped_to_sql_query()
    // {
    //     await base.Delete_where_keyless_entity_mapped_to_sql_query();

    //     AssertSql();
    // }

    public override async Task Delete_on_root_with_subquery()
    {
        await base.Delete_on_root_with_subquery();

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

UPDATE [r]
SET [r].[RootInt] = @p
FROM [Roots] AS [r]
WHERE [r].[RootInt] = 8
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
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL
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
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
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
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
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

UPDATE [r]
SET [r].[Int] = @p
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RootInt], [r0].[RootReferencingEntityId]
    FROM [Roots] AS [r0]
) AS [s] ON [r].[Id] = [s].[RootReferencingEntityId]
WHERE [s].[RootInt] = 8
""");
    }

    // public override async Task Update_where_keyless_entity_mapped_to_sql_query()
    // {
    //     await base.Update_where_keyless_entity_mapped_to_sql_query();

    //     AssertExecuteUpdateSql();
    // }

//     public override async Task Update_with_interface_in_property_expression()
//     {
//         await base.Update_with_interface_in_property_expression();

//         AssertExecuteUpdateSql(
//             """
// @p='0'

// UPDATE [c]
// SET [c].[SugarGrams] = @p
// FROM [Drinks] AS [d]
// INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
// """);
//     }

//     public override async Task Update_with_interface_in_EF_Property_in_property_expression()
//     {
//         await base.Update_with_interface_in_EF_Property_in_property_expression();

//         AssertExecuteUpdateSql(
//             """
// @p='0'

// UPDATE [c]
// SET [c].[SugarGrams] = @p
// FROM [Drinks] AS [d]
// INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
// """);
//     }

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
