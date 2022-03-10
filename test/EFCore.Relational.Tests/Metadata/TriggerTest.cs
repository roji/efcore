// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

public class TriggerTest
{
    [ConditionalFact]
    public void AddTrigger_with_duplicate_names_throws_exception()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        entityType.AddTrigger("SomeTrigger", "SomeTable", null);

        Assert.Equal(
            RelationalStrings.DuplicateTrigger("SomeTrigger", entityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddTrigger("SomeTrigger", "SomeTable")).Message);
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_trigger_when_trigger_exists()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        var constraint = entityType.AddTrigger("SomeTrigger", "SomeTable");

        Assert.Same(constraint, entityType.RemoveTrigger("SomeTrigger"));
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_null_when_trigger_is_missing()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        Assert.Null(entityType.RemoveTrigger("SomeTrigger"));
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => RelationalTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
