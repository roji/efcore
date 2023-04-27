// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlExpressionSimplifyingExpressionVisitor : ExpressionVisitor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _useRelationalNulls;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpressionSimplifyingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory, bool useRelationalNulls)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _useRelationalNulls = useRelationalNulls;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
        {
            return shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));
        }

        // Only applies to 'CASE WHEN condition...' not 'CASE operand WHEN...'
        if (extensionExpression is CaseExpression
            {
                Operand: null, ElseResult: CaseExpression { Operand: null } nestedCaseExpression
            } caseExpression)
        {
            return VisitExtension(
                _sqlExpressionFactory.Case(
                    caseExpression.WhenClauses.Union(nestedCaseExpression.WhenClauses).ToList(),
                    nestedCaseExpression.ElseResult));
        }

        if (extensionExpression is SqlBinaryExpression sqlBinaryExpression)
        {
            return SimplifySqlBinary(sqlBinaryExpression);
        }

        if (extensionExpression is SqlFunctionExpression sqlFunctionExpression
            && IsCoalesce(sqlFunctionExpression))
        {
            var arguments = new List<SqlExpression>();
            foreach (var argument in sqlFunctionExpression.Arguments!)
            {
                var newArgument = (SqlExpression)Visit(argument);
                if (IsCoalesce(newArgument))
                {
                    arguments.AddRange(((SqlFunctionExpression)newArgument).Arguments!);
                }
                else
                {
                    arguments.Add(newArgument);
                }
            }

            var distinctArguments = arguments.Distinct().ToList();

            return distinctArguments.Count > 1
                ? new SqlFunctionExpression(
                    sqlFunctionExpression.Name,
                    distinctArguments,
                    sqlFunctionExpression.IsNullable,
                    argumentsPropagateNullability: distinctArguments.Select(_ => false).ToArray(),
                    sqlFunctionExpression.Type,
                    sqlFunctionExpression.TypeMapping)
                : distinctArguments[0];
        }

        return base.VisitExtension(extensionExpression);

        static bool IsCoalesce(SqlExpression sqlExpression)
            => sqlExpression is SqlFunctionExpression { IsBuiltIn: true, Instance: null } sqlFunctionExpression
                && string.Equals(sqlFunctionExpression.Name, "COALESCE", StringComparison.OrdinalIgnoreCase)
                && sqlFunctionExpression.Arguments?.Count > 1;
    }

    private static bool IsCompareTo([NotNullWhen(true)] CaseExpression? caseExpression)
    {
        if (caseExpression is { Operand: null, ElseResult: null, WhenClauses.Count: 3 }
            && caseExpression.WhenClauses.All(c => c is { Test: SqlBinaryExpression, Result: SqlConstantExpression { Value: int } }))
        {
            var whenClauses = caseExpression.WhenClauses.Select(
                c => new { Test = (SqlBinaryExpression)c.Test, ResultValue = (int)((SqlConstantExpression)c.Result).Value! }).ToList();

            if (whenClauses[0].Test.Left.Equals(whenClauses[1].Test.Left)
                && whenClauses[1].Test.Left.Equals(whenClauses[2].Test.Left)
                && whenClauses[0].Test.Right.Equals(whenClauses[1].Test.Right)
                && whenClauses[1].Test.Right.Equals(whenClauses[2].Test.Right)
                && whenClauses[0].Test.OperatorType == ExpressionType.Equal
                && whenClauses[1].Test.OperatorType == ExpressionType.GreaterThan
                && whenClauses[2].Test.OperatorType == ExpressionType.LessThan
                && whenClauses[0].ResultValue == 0
                && whenClauses[1].ResultValue == 1
                && whenClauses[2].ResultValue == -1)
            {
                return true;
            }
        }

        return false;
    }

    private SqlExpression OptimizeCompareTo(
        SqlBinaryExpression sqlBinaryExpression,
        int intValue,
        CaseExpression caseExpression)
    {
        var testLeft = ((SqlBinaryExpression)caseExpression.WhenClauses[0].Test).Left;
        var testRight = ((SqlBinaryExpression)caseExpression.WhenClauses[0].Test).Right;
        var operatorType = sqlBinaryExpression.Right is SqlConstantExpression
            ? sqlBinaryExpression.OperatorType
            : sqlBinaryExpression.OperatorType switch
            {
                ExpressionType.GreaterThan => ExpressionType.LessThan,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
                ExpressionType.LessThan => ExpressionType.GreaterThan,
                ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                _ => sqlBinaryExpression.OperatorType
            };

        return operatorType switch
        {
            // CompareTo(a, b) != 0 -> a != b
            // CompareTo(a, b) != 1 -> a <= b
            // CompareTo(a, b) != -1 -> a >= b
            ExpressionType.NotEqual => (SqlExpression)Visit(
                intValue switch
                {
                    0 => _sqlExpressionFactory.NotEqual(testLeft, testRight),
                    1 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                    _ => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight)
                }),
            // CompareTo(a, b) > 0 -> a > b
            // CompareTo(a, b) > 1 -> false
            // CompareTo(a, b) > -1 -> a >= b
            ExpressionType.GreaterThan => (SqlExpression)Visit(
                intValue switch
                {
                    0 => _sqlExpressionFactory.GreaterThan(testLeft, testRight),
                    1 => _sqlExpressionFactory.Constant(false, sqlBinaryExpression.TypeMapping),
                    _ => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight)
                }),
            // CompareTo(a, b) >= 0 -> a >= b
            // CompareTo(a, b) >= 1 -> a > b
            // CompareTo(a, b) >= -1 -> true
            ExpressionType.GreaterThanOrEqual => (SqlExpression)Visit(
                intValue switch
                {
                    0 => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight),
                    1 => _sqlExpressionFactory.GreaterThan(testLeft, testRight),
                    _ => _sqlExpressionFactory.Constant(true, sqlBinaryExpression.TypeMapping)
                }),
            // CompareTo(a, b) < 0 -> a < b
            // CompareTo(a, b) < 1 -> a <= b
            // CompareTo(a, b) < -1 -> false
            ExpressionType.LessThan => (SqlExpression)Visit(
                intValue switch
                {
                    0 => _sqlExpressionFactory.LessThan(testLeft, testRight),
                    1 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                    _ => _sqlExpressionFactory.Constant(false, sqlBinaryExpression.TypeMapping)
                }),

            _ => (SqlExpression)Visit(
                intValue switch
                {
                    0 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                    1 => _sqlExpressionFactory.Constant(true, sqlBinaryExpression.TypeMapping),
                    _ => _sqlExpressionFactory.LessThan(testLeft, testRight)
                })
        };
    }

    private Expression SimplifySqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        var sqlConstantComponent =
            sqlBinaryExpression.Left as SqlConstantExpression ?? sqlBinaryExpression.Right as SqlConstantExpression;
        var caseComponent = sqlBinaryExpression.Left as CaseExpression ?? sqlBinaryExpression.Right as CaseExpression;

        // generic CASE statement comparison optimization:
        // (CASE
        //  WHEN condition1 THEN result1
        //  WHEN condition2 THEN result2
        //  WHEN ...
        //  WHEN conditionN THEN resultN) == result1 -> condition1
        if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
            && sqlConstantComponent?.Value is not null
            && caseComponent is { Operand: null, ElseResult: null })
        {
            var matchingCaseBlock = caseComponent.WhenClauses.FirstOrDefault(wc => sqlConstantComponent.Equals(wc.Result));
            if (matchingCaseBlock != null)
            {
                return Visit(matchingCaseBlock.Test);
            }
        }

        // CompareTo specific optimizations
        if (sqlConstantComponent != null
            && IsCompareTo(caseComponent)
            && sqlConstantComponent.Value is int intValue and > -2 and < 2
            && sqlBinaryExpression.OperatorType
                is ExpressionType.NotEqual
                or ExpressionType.GreaterThan
                or ExpressionType.GreaterThanOrEqual
                or ExpressionType.LessThan
                or ExpressionType.LessThanOrEqual)
        {
            return OptimizeCompareTo(
                sqlBinaryExpression,
                intValue,
                caseComponent);
        }

        var left = (SqlExpression)Visit(sqlBinaryExpression.Left);
        var right = (SqlExpression)Visit(sqlBinaryExpression.Right);

        if (sqlBinaryExpression.OperatorType is ExpressionType.AndAlso or ExpressionType.OrElse)
        {
            if (TryGetInExpressionCandidateInfo(left, out var leftCandidateInfo)
                && TryGetInExpressionCandidateInfo(right, out var rightCandidateInfo)
                && leftCandidateInfo.Item == rightCandidateInfo.Item
                && leftCandidateInfo.OperationType == rightCandidateInfo.OperationType)
            {
                var leftConstantIsEnumerable = leftCandidateInfo.Values is not null;
                var rightConstantIsEnumerable = rightCandidateInfo.Values is not null;

                if ((leftCandidateInfo.OperationType, sqlBinaryExpression.OperatorType) is
                    (ExpressionType.Equal, ExpressionType.OrElse) or (ExpressionType.NotEqual, ExpressionType.AndAlso))
                {
                    IReadOnlyList<SqlExpression> resultArray;

                    switch ((leftConstantIsEnumerable, rightConstantIsEnumerable))
                    {
                        case (false, false):
                        {
                            // comparison + comparison
                            var leftConstantExpression = leftCandidateInfo.Value!;
                            var rightConstantExpression = rightCandidateInfo.Value!;

                            // for relational nulls we can't combine comparisons that contain null
                            // a != 1 && a != null would be converted to a NOT IN (1, null), which never returns any results
                            // we need to keep it in the original form so that a != null gets converted to a IS NOT NULL instead
                            // for c# null semantics it's fine because null semantics visitor extracts null back into proper null checks
                            if (_useRelationalNulls && (leftConstantExpression.Value is null || rightConstantExpression.Value is null))
                            {
                                return sqlBinaryExpression.Update(left, right);
                            }

                            resultArray = new[] { leftConstantExpression, rightConstantExpression };
                            break;
                        }

                        case (true, true):
                        {
                            // in + in
                            resultArray = leftCandidateInfo.Values!.Union(rightCandidateInfo.Values!).ToArray();
                            break;
                        }

                        default:
                        {
                            // in + comparison
                            var values = leftConstantIsEnumerable
                                ? leftCandidateInfo.Values!
                                : rightCandidateInfo.Values!;

                            var constant = leftConstantIsEnumerable
                                ? rightCandidateInfo.Value!
                                : leftCandidateInfo.Value!;

                            if (_useRelationalNulls && constant.Value is null)
                            {
                                return sqlBinaryExpression.Update(left, right);
                            }

                            if (values.Contains(constant))
                            {
                                resultArray = values;
                            }
                            else
                            {
                                var newValues = values.ToList();
                                newValues.Add(constant);
                                resultArray = newValues;
                            }

                            break;
                        }
                    }

                    return _sqlExpressionFactory.In(
                        leftCandidateInfo.Item, resultArray, leftCandidateInfo.OperationType == ExpressionType.NotEqual);
                }

                if (leftConstantIsEnumerable && rightConstantIsEnumerable)
                {
                    // a IN (1, 2, 3) && a IN (2, 3, 4) -> a IN (2, 3)
                    // a NOT IN (1, 2, 3) || a NOT IN (2, 3, 4) -> a NOT IN (2, 3)
                    var resultArray = leftCandidateInfo.Values!.Intersect(rightCandidateInfo.Values!).ToArray();

                    return _sqlExpressionFactory.In(
                        leftCandidateInfo.Item, resultArray, leftCandidateInfo.OperationType == ExpressionType.NotEqual);
                }
            }
        }

        return sqlBinaryExpression.Update(left, right);
    }

    private static bool TryGetInExpressionCandidateInfo(
        SqlExpression sqlExpression,
        out (ColumnExpression Item, SqlConstantExpression? Value, IReadOnlyList<SqlExpression>? Values, RelationalTypeMapping TypeMapping, ExpressionType OperationType)
            candidateInfo)
    {
        switch (sqlExpression)
        {
            case SqlUnaryExpression { OperatorType: ExpressionType.Not } sqlUnaryExpression
                when TryGetInExpressionCandidateInfo(sqlUnaryExpression.Operand, out var inner):
                candidateInfo = (inner.Item, inner.Value, inner.Values, inner.TypeMapping,
                    inner.OperationType == ExpressionType.Equal ? ExpressionType.NotEqual : ExpressionType.Equal);

                return true;

            case SqlBinaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } sqlBinaryExpression:
            {
                // TODO: Why restrict to only constant, can also do parameter, another column...
                var column = (sqlBinaryExpression.Left as ColumnExpression ?? sqlBinaryExpression.Right as ColumnExpression);
                var constant = (sqlBinaryExpression.Left as SqlConstantExpression ?? sqlBinaryExpression.Right as SqlConstantExpression);

                if (column != null && constant != null)
                {
                    candidateInfo = (column, Value: constant, Values: null, constant.TypeMapping!, sqlBinaryExpression.OperatorType);
                    return true;
                }

                break;
            }

            case InExpression { Item: ColumnExpression column, Values: IReadOnlyList<SqlExpression> values } inExpression:
                candidateInfo =
                    (column, Value: null, values, values[0].TypeMapping!, inExpression.IsNegated ? ExpressionType.NotEqual : ExpressionType.Equal);

                return true;
        }

        candidateInfo = default;
        return false;
    }
}
