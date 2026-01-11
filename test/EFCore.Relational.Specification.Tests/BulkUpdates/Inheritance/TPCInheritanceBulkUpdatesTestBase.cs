// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPCInheritanceBulkUpdatesTestBase<TFixture> : InheritanceBulkUpdatesRelationalTestBase<TFixture>
    where TFixture : TPCInheritanceBulkUpdatesFixture, new()
{
    protected TPCInheritanceBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Keyless entities are mapped as TPH only
    public override Task Delete_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;

    public override Task Delete_on_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Root"),
            base.Delete_on_root);

    public override Task Delete_on_root_with_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Root"),
            base.Delete_on_root_with_subquery);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Root"),
            base.Delete_GroupBy_Where_Select_First_3);

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;

    public override Task Update_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Root"),
            base.Update_root);

    public override Task Update_with_OfType_leaf()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Root"),
            base.Update_with_OfType_leaf);
}
