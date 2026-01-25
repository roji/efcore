// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPC;

public class TPCInheritanceJsonQuerySqlServerTest(TPCInheritanceJsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceJsonQueryRelationalTestBase<TPCInheritanceJsonQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_leaf()
    {
        await base.Filter_on_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[ParentComplexType], [l].[ChildComplexType]
FROM [Leaf1] AS [l]
WHERE CAST(JSON_VALUE([l].[ChildComplexType], '$.Int') AS int) = 9
""");
    }

    public override async Task Filter_on_complex_type_property_on_root()
    {
        await base.Filter_on_complex_type_property_on_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[ParentComplexType], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], [l1].[Leaf2Int], [l1].[ChildComplexType] AS [ChildComplexType1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE CAST(JSON_VALUE([u].[ParentComplexType], '$.Int') AS int) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_leaf()
    {
        await base.Filter_on_nested_complex_type_property_on_leaf();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[ParentComplexType], [l].[ChildComplexType]
FROM [Leaf1] AS [l]
WHERE CAST(JSON_VALUE([l].[ChildComplexType], '$.Nested.Int') AS int) = 51
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_root()
    {
        await base.Filter_on_nested_complex_type_property_on_root();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[ParentComplexType], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], [l1].[Leaf2Int], [l1].[ChildComplexType] AS [ChildComplexType1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE CAST(JSON_VALUE([u].[ParentComplexType], '$.Nested.Int') AS int) = 50
""");
    }

    public override async Task Project_complex_type_on_leaf()
    {
        await base.Project_complex_type_on_leaf();

        AssertSql(
            """
SELECT [l].[ChildComplexType]
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Project_complex_type_on_root()
    {
        await base.Project_complex_type_on_root();

        AssertSql(
            """
SELECT [u].[ParentComplexType]
FROM (
    SELECT [r].[ParentComplexType]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[ParentComplexType]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[ParentComplexType]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[ParentComplexType]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[ParentComplexType]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[ParentComplexType]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Project_nested_complex_type_on_leaf()
    {
        await base.Project_nested_complex_type_on_leaf();

        AssertSql(
            """
SELECT JSON_QUERY([l].[ChildComplexType], '$.Nested')
FROM [Leaf1] AS [l]
""");
    }

    public override async Task Project_nested_complex_type_on_root()
    {
        await base.Project_nested_complex_type_on_root();

        AssertSql(
            """
SELECT JSON_QUERY([u].[ParentComplexType], '$.Nested')
FROM (
    SELECT [r].[ParentComplexType]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[ParentComplexType]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[ParentComplexType]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[ParentComplexType]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[ParentComplexType]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[ParentComplexType]
    FROM [Leaf2] AS [l1]
) AS [u]
""");
    }

    public override async Task Subquery_over_complex_collection()
    {
        await base.Subquery_over_complex_collection();

        AssertSql(
            """
SELECT [u].[Id], [u].[RootInt], [u].[RootReferencingEntityId], [u].[UniqueId], [u].[ConcreteIntermediateInt], [u].[IntermediateInt], [u].[Leaf3Int], [u].[Ints], [u].[Leaf1Int], [u].[Leaf2Int], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ComplexTypeCollection], [r].[ParentComplexType], NULL AS [ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[Id], [c].[RootInt], [c].[RootReferencingEntityId], [c].[UniqueId], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ConcreteIntermediateInt], NULL AS [IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[Id], [i].[RootInt], [i].[RootReferencingEntityId], [i].[UniqueId], [i].[ComplexTypeCollection], [i].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [i].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[ComplexTypeCollection], [l].[ParentComplexType], [l].[ConcreteIntermediateInt], NULL AS [IntermediateInt], [l].[Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[Id], [l0].[RootInt], [l0].[RootReferencingEntityId], [l0].[UniqueId], [l0].[ComplexTypeCollection], [l0].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l0].[IntermediateInt], NULL AS [Leaf3Int], [l0].[Ints], [l0].[Leaf1Int], [l0].[ChildComplexType], NULL AS [Leaf2Int], NULL AS [ChildComplexType1], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[Id], [l1].[RootInt], [l1].[RootReferencingEntityId], [l1].[UniqueId], [l1].[ComplexTypeCollection], [l1].[ParentComplexType], NULL AS [ConcreteIntermediateInt], [l1].[IntermediateInt], NULL AS [Leaf3Int], NULL AS [Ints], NULL AS [Leaf1Int], NULL AS [ChildComplexType], [l1].[Leaf2Int], [l1].[ChildComplexType] AS [ChildComplexType1], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([u].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c0]
    WHERE [c0].[Int] > 59) = 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
