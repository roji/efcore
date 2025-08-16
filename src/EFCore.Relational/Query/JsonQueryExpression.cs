// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression representing an entity or a collection of entities mapped to a JSON column and the path to access it.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <param name="json">A JSON value, typically a column.</param>
/// <param name="path">The list of path segments leading to the entity from the root of the JSON stored in the column.</param>
/// <param name="typeMapping">
///     The <see cref="RelationalTypeMapping" /> associated with the expression.
///     Represents the string (e.g. <c>nvarchar(max)</c> or JSON type (e.g. <c>jsonb</c>) returned by the JSON_QUERY() function.
/// </param>
public class JsonQueryExpression(SqlExpression json, IReadOnlyList<PathSegment> path, RelationalTypeMapping? typeMapping)
    : SqlExpression(typeof(string), typeMapping)
{
    // private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     The column containing the JSON value.
    /// </summary>
    public virtual SqlExpression Json { get; } = json;

    /// <summary>
    ///     The list of path segments leading to the entity from the root of the JSON stored in the column.
    /// </summary>
    public virtual IReadOnlyList<PathSegment> Path { get; } = path;

    // /// <summary>
    // ///     The value indicating whether the expression is nullable.
    // /// </summary>
    // public virtual bool IsNullable { get; }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("JSON_QUERY(");
        expressionPrinter.Visit(Json);
        expressionPrinter.Append(string.Join(".", Path.Select(e => e.ToString())));
        expressionPrinter.Append(")");
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        throw new NotImplementedException();
        // var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);

        // if (KeyPropertyMap is null)
        // {
        //     return Update(jsonColumn, keyPropertyMap: null);
        // }

        // var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
        // foreach (var (property, column) in KeyPropertyMap)
        // {
        //     newKeyPropertyMap[property] = (ColumnExpression)visitor.Visit(column);
        // }

        // return Update(jsonColumn, newKeyPropertyMap);
    }

    /// <inheritdoc />
    public override Expression Quote()
        => throw new NotImplementedException();

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is JsonQueryExpression jsonQueryExpression
                && Equals(jsonQueryExpression));

    private bool Equals(JsonQueryExpression jsonQueryExpression)
        => Json.Equals(jsonQueryExpression.Json) && Path.SequenceEqual(jsonQueryExpression.Path);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Json, Path);
}
