// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPTFiltersInheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : FiltersInheritanceBulkUpdatesRelationalTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    // // Keyless entities are mapped as TPH only
    // public override Task Delete_where_keyless_entity_mapped_to_sql_query()
    //     => Task.CompletedTask;

    // public override Task Delete_where_hierarchy()
    //     => AssertTranslationFailed(
    //         RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
    //         base.Delete_where_hierarchy);

    // public override Task Delete_where_hierarchy_subquery()
    //     => AssertTranslationFailed(
    //         RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
    //         base.Delete_where_hierarchy_subquery);

    // public override Task Delete_where_hierarchy_derived()
    //     => AssertTranslationFailed(
    //         RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Kiwi"),
    //         base.Delete_where_hierarchy_derived);

    // public override Task Delete_GroupBy_Where_Select_First_3()
    //     => AssertTranslationFailed(
    //         RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", "Animal"),
    //         base.Delete_GroupBy_Where_Select_First_3);

    // public override Task Update_base_and_derived_types()
    //     => AssertTranslationFailed(
    //         RelationalStrings.MultipleTablesInExecuteUpdate("k => k.FoundOn", "k => k.Name"),
    //         base.Update_base_and_derived_types);

    // // Keyless entities are mapped as TPH only
    // public override Task Update_where_keyless_entity_mapped_to_sql_query()
    //     => Task.CompletedTask;
}
