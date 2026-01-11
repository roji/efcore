// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public class TPCInheritanceBulkUpdatesSqlServerTest(
    TPCInheritanceBulkUpdatesSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPCInheritanceBulkUpdatesTestBase<TPCInheritanceBulkUpdatesSqlServerFixture>(fixture, testOutputHelper)
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

        AssertSql(
            """
DELETE FROM [k]
FROM [Kiwi] AS [k]
WHERE [k].[Name] = N'Great spotted kiwi'
""");
    }

    public override async Task Delete_where_using_hierarchy()
    {
        await base.Delete_where_using_hierarchy();

        AssertSql(
            """
DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [e].[CountryId]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Delete_where_using_hierarchy_derived()
    {
        await base.Delete_where_using_hierarchy_derived();

        AssertSql(
            """
DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
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

    public override async Task Update_base_type()
    {
        await base.Update_base_type();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_type_with_OfType()
    {
        await base.Update_base_type_with_OfType();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_subquery()
    {
        await base.Update_where_hierarchy_subquery();

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_property_on_derived_type()
    {
        await base.Update_base_property_on_derived_type();

        AssertExecuteUpdateSql(
            """
@p='SomeOtherKiwi' (Size = 4000)

UPDATE [k]
SET [k].[Name] = @p
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Update_derived_property_on_derived_type()
    {
        await base.Update_derived_property_on_derived_type();

        AssertExecuteUpdateSql(
            """
@p='0' (Size = 1)

UPDATE [k]
SET [k].[FoundOn] = @p
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Update_where_using_hierarchy()
    {
        await base.Update_where_using_hierarchy();

        AssertExecuteUpdateSql(
            """
@p='Monovia' (Size = 4000)

UPDATE [c]
SET [c].[Name] = @p
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [e].[CountryId]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Update_base_and_derived_types()
    {
        await base.Update_base_and_derived_types();

        AssertExecuteUpdateSql(
            """
@p='Kiwi' (Size = 4000)
@p1='0' (Size = 1)

UPDATE [k]
SET [k].[Name] = @p,
    [k].[FoundOn] = @p1
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Update_where_using_hierarchy_derived()
    {
        await base.Update_where_using_hierarchy_derived();

        AssertExecuteUpdateSql(
            """
@p='Monovia' (Size = 4000)

UPDATE [c]
SET [c].[Name] = @p
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Update_with_interface_in_property_expression()
    {
        await base.Update_with_interface_in_property_expression();

        AssertExecuteUpdateSql(
            """
@p='0'

UPDATE [c]
SET [c].[SugarGrams] = @p
FROM [Coke] AS [c]
""");
    }

    public override async Task Update_with_interface_in_EF_Property_in_property_expression()
    {
        await base.Update_with_interface_in_EF_Property_in_property_expression();

        AssertExecuteUpdateSql(
            """
@p='0'

UPDATE [c]
SET [c].[SugarGrams] = @p
FROM [Coke] AS [c]
""");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query()
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query();

        AssertExecuteUpdateSql();
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
