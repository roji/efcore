// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPT;

public class TPTFiltersInheritanceQuerySqlServerTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQuerySqlServerFixture>
{
    public TPTFiltersInheritanceQuerySqlServerTest(
        TPTFiltersInheritanceQuerySqlServerFixture fixture,
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
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [r].[RootInt] <> 8
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
WHERE [r].[RootInt] <> 8
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
WHERE [r].[RootInt] <> 8 AND [l].[Id] IS NOT NULL
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
WHERE [r].[RootInt] <> 8 AND [l].[Id] IS NOT NULL
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.Predicate_on_root_and_OfType_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId], CASE
    WHEN [l].[Id] IS NOT NULL THEN N'Leaf1'
END AS [Discriminator]
FROM [Roots] AS [r]
LEFT JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
LEFT JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [r].[RootInt] <> 8 AND [r].[RootInt] = 8 AND [l].[Id] IS NOT NULL
""");
    }

    public override async Task Is_leaf()
    {
        await base.Is_leaf();

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
WHERE [r].[RootInt] <> 8 AND [l0].[Id] IS NOT NULL
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
WHERE [r].[RootInt] <> 8 AND [l0].[Id] IS NOT NULL AND [r].[RootInt] = 8
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
WHERE [r].[RootInt] <> 8
""");
    }

    public override async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        await base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues();

        AssertSql(
            """
SELECT TOP(2) [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [r].[RootInt] = 8
""",
            //
            """
@p='4'

SELECT TOP(1) [r].[Id], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [i].[IntermediateInt], [l].[Ints], [l].[Leaf1Int], [l].[ChildComplexType_Int], [l].[ChildComplexType_UniqueId], [l].[ChildComplexType_Nested_Int], [l].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
INNER JOIN [Intermediate] AS [i] ON [r].[Id] = [i].[Id]
INNER JOIN [Leaf1] AS [l] ON [r].[Id] = [l].[Id]
WHERE [r].[Id] = @p
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
