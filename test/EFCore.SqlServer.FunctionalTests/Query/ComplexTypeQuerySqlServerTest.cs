// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexTypeQuerySqlServerTest : ComplexTypeQueryRelationalTestBase<
    ComplexTypeQuerySqlServerTest.ComplexTypeQuerySqlServerFixture>
{
    public ComplexTypeQuerySqlServerTest(ComplexTypeQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Filter_on_property_inside_complex_type(bool async)
    {
        await base.Filter_on_property_inside_complex_type(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
WHERE [c].[ShippingAddress_ZipCode] = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_complex_type(bool async)
    {
        await base.Filter_on_property_inside_nested_complex_type(async);

        AssertSql(
"""
SELECT [c].[Id], [c].[Name], [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
WHERE [c].[ShippingAddress_Country_Code] = N'DE'
""");
    }

    public override async Task Filter_on_property_inside_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_complex_type_after_subquery(async);

        AssertSql(
"""
@__p_0='1'

SELECT DISTINCT [t].[Id], [t].[Name], [t].[ShippingAddress_AddressLine1], [t].[ShippingAddress_AddressLine2], [t].[ShippingAddress_ZipCode], [t].[ShippingAddress_Country_Code], [t].[ShippingAddress_Country_FullName]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
    FROM [Customer] AS [c]
    ORDER BY [c].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
WHERE [t].[ShippingAddress_ZipCode] = 7728
""");
    }

    public override async Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
    {
        await base.Filter_on_property_inside_nested_complex_type_after_subquery(async);

        AssertSql(
"""
@__p_0='1'

SELECT DISTINCT [t].[Id], [t].[Name], [t].[ShippingAddress_AddressLine1], [t].[ShippingAddress_AddressLine2], [t].[ShippingAddress_ZipCode], [t].[ShippingAddress_Country_Code], [t].[ShippingAddress_Country_FullName]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
    FROM [Customer] AS [c]
    ORDER BY [c].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
WHERE [t].[ShippingAddress_Country_Code] = N'DE'
""");
    }

    public override async Task Load_complex_type_after_subquery_on_entity_type(bool async)
    {
        await base.Load_complex_type_after_subquery_on_entity_type(async);

        AssertSql(
"""
@__p_0='1'

SELECT DISTINCT [t].[Id], [t].[Name], [t].[ShippingAddress_AddressLine1], [t].[ShippingAddress_AddressLine2], [t].[ShippingAddress_ZipCode], [t].[ShippingAddress_Country_Code], [t].[ShippingAddress_Country_FullName]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
    FROM [Customer] AS [c]
    ORDER BY [c].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
""");
    }

    public override async Task Project_complex_type(bool async)
    {
        await base.Project_complex_type(async);

        AssertSql(
"""
SELECT [c].[ShippingAddress_AddressLine1], [c].[ShippingAddress_AddressLine2], [c].[ShippingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
""");
    }

    public override async Task Project_nested_complex_type(bool async)
    {
        await base.Project_nested_complex_type(async);

        AssertSql(
"""
SELECT [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
""");
    }

    public override async Task Project_single_property_on_nested_complex_type(bool async)
    {
        await base.Project_single_property_on_nested_complex_type(async);

        AssertSql(
"""
SELECT [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class ComplexTypeQuerySqlServerFixture : RelationalComplexTypeQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        // TODO: Temporarily seeding via raw SQL as update pipeline support for complex types, see provider-specific implementations
        protected override void Seed(PoolableDbContext context)
            => context.Database.ExecuteSqlRaw(
"""
INSERT INTO Customer (Id, Name, ShippingAddress_AddressLine1, ShippingAddress_ZipCode, ShippingAddress_Country_FullName, ShippingAddress_Country_Code)
VALUES
    (1, 'Mona Cy', '804 S. Lakeshore Road', 38654, 'United States', 'US'),
    (2, 'Antigonus Mitul', '72 Hickory Rd.', 07728, 'Germany', 'DE')
""");
    }
}
