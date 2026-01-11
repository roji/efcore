// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPC;

public abstract class TPCInheritanceQuerySqlServerTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceQueryTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : TPCInheritanceQuerySqlServerFixtureBase, new()
{
    public override async Task Query_root()
    {
        await base.Query_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Query_intermediate()
    {
        await base.Query_intermediate();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[Int], [u].[ComplexType_UniqueId], [u].[NestedComplexType_Int], [u].[NestedComplexType_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int], [i].[ComplexType_UniqueId], [i].[NestedComplexType_Int], [i].[NestedComplexType_UniqueId], [i].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int], [l0].[ComplexType_UniqueId], [l0].[NestedComplexType_Int], [l0].[NestedComplexType_UniqueId], [l0].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l0].[Leaf2Int], [l0].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l0].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l0].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l0].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l0]
) AS [u]
""");
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Query_leaf2()
    {
        await base.Query_leaf2();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Leaf2Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf2] AS [l]
""");
    }

    public override async Task Filter_root()
    {
        await base.Filter_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE [u].[RootInt] = 8
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
SELECT [l].[RootInt]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Project_scalar_from_root_via_root()
    {
        await base.Project_scalar_from_root_via_root();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
UNION ALL
SELECT [c].[RootInt]
FROM [ConcreteIntermediate] AS [c]
UNION ALL
SELECT [i].[RootInt]
FROM [Intermediate] AS [i]
UNION ALL
SELECT [l].[RootInt]
FROM [Leaf3] AS [l]
UNION ALL
SELECT [l0].[RootInt]
FROM [Leaf1] AS [l0]
UNION ALL
SELECT [l1].[RootInt]
FROM [Leaf2] AS [l1]
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task OfType_root_via_leaf()
    {
        await base.OfType_root_via_leaf();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task OfType_intermediate()
    {
        await base.OfType_intermediate();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [i].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l0].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l0].[Leaf2Int], [l0].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l0].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l0].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l0].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l0]
) AS [u]
""");
    }

    public override async Task OfType_leaf1()
    {
        await base.OfType_leaf1();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
""");
    }

    public override async Task OfType_leaf2()
    {
        await base.OfType_leaf2();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Leaf2Int], [l].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l]
) AS [u]
""");
    }

    public override async Task OfType_leaf_with_predicate_on_leaf()
    {
        await base.OfType_leaf_with_predicate_on_leaf();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
WHERE [u].[Leaf1Int] = 1000
""");
    }

    public override async Task OfType_leaf_with_predicate_on_root()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
WHERE [u].[RootInt] = 8
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.Predicate_on_root_and_OfType_leaf();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
WHERE [u].[RootInt] = 8
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT [l].[Leaf1Int]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task OfType_OrderBy_First()
    {
        await base.OfType_OrderBy_First();

        AssertSql(
            """
SELECT TOP(1) [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
ORDER BY [u].[Leaf1Int]
""");
    }

    public override async Task OfType_in_subquery()
    {
        await base.OfType_in_subquery();

        AssertSql(
            """
@p='5'

SELECT DISTINCT [u0].[Id], [u0].[RootInt], [u0].[RootReferencingEntityId], [u0].[UniqueId], [u0].[IntermediateInt], [u0].[Ints], [u0].[Leaf1Int], [u0].[c], [u0].[Int], [u0].[ComplexType_UniqueId], [u0].[NestedComplexType_Int], [u0].[NestedComplexType_UniqueId], [u0].[ChildComplexType_Int], [u0].[ChildComplexType_UniqueId], [u0].[ChildComplexType_Nested_Int], [u0].[ChildComplexType_Nested_UniqueId], [u0].[Discriminator]
FROM (
    SELECT TOP(@p) [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[IntermediateInt], [u].[Ints], [u].[Leaf1Int], [u].[ComplexTypeCollection] AS [c], [u].[Int], [u].[ComplexType_UniqueId], [u].[NestedComplexType_Int], [u].[NestedComplexType_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[Discriminator]
    FROM (
        SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int], [i].[ComplexType_UniqueId], [i].[NestedComplexType_Int], [i].[NestedComplexType_UniqueId], [i].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], N'Intermediate' AS [Discriminator]
        FROM [Intermediate] AS [i]
        UNION ALL
        SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], N'Leaf1' AS [Discriminator]
        FROM [Leaf1] AS [l]
        UNION ALL
        SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int], [l0].[ComplexType_UniqueId], [l0].[NestedComplexType_Int], [l0].[NestedComplexType_UniqueId], [l0].[IntermediateInt], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], N'Leaf2' AS [Discriminator]
        FROM [Leaf2] AS [l0]
    ) AS [u]
    ORDER BY [u].[Id]
) AS [u0]
WHERE [u0].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_root_via_root()
    {
        await base.Is_root_via_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Is_root_via_leaf()
    {
        await base.Is_root_via_leaf();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Is_intermediate()
    {
        await base.Is_intermediate();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l].[IntermediateInt], NULL AS [Leaf3Int], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l0].[Leaf2Int], [l0].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l0].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l0].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l0].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l0]
) AS [u]
""");
    }

    public override async Task Is_leaf()
    {
        await base.Is_leaf();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l].[IntermediateInt], NULL AS [Leaf3Int], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
