// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPTFiltersInheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : FiltersInheritanceBulkUpdatesRelationalTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : TPTInheritanceBulkUpdatesFixture, new()
{
    public override Task Delete_on_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", nameof(Root)),
            base.Delete_on_root);

    public override Task Delete_on_root_with_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", nameof(Root)),
            base.Delete_on_root_with_subquery);

    public override Task Delete_on_leaf()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", nameof(Leaf1)),
            base.Delete_on_leaf);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPT("ExecuteDelete", nameof(Root)),
            base.Delete_GroupBy_Where_Select_First_3);

    public override Task Update_both_root_and_leaf_properties()
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("l => l.Leaf1Int", "l => l.RootInt"),
            base.Update_both_root_and_leaf_properties);
}
