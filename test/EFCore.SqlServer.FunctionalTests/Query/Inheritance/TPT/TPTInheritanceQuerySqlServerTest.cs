// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPT;

public class TPTInheritanceQuerySqlServerTest(TPTInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPTInheritanceQueryTestBase<TPTInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Query_root()
    {
        await base.Query_root();

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
""");
    }

    public override async Task Query_intermediate()
    {
        await base.Query_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l0].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'Leaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2] AS [l0] ON [r].[Id] = [l0].[Id]
""");
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Query_leaf2()
    {
        await base.Query_leaf2();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf2] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Filter_root()
    {
        await base.Filter_root();

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
WHERE [r].[RootInt] = 8
""");
    }

    public override async Task Project_scalar_from_leaf()
    {
        await base.Project_scalar_from_leaf();

        AssertSql();
    }

    public override async Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
    {
        await base.Project_root_scalar_via_root_with_EF_Property_and_downcast();

        AssertSql();
    }

    public override async Task Project_scalar_from_root_via_leaf()
    {
        await base.Project_scalar_from_root_via_leaf();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Project_scalar_from_root_via_root()
    {
        await base.Project_scalar_from_root_via_root();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

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
""");
    }

    public override async Task OfType_root_via_leaf()
    {
        await base.OfType_root_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task OfType_intermediate()
    {
        await base.OfType_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l0].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l0].[Id] IS NOT NULL THEN N'Leaf2'
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
    WHEN [i].[Id] IS NOT NULL THEN N'Intermediate'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
LEFT JOIN [Leaf2] AS [l0] ON [r].[Id] = [l0].[Id]
WHERE [l0].[Id] IS NOT NULL OR [l].[Id] IS NOT NULL OR [i].[Id] IS NOT NULL
""");
    }

    public override async Task OfType_leaf1()
    {
        await base.OfType_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL
""");
    }

    public override async Task OfType_leaf2()
    {
        await base.OfType_leaf2();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf2'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf2] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL
""");
    }

    public override async Task OfType_leaf_with_predicate_on_leaf()
    {
        await base.OfType_leaf_with_predicate_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL AND [l].[Leaf1Int] = 1000
""");
    }

    public override async Task OfType_leaf_with_predicate_on_root()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL AND [r].[RootInt] = 8
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL AND [r].[RootInt] = 8
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT [l].[Leaf1Int]
FROM [Roots] AS [r]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL
""");
    }

    public override async Task OfType_OrderBy_First()
    {
        await base.OfType_OrderBy_First();

        AssertSql(
            """
SELECT TOP(1) [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [l].[Id] IS NOT NULL
ORDER BY [l].[Leaf1Int]
""");
    }

    public override async Task OfType_in_subquery()
    {
        await base.OfType_in_subquery();

        AssertSql(
            """
@p='5'

SELECT DISTINCT [s].[Id], [s].[RootInt], [s].[RootReferencingEntityId], [s].[UniqueId], [s].[IntermediateInt], [s].[Ints], [s].[Leaf1Int], [s].[c], [s].[ParentComplexType_Int], [s].[ParentComplexType_UniqueId], [s].[ParentComplexType_Nested_Int], [s].[ParentComplexType_Nested_UniqueId], [s].[ChildComplexType_Int], [s].[ChildComplexType_UniqueId], [s].[ChildComplexType_Nested_Int], [s].[ChildComplexType_Nested_UniqueId], [s].[Discriminator]
FROM (
    SELECT TOP(@p) [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection] AS [c], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
        WHEN [l0].[Id] IS NOT NULL THEN N'Leaf2'
        WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
    END AS [Discriminator]
    FROM [Roots] AS [r]
    INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
    LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
    LEFT JOIN [Leaf2] AS [l0] ON [r].[Id] = [l0].[Id]
    ORDER BY [r].[Id]
) AS [s]
WHERE [s].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_root_via_root()
    {
        await base.Is_root_via_root();

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
""");
    }

    public override async Task Is_root_via_leaf()
    {
        await base.Is_root_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Is_intermediate()
    {
        await base.Is_intermediate();

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
WHERE [l1].[Id] IS NOT NULL OR [l0].[Id] IS NOT NULL OR [i].[Id] IS NOT NULL
""");
    }

    public override async Task Is_leaf_via_root()
    {
        await base.Is_leaf_via_root();

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
WHERE [l0].[Id] IS NOT NULL
""");
    }

    public override async Task Is_leaf_via_leaf()
    {
        await base.Is_leaf_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Is_in_projection()
    {
        await base.Is_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [l].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Roots] AS [r]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

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
WHERE [l0].[Id] IS NOT NULL AND [r].[RootInt] = 8
""");
    }

    public override async Task Is_on_subquery_result()
    {
        await base.Is_on_subquery_result();

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
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [l2].[Id] AS [Id0]
        FROM [Roots] AS [r0]
        LEFT JOIN [Leaf1] AS [l2] ON [r0].[Id] = [l2].[Id]
        WHERE [r0].[UniqueId] = [r].[UniqueId]
    ) AS [s]
    WHERE [s].[Id0] IS NOT NULL)
