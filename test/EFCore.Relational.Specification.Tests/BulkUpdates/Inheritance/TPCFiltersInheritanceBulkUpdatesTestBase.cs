// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class TPCFiltersInheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : FiltersInheritanceBulkUpdatesRelationalTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : TPCInheritanceBulkUpdatesFixture, new()
{
    public override Task Delete_on_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", nameof(Root)),
            base.Delete_on_root);

    public override Task Delete_on_root_with_subquery()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", nameof(Root)),
            base.Delete_on_root_with_subquery);

    public override Task Delete_GroupBy_Where_Select_First_3()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteDelete", nameof(Root)),
            base.Delete_GroupBy_Where_Select_First_3);

    public override Task Update_root()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", nameof(Root)),
            base.Update_root);

    public override Task Update_with_OfType_leaf()
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnTPC("ExecuteUpdate", nameof(Root)),
            base.Update_with_OfType_leaf);
}
