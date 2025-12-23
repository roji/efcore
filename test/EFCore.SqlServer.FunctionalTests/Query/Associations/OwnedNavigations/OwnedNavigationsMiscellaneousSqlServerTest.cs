// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsMiscellaneousSqlServerTest(
    OwnedNavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedNavigationsMiscellaneousRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[AssociateTypeRootEntityId], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r5].[Id] AS [Id0], [r5].[Int] AS [Int0], [r5].[Ints] AS [Ints0], [r5].[Name] AS [Name0], [r5].[String] AS [String0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Ints] AS [Ints1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Ints] AS [Ints2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
WHERE [r0].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r3].[AssociateTypeRootEntityId], [r3].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1]
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[AssociateTypeRootEntityId], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r5].[Id] AS [Id0], [r5].[Int] AS [Int0], [r5].[Ints] AS [Ints0], [r5].[Name] AS [Name0], [r5].[String] AS [String0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Ints] AS [Ints1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Ints] AS [Ints2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
WHERE [o].[Int] = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r3].[AssociateTypeRootEntityId], [r3].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[AssociateTypeRootEntityId], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r5].[Id] AS [Id0], [r5].[Int] AS [Int0], [r5].[Ints] AS [Ints0], [r5].[Name] AS [Name0], [r5].[String] AS [String0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Ints] AS [Ints1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Ints] AS [Ints2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
WHERE [r1].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r3].[AssociateTypeRootEntityId], [r3].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1]
""");
    }

    #endregion Simple filters

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[AssociateTypeRootEntityId], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r].[RootEntityId], [r0].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r].[Id], [r].[Int], [r].[Ints], [r].[Name], [r].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
LEFT JOIN [OptionalRelated] AS [o] ON [m].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r] ON [m].[Id] = [r].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r0] ON [r].[RootEntityId] = [r0].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Ints] AS [Ints1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Ints] AS [Ints2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [m].[Id] = [s].[RootEntityId]
ORDER BY [m].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r].[RootEntityId], [r0].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r2].[AssociateTypeRootEntityId], [r2].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
