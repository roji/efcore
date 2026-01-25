// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPHInheritanceBulkUpdatesFixture : InheritanceBulkUpdatesRelationalFixtureBase
{
    protected override string StoreName
        => "TPHInheritanceBulkUpdatesTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // TODO: Understand the complete discriminator mapping thing
        // TODO: Map to same column
        // modelBuilder.Entity<Animal>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);
        // modelBuilder.Entity<Coke>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");
        // modelBuilder.Entity<Lilt>().Property(e => e.SugarGrams).HasColumnName("SugarGrams");

        // modelBuilder.Entity<AnimalQuery>().HasNoKey().ToSqlQuery("SELECT * FROM Animals");
        // modelBuilder.Entity<KiwiQuery>().HasDiscriminator().HasValue("Kiwi");
        // modelBuilder.Entity<EagleQuery>().HasDiscriminator().HasValue("Eagle");
    }
}
