// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

#nullable disable

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

    public override Task Delete_where_hierarchy()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            base.Delete_where_hierarchy);

    public override Task Delete_where_hierarchy_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            base.Delete_where_hierarchy_subquery);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", "Animal"),
            base.Delete_GroupBy_Where_Select_First_3);

    // Keyless entities are mapped as TPH only
    public override Task Update_where_keyless_entity_mapped_to_sql_query()
        => Task.CompletedTask;

    public override Task Update_base_type()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Animal"),
            base.Update_base_type);

    public override Task Update_base_type_with_OfType()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", "Animal"),
            base.Update_base_type_with_OfType);
}
