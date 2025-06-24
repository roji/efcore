// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public abstract class OwnedJsonProjectionRelationalTestBase<TFixture>(TFixture fixture)
    : OwnedNavigationsProjectionTestBase<TFixture>(fixture)
        where TFixture : OwnedJsonRelationshipsRelationalFixtureBase, new()
{
}
