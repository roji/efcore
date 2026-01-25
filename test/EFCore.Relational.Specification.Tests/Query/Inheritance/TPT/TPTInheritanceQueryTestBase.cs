// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance.TPT;

public abstract class TPTInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
    where TFixture : TPTInheritanceQueryFixture, new()
{
    public TPTInheritanceQueryTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // TPT does not have discriminator
    public override Task Project_scalar_from_leaf()
        => Task.CompletedTask;

    // TPT does not have discriminator
    public override Task Filter_on_discriminator()
        => Task.CompletedTask;

    // TPT does not have discriminator
    public override Task Project_discriminator()
        => Task.CompletedTask;

    // TPT does not have discriminator
    public override Task Project_root_scalar_via_root_with_EF_Property_and_downcast()
        => Task.CompletedTask;

    // [ConditionalFact]
    // public virtual void Using_from_sql_throws()
    // {
    //     using var context = CreateContext();

    //     var message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlRaw("Select * from Birds")).Message;

    //     Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSqlRaw", typeof(Bird).Name), message);

    //     message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlInterpolated($"Select * from Birds"))
    //         .Message;

    //     Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSqlInterpolated", typeof(Bird).Name), message);

    //     message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSql($"Select * from Birds"))
    //         .Message;

    //     Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSql", typeof(Bird).Name), message);
    // }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
