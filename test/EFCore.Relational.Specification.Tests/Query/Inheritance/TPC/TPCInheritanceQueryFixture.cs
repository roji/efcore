// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPC;

public abstract class TPCInheritanceQueryFixture : InheritanceQueryRelationalFixtureBase
{
    protected override string StoreName
        => "TPCInheritanceTest";

    public override bool HasDiscriminator
        => false;

    public override bool UseGeneratedKeys
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