""");
    }

    public override async Task Is_leaf_via_leaf()
    {
        await base.Is_leaf_via_leaf();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Is_in_projection()
    {
        await base.Is_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [u].[Discriminator] = N'Leaf1' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE [u].[Discriminator] = N'Leaf1' AND [u].[RootInt] = 8
""");
    }

    public override async Task Is_on_subquery_result()
    {
        await base.Is_on_subquery_result();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [u0].[Discriminator]
        FROM (
            SELECT [r0].[UniqueId], N'Root' AS [Discriminator]
            FROM [Roots] AS [r0]
            UNION ALL
            SELECT [c0].[UniqueId], N'ConcreteIntermediate' AS [Discriminator]
            FROM [ConcreteIntermediate] AS [c0]
            UNION ALL
            SELECT [i0].[UniqueId], N'Intermediate' AS [Discriminator]
            FROM [Intermediate] AS [i0]
            UNION ALL
            SELECT [l2].[UniqueId], N'Leaf3' AS [Discriminator]
            FROM [Leaf3] AS [l2]
            UNION ALL
            SELECT [l3].[UniqueId], N'Leaf1' AS [Discriminator]
            FROM [Leaf1] AS [l3]
            UNION ALL
            SELECT [l4].[UniqueId], N'Leaf2' AS [Discriminator]
            FROM [Leaf2] AS [l4]
        ) AS [u0]
        WHERE [u0].[UniqueId] = [u].[UniqueId]
    ) AS [u1]
    WHERE [u1].[Discriminator] = N'Leaf1')
""");
    }

    public override async Task Include_root()
    {
        await base.Include_root();

        AssertSql(
            """
SELECT [r].[Id], [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[RootInt], [r0].[RootReferencingEntityId], [r0].[UniqueId], [r0].[ComplexTypeCollection], [r0].[ParentComplexType_Int], [r0].[ParentComplexType_UniqueId], [r0].[ParentComplexType_Nested_Int], [r0].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r0]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u] ON [r].[Id] = [u].[RootReferencingEntityId]
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
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
) AS [u]
""");
    }

    public override async Task GetType_abstract_intermediate()
    {
        await base.GetType_abstract_intermediate();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
) AS [u]
""");
    }

    public override async Task GetType_leaf1()
    {
        await base.GetType_leaf1();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l].[IntermediateInt], NULL AS [Leaf3Int], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
""");
    }

    public override async Task GetType_leaf2()
    {
        await base.GetType_leaf2();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l].[Leaf2Int], [l].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l]
) AS [u]
""");
    }

    public override async Task GetType_leaf_reverse_equality()
    {
        await base.GetType_leaf_reverse_equality();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l].[IntermediateInt], NULL AS [Leaf3Int], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l]
) AS [u]
""");
    }

    public override async Task GetType_not_leaf1()
    {
        await base.GetType_not_leaf1();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE [u].[Discriminator] <> N'Leaf1'
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

    public override async Task Union_siblings_with_duplicate_property_in_subquery()
    {
        await base.Union_siblings_with_duplicate_property_in_subquery();

        AssertSql();
    }

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
    WHEN [u].[Discriminator] = N'Leaf1' AND [u].[Leaf1Int] = 50 AND [u].[Leaf1Int] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM (
    SELECT NULL AS [Leaf1Int], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT NULL AS [Leaf1Int], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT NULL AS [Leaf1Int], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT NULL AS [Leaf1Int], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Leaf1Int], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT NULL AS [Leaf1Int], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Is_on_multiple_contradictory_types()
    {
        await base.Is_on_multiple_contradictory_types();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE 0 = 1
""");
    }

    public override async Task OfType_on_multiple_contradictory_types()
    {
        await base.OfType_on_multiple_contradictory_types();

        AssertSql();
    }

    public override async Task Is_and_OfType_with_multiple_contradictory_types()
    {
        await base.Is_and_OfType_with_multiple_contradictory_types();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Discriminator] = N'Eagle'
""");
    }

    public override async Task Primitive_collection_on_subtype()
    {
        await base.Primitive_collection_on_subtype();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueId], [u].[ParentComplexType_Nested_Int], [u].[ParentComplexType_Nested_UniqueId], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueId], [u].[ChildComplexType_Nested_Int], [u].[ChildComplexType_Nested_UniqueId], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueId1], [u].[ChildComplexType_Nested_Int1], [u].[ChildComplexType_Nested_UniqueId1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [c].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [c].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[Int] AS [ParentComplexType_Int], [i].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [i].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [i].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[Int] AS [ParentComplexType_Int], [l0].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l0].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l0].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType_Int], [l0].[ChildComplexType_UniqueId], [l0].[ChildComplexType_Nested_Int], [l0].[ChildComplexType_Nested_UniqueId], NULL AS [Leaf2Int], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueId1], NULL AS [ChildComplexType_Nested_Int1], NULL AS [ChildComplexType_Nested_UniqueId1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[Int] AS [ParentComplexType_Int], [l1].[ComplexType_UniqueId] AS [ParentComplexType_UniqueId], [l1].[NestedComplexType_Int] AS [ParentComplexType_Nested_Int], [l1].[NestedComplexType_UniqueId] AS [ParentComplexType_Nested_UniqueId], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueId], NULL AS [ChildComplexType_Nested_Int], NULL AS [ChildComplexType_Nested_UniqueId], [l1].[Leaf2Int], [l1].[ChildComplexType_Int] AS [ChildComplexType_Int1], [l1].[ChildComplexType_UniqueId] AS [ChildComplexType_UniqueId1], [l1].[ChildComplexType_Nested_Int] AS [ChildComplexType_Nested_Int1], [l1].[ChildComplexType_Nested_UniqueId] AS [ChildComplexType_Nested_UniqueId1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([u].[Ints]) AS [i0])
""");
    }

    public override void Using_from_sql_throws()
    {
        base.Using_from_sql_throws();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType().BaseType!);
}
