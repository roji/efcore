// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using TableReference = Microsoft.EntityFrameworkCore.Query.SqlExpressions.TableExpressionBase.TableReference;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a column in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[DebuggerDisplay("{TableAlias}.{Name}")]
public class ColumnExpression : SqlExpression
{
    // ColumnExpression never references the table directly - only through its reference object.
    // This allows us to replace tables efficiently, by updating their reference object to point to the new table.
    private readonly TableReferenceExpression _tableReference;

    /// <summary>
    ///     Creates a new instance of the <see cref="ColumnExpression" /> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="table">The table this column references.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <param name="nullable">Whether this column can contain nulls.</param>
    public ColumnExpression(string name, TableExpressionBase table, Type type, RelationalTypeMapping? typeMapping, bool nullable)
        : this(name, table.Reference, type, typeMapping, nullable)
    {
    }

    private ColumnExpression(string name, TableReferenceExpression tableReference, Type type, RelationalTypeMapping? typeMapping, bool nullable)
        : base(type, typeMapping)
    {
        Check.DebugAssert(
            tableReference.Table.Alias is not null,
            "A table with no alias was provided to ColumnExpression. If the table represents a join, provide the wrapped table instead.");

        Name = name;
        _tableReference = tableReference;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The table from which column is being referenced.
    /// </summary>
    public virtual TableExpressionBase Table
        => _tableReference.Table;

    /// <summary>
    ///     The alias of the table from which column is being referenced.
    /// </summary>
    public virtual string TableAlias
        => Table.Alias!;

    /// <summary>
    ///     The bool value indicating if this column can have null values.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this; // Note that we do not visit the referenced table; that gets visited via SelectExpression.Tables.

    /// <summary>
    ///     Makes this column nullable.
    /// </summary>
    /// <returns>A new expression which has <see cref="IsNullable" /> property set to true.</returns>
    public virtual ColumnExpression MakeNullable()
        => IsNullable ? this : new ColumnExpression(Name, _tableReference, Type, TypeMapping, nullable: true);

    /// <summary>
    ///     Applies supplied type mapping to this expression.
    /// </summary>
    /// <param name="typeMapping">A relational type mapping to apply.</param>
    /// <returns>A new expression which has supplied type mapping.</returns>
    public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new ColumnExpression(Name, _tableReference, Type, typeMapping, IsNullable);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(TableAlias).Append(".").Append(Name);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ColumnExpression concreteColumnExpression
                && Equals(concreteColumnExpression));

    private bool Equals(ColumnExpression concreteColumnExpression)
        => base.Equals(concreteColumnExpression)
            && Name == concreteColumnExpression.Name
            && Table.Equals(concreteColumnExpression.Table)
            && IsNullable == concreteColumnExpression.IsNullable;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Name, Table, IsNullable);
}
