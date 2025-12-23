// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Creates an instance of <see cref="RelationalSqlTranslatingExpressionVisitorFactory" />.
/// </summary>
/// <param name="dependencies">The service dependencies.</param>
public class RelationalSqlTranslatingExpressionVisitorFactory(RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
    : IRelationalSqlTranslatingExpressionVisitorFactory
{
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; } = dependencies;

    /// <inheritdoc />
    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        RelationalTranslationContext translationContext,
        RelationalQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new(
            Dependencies,
            queryCompilationContext,
            translationContext,
            queryableMethodTranslatingExpressionVisitor);
}
