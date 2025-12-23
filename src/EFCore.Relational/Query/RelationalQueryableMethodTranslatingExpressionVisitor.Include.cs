// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    private static readonly List<MethodInfo> SupportedFilteredIncludeOperations =
    [
        QueryableMethods.Where,
        QueryableMethods.OrderBy,
        QueryableMethods.OrderByDescending,
        QueryableMethods.ThenBy,
        QueryableMethods.ThenByDescending,
        QueryableMethods.Skip,
        QueryableMethods.Take,
        QueryableMethods.AsQueryable
    ];

    // TODO: This implementation may make sense in core, since it doesn't actually do anything - just accumulates Includes into
    // the query compilation context. In fact, it may make sense even in preprocessing (though we have less information about the source
    // there).
    protected override ShapedQueryExpression? TranslateInclude(ShapedQueryExpression source, Expression navigationSelector)
        => TranslateIncludeCore(source, navigationSelector, thenInclude: false);

    protected override ShapedQueryExpression? TranslateThenInclude(ShapedQueryExpression source, Expression navigationSelector)
        => TranslateIncludeCore(source, navigationSelector, thenInclude: true);

    private ShapedQueryExpression? TranslateIncludeCore(ShapedQueryExpression source, Expression navigationSelector, bool thenInclude)
    {
        if (source.ShaperExpression is not RelationalStructuralTypeShaperExpression { StructuralType: IEntityType entityType } shaper)
        {
            throw new InvalidOperationException(CoreStrings.IncludeOnNonEntity(source.ShaperExpression.Print()));
        }

        var includeTree = shaper.IncludeTree;
        var setLoaded = true; // TODO

        switch (navigationSelector)
        {
            case Expression e when e.TryGetLambdaExpression(out var lambda):
            {
                var currentIncludeTreeNode = thenInclude
                    ? shaper.LastIncludeTreeNode! // LastIncludeTreeNode would be non-null for ThenInclude
                    : shaper.IncludeTree;

                var (result, filterExpression) = ExtractIncludeFilter(lambda.Body, lambda.Body);
                var lastIncludeTree = PopulateIncludeTree(currentIncludeTreeNode, result, setLoaded);
                // if (filterExpression != null)
                // {
                //     if (lastIncludeTree.FilterExpression != null
                //         && !ExpressionEqualityComparer.Instance.Equals(filterExpression, lastIncludeTree.FilterExpression))
                //     {
                //         throw new InvalidOperationException(
                //             CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                //                 FormatFilter(filterExpression.Body).Print(),
                //                 FormatFilter(lastIncludeTree.FilterExpression.Body).Print()));
                //     }

                //     lastIncludeTree.ApplyFilter(filterExpression);
                // }

                shaper.LastIncludeTreeNode = lastIncludeTree;
                return source;
            }

            // Include with string path
            case ConstantExpression { Value: string navigationChain }:
            {
                throw new NotImplementedException();

                // var navigationPaths = navigationChain.Split(["."], StringSplitOptions.None);
                // var includeTreeNodes = new Queue<IncludeTreeNode>();
                // includeTreeNodes.Enqueue(entityReference.IncludePaths);
                // foreach (var navigationName in navigationPaths)
                // {
                //     var nodesToProcess = includeTreeNodes.Count;
                //     while (nodesToProcess-- > 0)
                //     {
                //         var currentNode = includeTreeNodes.Dequeue();
                //         foreach (var navigation in FindNavigations(currentNode.EntityType, navigationName))
                //         {
                //             var addedNode = currentNode.AddNavigation(navigation, setLoaded);

                //             // This is to add eager Loaded navigations when owner type is included.
                //             PopulateEagerLoadedNavigations(addedNode);
                //             includeTreeNodes.Enqueue(addedNode);
                //         }
                //     }

                //     if (includeTreeNodes.Count == 0)
                //     {
                //         _queryCompilationContext.Logger.InvalidIncludePathError(navigationChain, navigationName);
                //     }
                // }
                // return source;
            }

            default:
                throw new UnreachableException();
        }

        static (Expression result, LambdaExpression? filterExpression) ExtractIncludeFilter(
            Expression currentExpression,
            Expression includeExpression)
        {
            switch (currentExpression)
            {
                case MemberExpression:
                    return (currentExpression, default);

                case MethodCallExpression methodCallExpression:
                {
                    if (methodCallExpression.Method.IsEFPropertyMethod())
                    {
                        return (currentExpression, default);
                    }

                    if (!methodCallExpression.Method.IsGenericMethod
                        || !SupportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
                    }

                    var (result, filterExpression) = ExtractIncludeFilter(methodCallExpression.Arguments[0], includeExpression);
                    if (filterExpression == null)
                    {
                        var prm = Expression.Parameter(result.Type);
                        filterExpression = Expression.Lambda(prm, prm);
                    }

                    var arguments = new List<Expression> { filterExpression.Body };
                    arguments.AddRange(methodCallExpression.Arguments.Skip(1));
                    filterExpression = Expression.Lambda(
                        methodCallExpression.Update(methodCallExpression.Object, arguments),
                        filterExpression.Parameters);

                    return (result, filterExpression);
                }

                default:
                    throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
            }
        }

        static Expression FormatFilter(Expression expression)
        {
            if (expression is MethodCallExpression { Method.IsGenericMethod: true } methodCallExpression
                && SupportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
            {
                if (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                {
                    return Expression.Parameter(expression.Type, "navigation");
                }

                var arguments = new List<Expression>();
                var source = FormatFilter(methodCallExpression.Arguments[0]);
                arguments.Add(source);
                arguments.AddRange(methodCallExpression.Arguments.Skip(1));

                return methodCallExpression.Update(methodCallExpression.Object, arguments);
            }

            return expression;
        }

        IncludeTreeNode PopulateIncludeTree(IncludeTreeNode includeTreeNode, Expression expression, bool setLoaded)
        {
            return expression switch
            {
                ParameterExpression => includeTreeNode,

                MethodCallExpression methodCallExpression
                    when methodCallExpression.TryGetEFPropertyArguments(out var entityExpression, out var propertyName)
                        && TryExtractIncludeTreeNode(entityExpression, propertyName, out var addedEfPropertyNode)
                    => addedEfPropertyNode,

                MemberExpression { Expression: not null } memberExpression
                    when TryExtractIncludeTreeNode(memberExpression.Expression, memberExpression.Member.Name, out var addedNode)
                    => addedNode,

                _ => throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(expression)),
            };

            bool TryExtractIncludeTreeNode(
                Expression innerExpression,
                string propertyName,
                [NotNullWhen(true)] out IncludeTreeNode? addedNode)
            {
                innerExpression = innerExpression.UnwrapTypeConversion(out var convertedType);
                var innerIncludeTreeNode = PopulateIncludeTree(includeTreeNode, innerExpression, setLoaded);
                var entityType = innerIncludeTreeNode.EntityType;

                if (convertedType is not null)
                {
                    entityType = entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                        .FirstOrDefault(et => et.ClrType == convertedType);
                    if (entityType is null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidTypeConversionWithInclude(expression, convertedType.ShortDisplayName()));
                    }
                }

                var navigation = (INavigationBase?)entityType.FindNavigation(propertyName)
                    ?? entityType.FindSkipNavigation(propertyName);

                if (navigation is not null)
                {
                    // TODO: INavigationBase
                    addedNode = innerIncludeTreeNode.AddNavigation((INavigation)navigation, setLoaded);

                    // This is to add eager Loaded navigations when owner type is included.
                    // TODO
                    // PopulateEagerLoadedNavigations(addedNode);

                    return true;
                }

                addedNode = null;
                return false;
            }
        }
    }

    private class IncludeProcessor(RelationalQueryableMethodTranslatingExpressionVisitor queryableMethodTranslator) : ExpressionVisitor
    {
        private SelectExpression? _currentSelect;
        private bool _includesApplied;

        internal Expression ProcessIncludes(ShapedQueryExpression shapedQuery)
        {
            _includesApplied = false;
            _currentSelect = (SelectExpression)shapedQuery.QueryExpression;
            var visitedShaper = Visit(shapedQuery.ShaperExpression);
            _currentSelect = null;

            // We should be able to simply check whether the shaper/shaped query were changed in the visitation; but since SelectExpression
            // is mutable and we may have only mutated the select's projections, this wouldn't actually detect such cases. So we manually
            // track whether any includes were applied.
            // return shapedQuery;
            return _includesApplied
                ? queryableMethodTranslator.TranslateSelect(
                    shapedQuery,
                    Expression.Lambda(visitedShaper, Expression.Parameter(shapedQuery.ShaperExpression.Type, "_")))
                : shapedQuery;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ShapedQueryExpression shapedQuery:
                    return shapedQuery.UpdateShaperExpression(Visit(shapedQuery.ShaperExpression));

                case RelationalStructuralTypeShaperExpression { StructuralType: IEntityType } shaper:
                    return ApplyIncludes(shaper, shaper.IncludeTree);

                case ProjectionBindingExpression { QueryExpression: SelectExpression select } binding:
                    var visitedProjection = Visit(select.GetProjection(binding));
                    select.ReplaceProjection(binding, visitedProjection);
                    return binding;

                default:
                    return base.VisitExtension(node);
            }
        }

        private Expression ApplyIncludes(RelationalStructuralTypeShaperExpression outerShaper, IncludeTreeNode outerIncludeNode)
        {
            Check.DebugAssert(_currentSelect is not null);

            Expression outerShaperWithIncludes = outerShaper;

            // Process (non-JSON) reference navigations before (non-JSON) collection navigations, since JOINs for the former get added
            // here, while JOINs for the latter get added later, when applying the final projection. Ordering like this keeps the generated
            // table aliases more sane.
            var navigations = outerIncludeNode.OrderBy(kv => kv.Key.IsCollection && !kv.Key.TargetEntityType.IsMappedToJson());

            foreach (var (navigation, innerIncludeNode) in navigations)
            {
                _includesApplied = true;

                switch (navigation)
                {
                    case { } when navigation.TargetEntityType.IsMappedToJson():
                    {
                        var structuralTypeProjection = outerShaper.ValueBufferExpression switch
                        {
                            StructuralTypeProjectionExpression p => p,

                            ProjectionBindingExpression pbe
                                when ((SelectExpression)pbe.QueryExpression).GetProjection(pbe) is StructuralTypeProjectionExpression p => p,

                            _ => throw new InvalidOperationException("Unexpected ValueBufferExpression type for JSON navigation include.")
                        };

                        var innerJsonShaper = structuralTypeProjection.BindNavigation(navigation)!;

                        outerShaperWithIncludes = new IncludeExpression(outerShaperWithIncludes, innerJsonShaper, navigation);
                        continue;
                    }

                    case { IsCollection: false }:
                    {
                        if (navigation.ForeignKey.IsOwnership
                            // TODO: Better way to identify table sharing?
                            && navigation.TargetEntityType.GetViewOrTableMappings().Single().Table is { } targetTable
                            && navigation.DeclaringEntityType.GetViewOrTableMappings().Select(m => m.Table).Contains(targetTable))
                        {
                            throw new NotImplementedException("Owned table splitting not implemented yet");
                        }

                        var innerSelect = queryableMethodTranslator.CreateSelect(navigation.TargetEntityType);

                        // var innerNullable = outerShaper.IsNullable
                        //     || (navigation.IsOnDependent ? !navigation.ForeignKey.IsRequired : !navigation.ForeignKey.IsRequiredDependent);

                        var innerNullable = outerShaper.IsNullable
                            || !navigation.IsOnDependent
                            || !navigation.ForeignKey.IsRequired;

                        if (innerNullable)
                        {
                            // Make the inner select's projection nullable
                            var emptyProjection = new ProjectionBindingExpression(innerSelect, new ProjectionMember(), typeof(ValueBuffer));
                            var nullableProjection = SelectExpression.MakeNullable(innerSelect.GetProjection(emptyProjection), nullable: true);
                            innerSelect.ReplaceProjection(emptyProjection, nullableProjection);
                        }

                        // TODO: Careful, can an existing bound inner shaper have a different nullability?
                        var existingBoundInnerShaper = outerShaper.BoundNavigations.GetValueOrDefault(navigation)
                            as RelationalStructuralTypeShaperExpression;

                        var innerShaper = existingBoundInnerShaper
                            ?? new RelationalStructuralTypeShaperExpression(
                                navigation.TargetEntityType,
                                new ProjectionBindingExpression(
                                    innerSelect,
                                    new ProjectionMember(),
                                    typeof(ValueBuffer)),
                                innerNullable);

                        if (existingBoundInnerShaper is null)
                        {
                            // TODO: Test reference join of a TPC type!

                            // Add a JOIN for the navigation, unless this is an owned table sharing navigation.
                            // if (_processedShapers.Add(outerShaper))
                            // {
                            queryableMethodTranslator.AddJoin(_currentSelect, navigation, outerShaper, innerShaper);
                            // }

                            // Register the inner shaper in the outer shaper's bound navigations map.
                            // This both helps us avoid adding a duplicate JOIN in case the same shaper is projected multiple times
                            // (we'll find innerShaper as existingBoundInnerShaper next time), and also in theory allows binding
                            // to properties later in post-processing, if we ever need that.
                            outerShaper.BoundNavigations.Add(navigation, innerShaper);
                        }

                        // TODO: Maybe breadth-first? Check our current behavior.
                        var innerShaperWithIncludes = ApplyIncludes(innerShaper, innerIncludeNode);
                        outerShaperWithIncludes = new IncludeExpression(outerShaperWithIncludes, innerShaperWithIncludes, navigation);
                        continue;
                    }

                    case { IsCollection: true }:
                    {
                        // Collection navigations aren't re-bound like reference navigations:
                        // With reference navigations, you can have: Where(x => x.Details.Foo == 8 && x.Details.Bar == 9)
                        // (x.Details is a reference navigation that's bound twice, and we don't want to add two JOINs).
                        // With collection navigations, a subquery is (generally) composed on top: Where(x => x.Posts.Count() == 3 && x.Posts.Any(...))
                        // Each subquery is its own separate subquery, so the navigation itself (x.Posts) isn't re-bound.
                        // However, there are some edge cases that require us to cache bound navigations. For example, the same Select() selector may get
                        // evaluated twice by RelationalProjectionBindingExpressionVisitor - once in regular mode, and then in client eval mode.

                        // TODO: Re the above, JSON collections can be rebound normally (not necessarily subquery), but since there's no JOIN involved, not
                        // clear we even need to cache these rather than re-bind from scratch each time.

                        if (outerShaper.BoundNavigations.TryGetValue(navigation, out var boundExpression))
                        {
                            return boundExpression;
                        }

                        if (navigation.TargetEntityType.IsMappedToJson())
                        {
                            throw new NotImplementedException("Include of JSON collection navigation not implemented yet.");
                        }

                        // TODO: Do we need to check and add in the bound navigations map like for reference navigations? If so, don't forget to clone

                        var innerShapedQuery = queryableMethodTranslator.CreateShapedQueryExpression2(navigation.TargetEntityType);

                        var innerSelect = (SelectExpression)innerShapedQuery.QueryExpression;
                        var joinPredicate = queryableMethodTranslator.CreateJoinPredicate(navigation, outerShaper, innerShapedQuery.ShaperExpression);
                        innerSelect.ApplyPredicate(joinPredicate);

                        // {
                        //     var clonedSelectExpression = ((SelectExpression)innerShapedQuery.QueryExpression).Clone();

                        //     // Since SelectExpression is mutable, clone it first so that subsequent processing doesn't modify what we cache as a bound navigation
                        //     // TODO: Not sure this is necessary for collection navigations
                        //     var clonedInnerShapedQuery = new ShapedQueryExpression(
                        //         clonedSelectExpression,
                        //         new QueryExpressionReplacingExpressionVisitor(innerShapedQuery.QueryExpression, clonedSelectExpression)
                        //             .Visit(innerShapedQuery.ShaperExpression));

                        //     outerShaper.BoundNavigations.Add(navigation, clonedInnerShapedQuery);
                        // }

                        var innerShaper = (RelationalStructuralTypeShaperExpression)innerShapedQuery.ShaperExpression;

                        // Continue applying Includes recursively. Set _currentShapedQuery so that any reference includes get applied inside the subquery
                        // and not on the top-level select.
                        var parentSelect = _currentSelect;
                        _currentSelect = innerSelect;
                        _includesApplied = false;
                        var innerShaperWithIncludes = ApplyIncludes(innerShaper, innerIncludeNode);

                        innerShapedQuery = queryableMethodTranslator.TranslateSelect(
                            innerShapedQuery,
                            Expression.Lambda(innerShaperWithIncludes, Expression.Parameter(innerShapedQuery.ShaperExpression.Type, "_")));

                        _includesApplied = true;
                        _currentSelect = parentSelect;

                        outerShaperWithIncludes = new IncludeExpression(
                            outerShaperWithIncludes,
                            new CollectionResultExpression(
                                innerShapedQuery, navigation, elementType: navigation.TargetEntityType.ClrType),
                                navigation);
                        // outerShaperWithIncludes = new IncludeExpression(outerShaperWithIncludes, new MaterializeCollectionNavigationExpression(innerShapedQuery, navigation), navigation);
                        continue;
                    }

                    default:
                        throw new UnreachableException();
                }
            }

            return outerShaperWithIncludes;
        }
    }
}
