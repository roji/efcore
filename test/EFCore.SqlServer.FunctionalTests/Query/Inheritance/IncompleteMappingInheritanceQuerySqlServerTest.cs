// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance.TPH;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class IncompleteMappingInheritanceQuerySqlServerTest(
    IncompleteMappingInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceQueryTestBase<IncompleteMappingInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Query_root()
    {
        await base.Query_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Query_intermediate()
    {
        await base.Query_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Intermediate', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Query_leaf2()
    {
        await base.Query_leaf2();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf2'
""");
    }

    public override async Task Filter_root()
    {
        await base.Filter_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND [r].[RootInt] = 8
""");
    }

    public override async Task Project_scalar_from_leaf()
    {
        await base.Project_scalar_from_leaf();

        AssertSql(
            """
SELECT [r].[Leaf1Int]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
    {
        await base.Project_root_scalar_via_root_with_EF_Property_and_downcast();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Project_scalar_from_root_via_leaf()
    {
        await base.Project_scalar_from_root_via_leaf();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Project_scalar_from_root_via_root()
    {
        await base.Project_scalar_from_root_via_root();

        AssertSql(
            """
SELECT [r].[RootInt]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task OfType_root_via_leaf()
    {
        await base.OfType_root_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task OfType_intermediate()
    {
        await base.OfType_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Intermediate', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task OfType_leaf1()
    {
        await base.OfType_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task OfType_leaf2()
    {
        await base.OfType_leaf2();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf2'
""");
    }

    public override async Task OfType_leaf_with_predicate_on_leaf()
    {
        await base.OfType_leaf_with_predicate_on_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[Leaf1Int] = 1000
""");
    }

    public override async Task OfType_leaf_with_predicate_on_root()
    {
        await base.OfType_leaf_with_predicate_on_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] = 8
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.Predicate_on_root_and_OfType_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND [r].[RootInt] = 8 AND [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT [r].[Leaf1Int]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task OfType_OrderBy_First()
    {
        await base.OfType_OrderBy_First();

        AssertSql(
            """
SELECT TOP(1) [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
ORDER BY [r].[Leaf1Int]
""");
    }

    public override async Task OfType_in_subquery()
    {
        await base.OfType_in_subquery();

        AssertSql(
            """
@p='5'

SELECT DISTINCT [r0].[Id], [r0].[Discriminator], [r0].[RootInt], [r0].[RootReferencingEntityId], [r0].[UniqueId], [r0].[IntermediateInt], [r0].[Ints], [r0].[Leaf1Int], [r0].[c], [r0].[ParentComplexType_Int], [r0].[ParentComplexType_UniqueId], [r0].[ParentComplexType_Nested_Int], [r0].[ParentComplexType_Nested_UniqueId], [r0].[ChildComplexType_Int], [r0].[ChildComplexType_UniqueId], [r0].[ChildComplexType_Nested_Int], [r0].[ChildComplexType_Nested_UniqueId]
FROM (
    SELECT TOP(@p) [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection] AS [c], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
    FROM [Roots] AS [r]
    WHERE [r].[Discriminator] IN (N'Intermediate', N'Leaf1', N'Leaf2')
    ORDER BY [r].[Id]
) AS [r0]
WHERE [r0].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_root_via_root()
    {
        await base.Is_root_via_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Is_root_via_leaf()
    {
        await base.Is_root_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_intermediate()
    {
        await base.Is_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Intermediate', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Is_leaf_via_root()
    {
        await base.Is_leaf_via_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_leaf_via_leaf()
    {
        await base.Is_leaf_via_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_in_projection()
    {
        await base.Is_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [r].[Discriminator] = N'Leaf1' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] = 8
""");
    }

    public override async Task Is_on_subquery_result()
    {
        await base.Is_on_subquery_result();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND (
    SELECT TOP(1) [r0].[Discriminator]
    FROM [Roots] AS [r0]
    WHERE [r0].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND [r0].[UniqueId] = [r].[UniqueId]) = N'Leaf1'
""");
    }

    public override async Task Include_root()
    {
        await base.Include_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Int], [r1].[Id], [r1].[Discriminator], [r1].[RootInt], [r1].[RootReferencingEntityId], [r1].[UniqueId], [r1].[ConcreteIntermediateInt], [r1].[IntermediateInt], [r1].[Leaf3Int], [r1].[Ints], [r1].[Leaf1Int], [r1].[Leaf2Int], [r1].[c], [r1].[ParentComplexType_Int], [r1].[ParentComplexType_UniqueId], [r1].[ParentComplexType_Nested_Int], [r1].[ParentComplexType_Nested_UniqueId], [r1].[ChildComplexType_Int], [r1].[ChildComplexType_UniqueId], [r1].[ChildComplexType_Nested_Int], [r1].[ChildComplexType_Nested_UniqueId], [r1].[Leaf2_ChildComplexType_Int], [r1].[Leaf2_ChildComplexType_UniqueId], [r1].[Leaf2_ChildComplexType_Nested_Int], [r1].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [RootReferencingEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[Discriminator], [r0].[RootInt], [r0].[RootReferencingEntityId], [r0].[UniqueId], [r0].[ConcreteIntermediateInt], [r0].[IntermediateInt], [r0].[Leaf3Int], [r0].[Ints], [r0].[Leaf1Int], [r0].[Leaf2Int], [r0].[ComplexTypeCollection] AS [c], [r0].[ParentComplexType_Int], [r0].[ParentComplexType_UniqueId], [r0].[ParentComplexType_Nested_Int], [r0].[ParentComplexType_Nested_UniqueId], [r0].[ChildComplexType_Int], [r0].[ChildComplexType_UniqueId], [r0].[ChildComplexType_Nested_Int], [r0].[ChildComplexType_Nested_UniqueId], [r0].[Leaf2_ChildComplexType_Int], [r0].[Leaf2_ChildComplexType_UniqueId], [r0].[Leaf2_ChildComplexType_Nested_Int], [r0].[Leaf2_ChildComplexType_Nested_UniqueId]
    FROM [Roots] AS [r0]
    WHERE [r0].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
) AS [r1] ON [r].[Id] = [r1].[RootReferencingEntityId]
""");
    }

    public override async Task Filter_on_discriminator()
    {
        await base.Filter_on_discriminator();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Project_discriminator()
    {
        await base.Project_discriminator();

        AssertSql(
            """
SELECT [r].[Discriminator]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task GetType_abstract_root()
    {
        await base.GetType_abstract_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Root'
""");
    }

    public override async Task GetType_abstract_intermediate()
    {
        await base.GetType_abstract_intermediate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Intermediate'
""");
    }

    public override async Task GetType_leaf1()
    {
        await base.GetType_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task GetType_leaf2()
    {
        await base.GetType_leaf2();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf2'
""");
    }

    public override async Task GetType_leaf_reverse_equality()
    {
        await base.GetType_leaf_reverse_equality();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task GetType_not_leaf1()
    {
        await base.GetType_not_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND [r].[Discriminator] <> N'Leaf1'
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

        AssertSql(" ");
    }

//     public override async Task Union_siblings_with_duplicate_property_in_subquery()
//     {
//         await base.Union_siblings_with_duplicate_property_in_subquery();

//         AssertSql(
//             """
// SELECT [t].[Id], [t].[Discriminator], [t].[CaffeineGrams], [t].[CokeCO2], [t].[SugarGrams], [t].[Carbonation], [t].[SugarGrams0], [t].[CaffeineGrams0], [t].[HasMilk]
// FROM (
//     SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], NULL AS [CaffeineGrams0], NULL AS [HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
//     FROM [Drinks] AS [d]
//     WHERE [d].[Discriminator] = N'Coke'
//     UNION
//     SELECT [d0].[Id], [d0].[Discriminator], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], [d0].[CaffeineGrams] AS [CaffeineGrams0], [d0].[HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
//     FROM [Drinks] AS [d0]
//     WHERE [d0].[Discriminator] = N'Tea'
// ) AS [t]
// WHERE [t].[Id] > 0
// """);
//     }

    public override async Task Union_entity_equality()
    {
        await base.Union_entity_equality();

        AssertSql(
            """
SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn], NULL AS [Group]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] = N'Kiwi'
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], NULL AS [FoundOn], [a0].[Group]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] = N'Eagle'
) AS [t]
WHERE 0 = 1
""");
    }

    public override async Task Conditional_with_is_and_downcast_in_projection()
    {
        await base.Conditional_with_is_and_downcast_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [r].[Discriminator] = N'Leaf1' AND [r].[Leaf1Int] = 50 AND [r].[Leaf1Int] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2')
""");
    }

    public override async Task Is_on_multiple_contradictory_types()
    {
        await base.Is_on_multiple_contradictory_types();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE 0 = 1
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
// SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group]
// FROM [Animals] AS [a]
// WHERE 0 = 1
// """);
//     }

    public override async Task Primitive_collection_on_subtype()
    {
        await base.Primitive_collection_on_subtype();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] IN (N'Root', N'ConcreteIntermediate', N'Intermediate', N'Leaf3', N'Leaf1', N'Leaf2') AND EXISTS (
    SELECT 1
    FROM OPENJSON([r].[Ints]) AS [i])
""");
    }

    public override void FromSql_on_root()
    {
        base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[Species], [m].[EagleId], [m].[IsFlightless], [m].[Group], [m].[FoundOn]
FROM (
    select * from "Animals"
) AS [m]
WHERE [m].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override void FromSql_on_leaf()
    {
        base.FromSql_on_leaf();

        AssertSql(
            """
SELECT [m].[Id], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[Species], [m].[EagleId], [m].[IsFlightless], [m].[Group]
FROM (
    select * from "Animals"
) AS [m]
WHERE [m].[Discriminator] = N'Eagle'
""");
    }

//     public override void Casting_to_base_type_joining_with_query_type_works()
//     {
//         base.Casting_to_base_type_joining_with_query_type_works();

//         AssertSql(
//             """
// SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[EagleId], [m].[IsFlightless], [m].[Group], [m].[FoundOn]
// FROM [Animals] AS [a]
// INNER JOIN (
//     Select * from "Animals"
// ) AS [m] ON [a].[Name] = [m].[Name]
// WHERE [a].[Discriminator] = N'Eagle'
// """);
//     }

//     [ConditionalFact]
//     public virtual void Common_property_shares_column()
//     {
//         using var context = CreateContext();
//         var liltType = context.Model.FindEntityType(typeof(Lilt))!;
//         var cokeType = context.Model.FindEntityType(typeof(Coke))!;
//         var teaType = context.Model.FindEntityType(typeof(Tea))!;

//         Assert.Equal("SugarGrams", cokeType.FindProperty("SugarGrams")!.GetColumnName());
//         Assert.Equal("CaffeineGrams", cokeType.FindProperty("CaffeineGrams")!.GetColumnName());
//         Assert.Equal("CokeCO2", cokeType.FindProperty("Carbonation")!.GetColumnName());

//         Assert.Equal("SugarGrams", liltType.FindProperty("SugarGrams")!.GetColumnName());
//         Assert.Equal("LiltCO2", liltType.FindProperty("Carbonation")!.GetColumnName());

//         Assert.Equal("CaffeineGrams", teaType.FindProperty("CaffeineGrams")!.GetColumnName());
//         Assert.Equal("HasMilk", teaType.FindProperty("HasMilk")!.GetColumnName());
//     }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
