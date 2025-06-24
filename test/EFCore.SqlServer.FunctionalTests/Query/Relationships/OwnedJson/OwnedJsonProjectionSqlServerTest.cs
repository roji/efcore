// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonProjectionSqlServerTest
    : OwnedJsonProjectionRelationalTestBase<OwnedJsonSqlServerFixture>
{
    public OwnedJsonProjectionSqlServerTest(OwnedJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_trunk_optional(async, queryTrackingBehavior));

    public override Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_trunk_required(async, queryTrackingBehavior));

    public override Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_trunk_collection(async, queryTrackingBehavior));

    public override Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_required_required(async, queryTrackingBehavior));

    public override Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_required_optional(async, queryTrackingBehavior));

    public override Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_optional_required(async, queryTrackingBehavior));

    public override Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_optional_optional(async, queryTrackingBehavior));

    public override Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_required_collection(async, queryTrackingBehavior));

    public override Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_branch_optional_collection(async, queryTrackingBehavior));

    public override Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_multiple_branch_leaf(async, queryTrackingBehavior));

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior));

    public override Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior));

    public override Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_leaf_trunk_root(async, queryTrackingBehavior));

    #endregion Multiple

    #region Subquery

    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior));

    #endregion Subquery

    #region SelectMany

    public override Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.SelectMany_trunk_collection(async, queryTrackingBehavior));

    public override Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior));

    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackJson(queryTrackingBehavior, () => base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior));

    #endregion SelectMany

    private async Task AssertCantTrackJson(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            return;
        }

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery("AsNoTracking"), message);
        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
