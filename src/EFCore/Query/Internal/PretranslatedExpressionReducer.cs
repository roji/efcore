// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// /// <summary>
// ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
// ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
// ///     any release. You should only use it directly in your code with extreme caution and knowing that
// ///     doing so can result in application failures when updating to a new Entity Framework Core release.
// /// </summary>
// [EntityFrameworkInternal]
// public class PretranslatedExpressionReducer : ExpressionVisitor
// {
//     private IReadOnlyDictionary<ParameterExpression, Expression> _parameterMap = null!;
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     [return: NotNullIfNotNull(nameof(expression))]
//     public Expression? Reduce(Expression? expression, IReadOnlyDictionary<ParameterExpression, Expression> parameterMap)
//     {
//         _parameterMap = parameterMap;
//         var result = Visit(expression);
//         _parameterMap = null!;
//         return result;
//     }
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     protected override Expression VisitMember(MemberExpression memberExpression)
//         => Visit(memberExpression.Expression) switch
//         {
//             GroupByShaperExpression groupByShaperExpression when memberExpression.Member.Name == nameof(IGrouping<int, int>.Key)
//                 => groupByShaperExpression.KeySelector,
//
//             NewExpression newExpression when newExpression.Members?.IndexOf(memberExpression.Member) is >= 0 and var index
//                 => newExpression.Arguments[index],
//
//             var e when e.UnwrapTypeConversion(out _) is MemberInitExpression memberInitExpression
//                 && memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.IsSameAs(memberExpression.Member)) is MemberAssignment
//                     memberAssignment
//                 => memberAssignment.Expression,
//
//             _ => memberExpression
//         };
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     protected override Expression VisitParameter(ParameterExpression parameterExpression)
//         => _parameterMap.TryGetValue(parameterExpression, out var mappedExpression)
//             ? Visit(mappedExpression)
//             : parameterExpression;
//
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     protected override Expression VisitExtension(Expression node)
//         // This visitor runs over pre-translated nodes, so generally doesn't encounter extension nodes. However, we generally mix
//         // pre-translated and translated nodes; for example, we map parameters to RelationalGroupByShaperExpression (translated
//         // node), which is then observed under pre-translated ones. Simply ignore these, returning them immediately (some of these nodes
//         // throw from VisitChildren, so we must handle them explicitly here).
//         => node;
// }
