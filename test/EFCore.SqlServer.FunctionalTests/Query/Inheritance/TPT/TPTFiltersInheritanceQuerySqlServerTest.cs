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

    public override async Task Can_use_of_type_animal()
    {
        await base.Can_use_of_type_animal();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_is_kiwi()
    {
        await base.Can_use_is_kiwi();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND [k].[Id] IS NOT NULL
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate()
    {
        await base.Can_use_is_kiwi_with_other_predicate();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND [k].[Id] IS NOT NULL AND [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_is_kiwi_in_projection()
    {
        await base.Can_use_is_kiwi_in_projection();

        AssertSql(
            """
SELECT CASE
    WHEN [k].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_bird()
    {
        await base.Can_use_of_type_bird();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND ([k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL)
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate()
    {
        await base.Can_use_of_type_bird_predicate();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND ([k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL)
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection()
    {
        await base.Can_use_of_type_bird_with_projection();

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND ([k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL)
""");
    }

    public override async Task Can_use_of_type_bird_first()
    {
        await base.Can_use_of_type_bird_first();

        AssertSql(
            """
SELECT TOP(1) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND ([k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL)
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_kiwi()
    {
        await base.Can_use_of_type_kiwi();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND [k].[Id] IS NOT NULL
""");
    }

    public override async Task Can_use_derived_set()
    {
        await base.Can_use_derived_set();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
WHERE [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
    {
        await base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues();

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
""",
            //
            """
@p='1'

SELECT TOP(1) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
WHERE [a].[Id] = @p
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
