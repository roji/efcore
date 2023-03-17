// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NonSharedPrimitiveCollectionsQueryRelationalTestBase : NonSharedPrimitiveCollectionsQueryTestBase
{
    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public override Task Array_of_byte()
        => AssertTranslationFailed(() => TestArray((byte)1, (byte)2));

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
