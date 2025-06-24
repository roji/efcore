// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedProjectionSqlServerTest
    : OwnedNavigationsProjectionRelationalTestBase<OwnedRelationshipsSqlServerFixture>
{
    public OwnedProjectionSqlServerTest(OwnedRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r3].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r4].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
    ) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
    END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r10].[RelationshipsBranchId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkRelationshipsRootId] = [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r9].[Id1] = [r10].[RelationshipsBranchId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
    }

    public override Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_optional(async, queryTrackingBehavior));

    public override Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_required(async, queryTrackingBehavior));

    public override Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_collection(async, queryTrackingBehavior));

    public override Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_required(async, queryTrackingBehavior));

    public override Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_optional(async, queryTrackingBehavior));

    public override Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_required(async, queryTrackingBehavior));

    public override Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_optional(async, queryTrackingBehavior));

    public override Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_collection(async, queryTrackingBehavior));

    public override Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_collection(async, queryTrackingBehavior));

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RelationshipsRootId], [s4].[Id1], [s4].[Name], [s4].[RelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsTrunkId1], [s4].[Id10], [s4].[Name0], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsBranchRelationshipsTrunkId1], [s4].[RelationshipsBranchId1], [s4].[Id100], [s4].[Name00], [s4].[OptionalReferenceLeaf_Name], [s4].[RequiredReferenceLeaf_Name], [s4].[OptionalReferenceBranch_Name], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s4].[RelationshipsBranchRelationshipsTrunkId10], [s4].[Id11], [s4].[Name1], [s4].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s4].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RequiredReferenceBranch_Name], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s4].[RelationshipsBranchRelationshipsTrunkId11], [s4].[Id12], [s4].[Name2], [s4].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s4].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s5].[RelationshipsTrunkRelationshipsRootId], [s5].[Id1], [s5].[Name], [s5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s5].[RelationshipsBranchId1], [s5].[Id10], [s5].[Name0], [s5].[OptionalReferenceLeaf_Name], [s5].[RequiredReferenceLeaf_Name], [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r20].[Id1], [r20].[Name], [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r21].[Id1], [r21].[Name], [s6].[RelationshipsTrunkRelationshipsRootId], [s6].[Id1], [s6].[Name], [s6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s6].[RelationshipsBranchId1], [s6].[Id10], [s6].[Name0], [s6].[OptionalReferenceLeaf_Name], [s6].[RequiredReferenceLeaf_Name], [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r24].[Id1], [r24].[Name], [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r25].[Id1], [r25].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r3].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r4].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
    ) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
    END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r10].[RelationshipsBranchId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkRelationshipsRootId] = [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r9].[Id1] = [r10].[RelationshipsBranchId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r13].[RelationshipsRootId], [r13].[Id1], [r13].[Name], [s3].[RelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsTrunkId1], [s3].[Id1] AS [Id10], [s3].[Name] AS [Name0], [s3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsBranchRelationshipsTrunkId1], [s3].[RelationshipsBranchId1], [s3].[Id10] AS [Id100], [s3].[Name0] AS [Name00], [s3].[OptionalReferenceLeaf_Name], [s3].[RequiredReferenceLeaf_Name], [r13].[OptionalReferenceBranch_Name], [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r16].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r16].[Id1] AS [Id11], [r16].[Name] AS [Name1], [r13].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r13].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r13].[RequiredReferenceBranch_Name], [r17].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r17].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r17].[Id1] AS [Id12], [r17].[Name] AS [Name2], [r13].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r13].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r13]
    LEFT JOIN (
        SELECT [r14].[RelationshipsTrunkRelationshipsRootId], [r14].[RelationshipsTrunkId1], [r14].[Id1], [r14].[Name], [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r15].[RelationshipsBranchRelationshipsTrunkId1], [r15].[RelationshipsBranchId1], [r15].[Id1] AS [Id10], [r15].[Name] AS [Name0], [r14].[OptionalReferenceLeaf_Name], [r14].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r14]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r15] ON [r14].[RelationshipsTrunkRelationshipsRootId] = [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r14].[RelationshipsTrunkId1] = [r15].[RelationshipsBranchRelationshipsTrunkId1] AND [r14].[Id1] = [r15].[RelationshipsBranchId1]
    ) AS [s3] ON [r13].[RelationshipsRootId] = [s3].[RelationshipsTrunkRelationshipsRootId] AND [r13].[Id1] = [s3].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r16] ON CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[RelationshipsRootId]
    END = [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[Id1]
    END = [r16].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r17] ON [r13].[RelationshipsRootId] = [r17].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r13].[Id1] = [r17].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s4] ON [r].[Id] = [s4].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r18].[RelationshipsTrunkRelationshipsRootId], [r18].[Id1], [r18].[Name], [r19].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r19].[RelationshipsBranchId1], [r19].[Id1] AS [Id10], [r19].[Name] AS [Name0], [r18].[OptionalReferenceLeaf_Name], [r18].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r18]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r19] ON [r18].[RelationshipsTrunkRelationshipsRootId] = [r19].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r18].[Id1] = [r19].[RelationshipsBranchId1]
) AS [s5] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s5].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r20] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r21] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r22].[RelationshipsTrunkRelationshipsRootId], [r22].[Id1], [r22].[Name], [r23].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r23].[RelationshipsBranchId1], [r23].[Id1] AS [Id10], [r23].[Name] AS [Name0], [r22].[OptionalReferenceLeaf_Name], [r22].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r22]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r23] ON [r22].[RelationshipsTrunkRelationshipsRootId] = [r23].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r22].[Id1] = [r23].[RelationshipsBranchId1]
) AS [s6] ON [r].[Id] = [s6].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r24] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r25] ON [r].[Id] = [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [s4].[RelationshipsRootId], [s4].[Id1], [s4].[RelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsTrunkId1], [s4].[Id10], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsBranchRelationshipsTrunkId1], [s4].[RelationshipsBranchId1], [s4].[Id100], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s4].[RelationshipsBranchRelationshipsTrunkId10], [s4].[Id11], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s4].[RelationshipsBranchRelationshipsTrunkId11], [s4].[Id12], [s5].[RelationshipsTrunkRelationshipsRootId], [s5].[Id1], [s5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s5].[RelationshipsBranchId1], [s5].[Id10], [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r20].[Id1], [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r21].[Id1], [s6].[RelationshipsTrunkRelationshipsRootId], [s6].[Id1], [s6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s6].[RelationshipsBranchId1], [s6].[Id10], [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r24].[Id1], [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
    }

    public override Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior));

    public override Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior));

    public override Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_leaf_trunk_root(async, queryTrackingBehavior));

    public override Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_multiple_branch_leaf(async, queryTrackingBehavior));

    #endregion Multiple

    #region Subquery

    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior));

    #endregion Subquery

    #region SelectMany

    public override Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_trunk_collection(async, queryTrackingBehavior));

    public override Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior));

    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior));

    #endregion SelectMany

    private async Task AssertCantTrackOwned(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
    {
        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            return;
        }

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
