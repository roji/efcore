// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public abstract class OwnedJsonRelationshipsRelationalFixtureBase : OwnedNavigationsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.OptionalReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.RequiredReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRoot>().OwnsMany(x => x.CollectionTrunk).ToJson();
    }
}
