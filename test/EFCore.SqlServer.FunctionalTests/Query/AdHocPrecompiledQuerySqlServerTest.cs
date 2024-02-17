// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqlServerTest(ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlServerPrecompiledQueryTestHelpers.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        // TODO: Figure out if there's a nice way to continue using the retrying strategy
        var sqlServerOptionsBuilder = new SqlServerDbContextOptionsBuilder(builder);
        sqlServerOptionsBuilder.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        return builder;
    }
}
