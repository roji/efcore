// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

public abstract class PrecompiledQueryRelationalFixture
    : SharedStoreFixtureBase<PrecompiledQueryRelationalTestBase.PrecompiledQueryContext>
{
    protected override string StoreName
        => "PrecompiledQueryTest";

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddScoped<IQueryCompiler, NonCompilingQueryCompiler>();

    public abstract PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers { get; }
}
