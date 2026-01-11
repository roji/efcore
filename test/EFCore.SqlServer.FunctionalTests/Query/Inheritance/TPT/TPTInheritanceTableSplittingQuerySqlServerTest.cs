// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPT;

public class TPTInheritanceTableSplittingQuerySqlServerTest(
    TPTInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPTInheritanceTableSplittingQueryRelationalTestBase<TPTInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_leaf()
    {
        await base.Filter_on_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[ChildComplexType_Int] = 9
""");
    }

    public override async Task Filter_on_complex_type_property_on_root()
    {
        await base.Filter_on_complex_type_property_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [c].[ConcreteIntermediateInt], [i].[IntermediateInt], [l].[Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l1].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], [l1].[ChildComplexType_Int], [l1].[ChildComplexType_UniqueId], [l1].[ChildComplexType_Nested_Int], [l1].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l1].[Id] IS NOT NULL THEN N'Leaf2'
    WHEN [l0].[Id] IS NOT NULL THEN N'Leaf1'
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf3'
    WHEN [i].[Id] IS NOT NULL THEN N'Intermediate'
    WHEN [c].[Id] IS NOT NULL THEN N'ConcreteIntermediate'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [ConcreteIntermediate] AS [c] ON [r].[Id] = [c].[Id]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf3] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf1] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN [Leaf2] AS [l1] ON [r].[Id] = [l1].[Id]
WHERE [r].[ParentComplexType_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_leaf()
    {
        await base.Filter_on_nested_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[ChildComplexType_Nested_Int] = 51
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_root()
    {
        await base.Filter_on_nested_complex_type_property_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [c].[ConcreteIntermediateInt], [i].[IntermediateInt], [l].[Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l1].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], [l1].[ChildComplexType_Int], [l1].[ChildComplexType_UniqueId], [l1].[ChildComplexType_Nested_Int], [l1].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l1].[Id] IS NOT NULL THEN N'Leaf2'
    WHEN [l0].[Id] IS NOT NULL THEN N'Leaf1'
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf3'
    WHEN [i].[Id] IS NOT NULL THEN N'Intermediate'
    WHEN [c].[Id] IS NOT NULL THEN N'ConcreteIntermediate'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [ConcreteIntermediate] AS [c] ON [r].[Id] = [c].[Id]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf3] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf1] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN [Leaf2] AS [l1] ON [r].[Id] = [l1].[Id]
WHERE [r].[ParentComplexType_Nested_Int] = 50
""");
    }

    public override async Task Project_complex_type_on_leaf()
    {
        await base.Project_complex_type_on_leaf();

        AssertSql(
            """
SELECT [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
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
SELECT [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
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
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [c].[ConcreteIntermediateInt], [i].[IntermediateInt], [l].[Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l1].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], [l1].[ChildComplexType_Int], [l1].[ChildComplexType_UniqueId], [l1].[ChildComplexType_Nested_Int], [l1].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l1].[Id] IS NOT NULL THEN N'Leaf2'
    WHEN [l0].[Id] IS NOT NULL THEN N'Leaf1'
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf3'
    WHEN [i].[Id] IS NOT NULL THEN N'Intermediate'
    WHEN [c].[Id] IS NOT NULL THEN N'ConcreteIntermediate'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [ConcreteIntermediate] AS [c] ON [r].[Id] = [c].[Id]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf3] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf1] AS [l0] ON [r].[Id] = [l0].[Id]
LEFT JOIN [Leaf2] AS [l1] ON [r].[Id] = [l1].[Id]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c0]
    WHERE [c0].[Int] > 59) = 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
