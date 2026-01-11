// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPTInheritanceBulkUpdatesTestBase<TFixture> : InheritanceBulkUpdatesRelationalTestBase<TFixture>
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    protected TPTInheritanceBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        ClearLog();
    }

    // Keyless entities are mapped as TPH only
    public override Task Delete_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;

    public override Task Delete_on_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Root"),
            base.Delete_on_root);

    public override Task Delete_on_root_with_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Root"),
            base.Delete_on_root_with_subquery);

    public override Task Delete_on_leaf()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Leaf1"),
            base.Delete_on_leaf);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Root"),
            base.Delete_GroupBy_Where_Select_First_3);

    [ConditionalFact(Skip = "FK constraint issue")]
    public override Task Delete_entity_type_referencing_hierarchy()
        => base.Delete_entity_type_referencing_hierarchy();

    public override Task Update_both_root_and_leaf_properties()
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("l => l.Leaf1Int", "l => l.RootInt"),
            base.Update_both_root_and_leaf_properties);

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;
}
