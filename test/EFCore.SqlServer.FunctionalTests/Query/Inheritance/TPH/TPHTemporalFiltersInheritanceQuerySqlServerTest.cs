// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPH;

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TPHTemporalFiltersInheritanceQuerySqlServerTest : FiltersInheritanceQueryTestBase<
    TPHTemporalFiltersInheritanceQuerySqlServerFixture>
{
    public TPHTemporalFiltersInheritanceQuerySqlServerTest(
        TPHTemporalFiltersInheritanceQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var temporalEntityTypes = new List<Type>
        {
            // typeof(Animal),
            // typeof(Plant),
            // typeof(Country),
            // typeof(Drink),
        };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    public override async Task Query_leaf1()
    {
        await base.Query_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] <> 8
""");
    }

    public override async Task OfType_root_via_root()
    {
        await base.OfType_root_via_root();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8
""");
    }

    public override async Task OfType_leaf1()
    {
        await base.OfType_leaf1();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task OfType_leaf_and_project_scalar()
    {
        await base.OfType_leaf_and_project_scalar();

        AssertSql(
            """
SELECT [r].[Leaf1Int]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Predicate_on_root_and_OfType_leaf()
    {
        await base.Predicate_on_root_and_OfType_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[IntermediateInt], [r].[Ints], [r].[Leaf1Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[RootInt] = 8 AND [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_leaf()
    {
        await base.Is_leaf();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Discriminator] = N'Leaf1'
""");
    }

    public override async Task Is_with_other_predicate()
    {
        await base.Is_with_other_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Discriminator], [r].[RootInt], [r].[RootReferencingEntityId], [r].[UniqueId], [r].[ConcreteIntermediateInt], [r].[IntermediateInt], [r].[Leaf3Int], [r].[Ints], [r].[Leaf1Int], [r].[Leaf2Int], [r].[ComplexTypeCollection], [r].[ParentComplexType_Int], [r].[ParentComplexType_UniqueId], [r].[ParentComplexType_Nested_Int], [r].[ParentComplexType_Nested_UniqueId], [r].[ChildComplexType_Int], [r].[ChildComplexType_UniqueId], [r].[ChildComplexType_Nested_Int], [r].[ChildComplexType_Nested_UniqueId], [r].[Leaf2_ChildComplexType_Int], [r].[Leaf2_ChildComplexType_UniqueId], [r].[Leaf2_ChildComplexType_Nested_Int], [r].[Leaf2_ChildComplexType_Nested_UniqueId]
FROM [Roots] AS [r]
WHERE [r].[RootInt] <> 8 AND [r].[Discriminator] = N'Leaf1' AND [r].[RootInt] = 8
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
WHERE [r].[RootInt] <> 8
""");
    }

    public override Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
        => Task.CompletedTask;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