""");
    }

    public override async Task Include_root()
    {
        await base.Include_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Int], [s].[Id], [s].[RootInt], [s].[RootReferencingEntityId], [s].[UniqueId], [s].[ConcreteIntermediateInt], [s].[IntermediateInt], [s].[Leaf3Int], [s].[Ints], [s].[Leaf1Int], [s].[Leaf2Int], [s].[c], [s].[ParentComplexType_Int], [s].[ParentComplexType_UniqueId], [s].[ParentComplexType_Nested_Int], [s].[ParentComplexType_Nested_UniqueId], [s].[ChildComplexType_Int], [s].[ChildComplexType_UniqueId], [s].[ChildComplexType_Nested_Int], [s].[ChildComplexType_Nested_UniqueId], [s].[ChildComplexType_Int0], [s].[ChildComplexType_UniqueId0], [s].[ChildComplexType_Nested_Int0], [s].[ChildComplexType_Nested_UniqueId0], [s].[Discriminator]
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[RootInt], [r0].[RootReferencingEntityId], [r0].[UniqueId], [c].[ConcreteIntermediateInt], [i].[IntermediateInt], [l].[Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l1].[Leaf2Int], [r0].[ComplexTypeCollection] AS [c], [r0].[ParentComplexType_Int], [r0].[ParentComplexType_UniqueId], [r0].[ParentComplexType_Nested_Int], [r0].[ParentComplexType_Nested_UniqueId], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int0], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId0], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int0], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId0], CASE
        WHEN [l1].[Id] IS NOT NULL THEN N'Leaf2'
        WHEN [l0].[Id] IS NOT NULL THEN N'Leaf1'
        WHEN [l].[Id] IS NOT NULL THEN N'Leaf3'
        WHEN [i].[Id] IS NOT NULL THEN N'Intermediate'
        WHEN [c].[Id] IS NOT NULL THEN N'ConcreteIntermediate'
    END AS [Discriminator]
    FROM [Roots] AS [r0]
    LEFT JOIN [ConcreteIntermediate] AS [c] ON [r0].[Id] = [c].[Id]
    LEFT JOIN [Intermediate] AS [i] ON [r0].[Id] = [i].[Id]
    LEFT JOIN [Leaf3] AS [l] ON [r0].[Id] = [l].[Id]
    LEFT JOIN [Leaf1] AS [l0] ON [r0].[Id] = [l0].[Id]
    LEFT JOIN [Leaf2] AS [l1] ON [r0].[Id] = [l1].[Id]
) AS [s] ON [r].[Id] = [s].[RootReferencingEntityId]
""");
    }

    public override async Task Filter_on_discriminator()
    {
        await base.Filter_on_discriminator();

        AssertSql();
    }

    public override async Task Project_discriminator()
    {
        await base.Project_discriminator();

        AssertSql();
    }

    public override async Task GetType_abstract_root()
    {
        await base.GetType_abstract_root();

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
WHERE [l1].[Id] IS NULL AND [l0].[Id] IS NULL AND [l].[Id] IS NULL AND [i].[Id] IS NULL AND [c].[Id] IS NULL
""");
    }

    public override async Task GetType_abstract_intermediate()
    {
        await base.GetType_abstract_intermediate();

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
WHERE [l1].[Id] IS NULL AND [l0].[Id] IS NULL AND [i].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_leaf1()
    {
        await base.GetType_leaf1();

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
WHERE [l0].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_leaf2()
    {
        await base.GetType_leaf2();

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
WHERE [l1].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_leaf_reverse_equality()
    {
        await base.GetType_leaf_reverse_equality();

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
WHERE [l0].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_not_leaf1()
    {
        await base.GetType_not_leaf1();

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
WHERE [l0].[Id] IS NULL
""");
    }

    public override async Task OfType_Union_OfType_on_same_type_Where()
    {
        await base.OfType_Union_OfType_on_same_type_Where();

        AssertSql();
    }

    public override async Task OfType_leaf_Union_intermediate_OfType_leaf()
    {
        await base.OfType_leaf_Union_intermediate_OfType_leaf();

        AssertSql();
    }

    // public override async Task Union_siblings_with_duplicate_property_in_subquery()
    // {
    //     await base.Union_siblings_with_duplicate_property_in_subquery();

    //     AssertSql();
    // }

    public override async Task Union_entity_equality()
    {
        await base.Union_entity_equality();

        AssertSql();
    }

    public override async Task Conditional_with_is_and_downcast_in_projection()
    {
        await base.Conditional_with_is_and_downcast_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [l].[Id] IS NOT NULL AND [l].[Leaf1Int] = 50 AND [l].[Leaf1Int] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Roots] AS [r]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
""");
    }

    public override async Task Is_on_multiple_contradictory_types()
    {
        await base.Is_on_multiple_contradictory_types();

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
WHERE [l0].[Id] IS NOT NULL AND [l1].[Id] IS NOT NULL
""");
    }

//     public override async Task OfType_on_multiple_contradictory_types()
//     {
//         await base.OfType_on_multiple_contradictory_types();

//         AssertSql();
//     }

//     public override async Task Is_and_OfType_with_multiple_contradictory_types()
//     {
//         await base.Is_and_OfType_with_multiple_contradictory_types();

//         AssertSql(
//             """
// SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], CASE
//     WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
// END AS [Discriminator]
// FROM [Animals] AS [a]
// LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
// LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
// LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
// WHERE [k].[Id] IS NOT NULL AND [e].[Id] IS NOT NULL
// """);
//     }

    public override async Task Primitive_collection_on_subtype()
    {
        await base.Primitive_collection_on_subtype();

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
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([l0].[Ints]) AS [i0])
""");
    }

    // public override void Using_from_sql_throws()
    // {
    //     base.Using_from_sql_throws();

    //     AssertSql();
    // }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
