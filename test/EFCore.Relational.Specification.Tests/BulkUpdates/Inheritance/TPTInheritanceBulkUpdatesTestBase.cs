// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

#nullable disable

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

    public override Task Delete_where_hierarchy()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            base.Delete_where_hierarchy);

    public override Task Delete_where_hierarchy_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            base.Delete_where_hierarchy_subquery);

    public override Task Delete_where_hierarchy_derived()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Kiwi"),
            base.Delete_where_hierarchy_derived);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
            base.Delete_GroupBy_Where_Select_First_3);

    [ConditionalFact(Skip = "FK constraint issue")]
    public override Task Delete_where_using_hierarchy()
        => base.Delete_where_using_hierarchy();

    [ConditionalFact(Skip = "FK constraint issue")]
    public override Task Delete_where_using_hierarchy_derived()
        => base.Delete_where_using_hierarchy_derived();

    public override Task Update_base_and_derived_types()
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("k => k.FoundOn", "k => k.Name"),
            base.Update_base_and_derived_types);

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;
}
