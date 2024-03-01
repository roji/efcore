// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

using static System.Linq.Expressions.Expression;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalExpressionTreeFuncletizer(
    ExpressionTreeFuncletizerDependencies dependencies,
    Type contextType,
    bool generateContextAccessors)
    : ExpressionTreeFuncletizer(dependencies, contextType, generateContextAccessors)
{
    protected override Expression VisitExtension(Expression extension)
        => extension switch
        {
            FromSqlQueryRootExpression sqlQueryRoot => VisitFromSqlQueryRootExpression(sqlQueryRoot),

            _ => base.VisitExtension(extension)
        };

    protected virtual FromSqlQueryRootExpression VisitFromSqlQueryRootExpression(FromSqlQueryRootExpression fromSqlQueryRoot)
    {
        var visitedArgument = Visit(fromSqlQueryRoot.Argument, out var state);

        if (state.IsEvaluatable)
        {
            // Note that FromSqlQueryRootExpression's Arguments need to be a parameter rather than constant, so we always pass
            // containsCapturedVariable: true regardless of the visitation result above.
            state = state with { StateType = StateType.EvaluatableWithCapturedVariable};

            visitedArgument = ProcessEvaluatableRoot(visitedArgument, ref state);
        }

        State = state.ContainsEvaluatable && CalculatingPath
            ? EvaluatabilityState.CreateContainsEvaluatable(
                typeof(FromSqlQueryRootExpression),
                [State.Path! with { PathFromParent = static e => Property(e, nameof(FromSqlQueryRootExpression.Argument)) }])
            : EvaluatabilityState.NoEvaluatability;

        // TODO: Do the stuff that's done in the base class for query roots
        // TODO: Clean up the _evaluateRoot/_inLambda crap from the base class
        return fromSqlQueryRoot.Update(visitedArgument);
    }
}
