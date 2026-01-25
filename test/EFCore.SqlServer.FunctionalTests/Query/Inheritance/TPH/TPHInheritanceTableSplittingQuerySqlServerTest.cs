// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPH;

public class TPHInheritanceTableSplittingQuerySqlServerTest(
    TPHInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceTableSplittingQueryRelationalTestBase<TPHInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_leaf()
    {
        await base.Filter_on_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[ChildComplexType_Int] = 9
""");
    }

    public override async Task Filter_on_complex_type_property_on_root()
    {
        await base.Filter_on_complex_type_property_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[ParentComplexType_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_leaf()
    {
        await base.Filter_on_nested_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[ChildComplexType_Nested_Int] = 51
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_root()
    {
        await base.Filter_on_nested_complex_type_property_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[ParentComplexType_Nested_Int] = 50
""");
    }

    public override async Task Project_complex_type_on_leaf()
    {
        await base.Project_complex_type_on_leaf();

        AssertSql(
            """
SELECT [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Project_complex_type_on_root()
    {
        await base.Project_complex_type_on_root();

        AssertSql(
            """
SELECT [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
""");
    }

    public override async Task Project_nested_complex_type_on_leaf()
    {
        await base.Project_nested_complex_type_on_leaf();

        AssertSql(
            """
SELECT [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Project_nested_complex_type_on_root()
    {
        await base.Project_nested_complex_type_on_root();

        AssertSql(
            """
SELECT [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
""");
    }

    public override async Task Subquery_over_complex_collection()
    {
        await base.Subquery_over_complex_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c]
    WHERE [c].[Int] > 59) = 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
