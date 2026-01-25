// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPC;

public class TPCFiltersInheritanceQuerySqlServerTest : TPCFiltersInheritanceQueryTestBase<TPCFiltersInheritanceQuerySqlServerFixture>
{
    public TPCFiltersInheritanceQuerySqlServerTest(
        TPCFiltersInheritanceQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] <> 8
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
WHERE [u].[RootInt] <> 8
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
WHERE [u].[RootInt] <> 8
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT [u].[Leaf1Int]
FROM (
    SELECT [l].[RootInt], [l].[Leaf1Int]
    FROM [Leaf1] AS [l]
) AS [u]
WHERE [u].[RootInt] <> 8
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
WHERE [u].[RootInt] <> 8 AND [u].[RootInt] = 8
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
WHERE [u].[RootInt] <> 8
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
WHERE [u].[RootInt] <> 8 AND [u].[Discriminator] = N'Leaf1' AND [u].[RootInt] = 8
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
    SELECT [r].[RootInt], N'Root' AS [Discriminator]
    FROM [Roots] AS [r]
    UNION ALL
    SELECT [c].[RootInt], N'ConcreteIntermediate' AS [Discriminator]
    FROM [ConcreteIntermediate] AS [c]
    UNION ALL
    SELECT [i].[RootInt], N'Intermediate' AS [Discriminator]
    FROM [Intermediate] AS [i]
    UNION ALL
    SELECT [l].[RootInt], N'Leaf3' AS [Discriminator]
    FROM [Leaf3] AS [l]
    UNION ALL
    SELECT [l0].[RootInt], N'Leaf1' AS [Discriminator]
    FROM [Leaf1] AS [l0]
    UNION ALL
    SELECT [l1].[RootInt], N'Leaf2' AS [Discriminator]
    FROM [Leaf2] AS [l1]
) AS [u]
WHERE [u].[RootInt] <> 8
""");
    }

    public override async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        await base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues();

        AssertSql(
            """
SELECT TOP(2) [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ComplexTypeCollection], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
WHERE [l].[RootInt] = 8
""",
            //
            """
@p='1'

SELECT TOP(1) [l].[Id], [l].[RootInt], [l].[RootReferencingEntityId], [l].[UniqueId], [l].[Int], [l].[ComplexType_UniqueId], [l].[NestedComplexType_Int], [l].[NestedComplexType_UniqueId], [l].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Leaf1] AS [l]
WHERE [l].[Id] = @p
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
