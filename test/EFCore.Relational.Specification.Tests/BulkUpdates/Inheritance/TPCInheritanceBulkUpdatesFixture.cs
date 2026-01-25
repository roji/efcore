// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPCInheritanceBulkUpdatesFixture : InheritanceBulkUpdatesRelationalFixtureBase
{
    protected override string StoreName
        => "TPCInheritanceBulkUpdatesTest";

    public override bool HasDiscriminator
        => false;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Root>().UseTpcMappingStrategy();

        // // Keyless entities are mapped to TPH so ignoring them
        // modelBuilder.Ignore<AnimalQuery>();
        // modelBuilder.Ignore<BirdQuery>();
        // modelBuilder.Ignore<KiwiQuery>();
        // modelBuilder.Ignore<EagleQuery>();
    }
}
