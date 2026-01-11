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

    public override async Task Can_use_of_type_animal()
    {
        await base.Can_use_of_type_animal();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_is_kiwi()
    {
        await base.Can_use_is_kiwi();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate()
    {
        await base.Can_use_is_kiwi_with_other_predicate();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1 AND [u].[Discriminator] = N'Kiwi' AND [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_is_kiwi_in_projection()
    {
        await base.Can_use_is_kiwi_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [u].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT [e].[CountryId], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[CountryId], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_bird()
    {
        await base.Can_use_of_type_bird();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate()
    {
        await base.Can_use_of_type_bird_predicate();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection()
    {
        await base.Can_use_of_type_bird_with_projection();

        AssertSql(
            """
SELECT [u].[Name]
FROM (
    SELECT [e].[CountryId], [e].[Name]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[CountryId], [k].[Name]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_bird_first()
    {
        await base.Can_use_of_type_bird_first();

        AssertSql(
            """
SELECT TOP(1) [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_kiwi()
    {
        await base.Can_use_of_type_kiwi();

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_derived_set()
    {
        await base.Can_use_derived_set();

        AssertSql(
            """
SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group]
FROM [Eagle] AS [e]
WHERE [e].[CountryId] = 1
""");
    }

    public override async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        await base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues();

        AssertSql(
            """
SELECT TOP(2) [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group]
FROM [Eagle] AS [e]
""",
            //
            """
@p='2'

SELECT TOP(1) [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group]
FROM [Eagle] AS [e]
WHERE [e].[Id] = @p
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
