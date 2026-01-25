// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPH;

public abstract class TPHInheritanceJsonQueryRelationalFixtureBase : TPHInheritanceQueryFixture
{
    protected override string StoreName
        => "TPHInheritanceJsonTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Root>().ComplexProperty(d => d.ParentComplexType, d => d.ToJson().IsRequired(false));
        modelBuilder.Entity<Leaf1>().ComplexProperty(c => c.ChildComplexType, c => c.ToJson().IsRequired(false));
        modelBuilder.Entity<Leaf2>().ComplexProperty(t => t.ChildComplexType, t => t.ToJson().IsRequired(false));
    }
}
