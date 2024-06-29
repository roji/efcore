// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[DebuggerDisplay("{PrintShortSql(), nq}")]
public sealed class SelectExpression : Expression, IPrintableExpression
{
    private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
    private readonly List<SourceExpression> _sources = [];
    private readonly List<ProjectionExpression> _projection = [];
    private readonly List<OrderingExpression> _orderings = [];

    private readonly CosmosAliasManager _aliasManager;

    private readonly List<(Expression ValueExpression, IProperty Property)> _partitionKeyValues = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ReadItemInfo? ReadItemInfo { get; init; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(
        List<SourceExpression> sources,
        SqlExpression? predicate,
        List<ProjectionExpression> projections,
        bool distinct,
        List<OrderingExpression> orderings,
        SqlExpression? offset,
        SqlExpression? limit,
        CosmosAliasManager aliasManager)
    {
        _sources = sources;
        Predicate = predicate;
        _projection = projections;
        IsDistinct = distinct;
        _orderings = orderings;
        Offset = offset;
        Limit = limit;
        _aliasManager = aliasManager;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(Expression projection, CosmosAliasManager aliasManager)
    {
        _projectionMapping[new ProjectionMember()] = projection;
        _aliasManager = aliasManager;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression(
        SourceExpression source,
        Expression projection,
        CosmosAliasManager aliasManager,
        ReadItemInfo? readItemInfo = null)
    {
        _sources.Add(source);
        _projectionMapping[new ProjectionMember()] = projection;
        _aliasManager = aliasManager;
        ReadItemInfo = readItemInfo;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SelectExpression CreateForCollection(
        Expression sourceExpression,
        string sourceAlias,
        Expression projection,
        CosmosAliasManager aliasManager)
    {
        // SelectExpressions representing bare arrays are of the form SELECT VALUE i FROM i IN x.
        // Unfortunately, Cosmos doesn't support x being anything but a root container or a property access
        // (e.g. SELECT VALUE i FROM i IN c.SomeArray).
        // For example, x cannot be a function invocation (SELECT VALUE i FROM i IN SetUnion(...)) or an array constant
        // (SELECT VALUE i FROM i IN [1,2,3]).
        // So we wrap any non-property in a subquery as follows: SELECT i FROM i IN (SELECT VALUE [1,2,3])
        if (!SourceExpression.IsCompatible(sourceExpression))
        {
            sourceExpression = new SelectExpression(
                sources: [],
                predicate: null,
                projections: [new ProjectionExpression(sourceExpression, null!)],
                distinct: false,
                orderings: [],
                offset: null,
                limit: null,
                aliasManager)
            {
                UsesSingleValueProjection = true
            };
        }

        var source = new SourceExpression(sourceExpression, sourceAlias, withIn: true);

        return new SelectExpression(source, projection, aliasManager)
        {
            UsesSingleValueProjection = true
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<ProjectionExpression> Projection
        => _projection;

    /// <summary>
    ///     If set, indicates that the <see cref="SelectExpression" /> has a Cosmos VALUE projection, which does not get wrapped in a
    ///     JSON object. If <see langword="true" />, <see cref="Projection" /> must contain a single item.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public bool UsesSingleValueProjection { get; init; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<SourceExpression> Sources
        => _sources;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<OrderingExpression> Orderings
        => _orderings;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Predicate { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Limit { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Offset { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsDistinct { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Expression GetMappedProjection(ProjectionMember projectionMember)
        => _projectionMapping[projectionMember];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ClearProjection()
        => _projectionMapping.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddPartitionKey(IProperty partitionKeyProperty, Expression expression)
        => _partitionKeyValues.Add((expression, partitionKeyProperty));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PartitionKey GetPartitionKeyValue(IReadOnlyDictionary<string, object> parameterValues)
    {
        if (!_partitionKeyValues.Any())
        {
            return PartitionKey.None;
        }

        var builder = new PartitionKeyBuilder();
        foreach (var tuple in _partitionKeyValues)
        {
            var rawKeyValue = tuple.ValueExpression switch
            {
                ConstantExpression constantExpression
                    => constantExpression.Value,
                ParameterExpression parameterExpression when parameterValues.TryGetValue(parameterExpression.Name!, out var value)
                    => value,
                _ => null
            };
            builder.Add(rawKeyValue, tuple.Property);
        }

        return builder.Build();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyProjection()
    {
        if (Projection.Any())
        {
            return;
        }

        var result = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            result[projectionMember] = Constant(
                AddToProjection(
                    expression,
                    projectionMember.Last?.Name));
        }

        _projectionMapping = result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
    {
        _projectionMapping.Clear();
        foreach (var (projectionMember, expression) in projectionMapping)
        {
            _projectionMapping[projectionMember] = expression;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int AddToProjection(Expression sqlExpression)
        => AddToProjection(sqlExpression, null);

    private int AddToProjection(Expression expression, string? alias)
    {
        var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(expression));
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        var baseAlias = alias
            ?? (expression as IAccessExpression)?.PropertyName
            ?? "c";

        var currentAlias = baseAlias;
        var counter = 0;
        while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
        {
            currentAlias = $"{baseAlias}{counter++}";
        }

        _projection.Add(new ProjectionExpression(expression, currentAlias));

        return _projection.Count - 1;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyDistinct()
        => IsDistinct = true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ClearOrdering()
        => _orderings.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyPredicate(SqlExpression expression)
    {
        if (expression is SqlConstantExpression { Value: true })
        {
            return;
        }

        Predicate = Predicate == null
            ? expression
            : new SqlBinaryExpression(
                ExpressionType.AndAlso,
                Predicate,
                expression,
                typeof(bool),
                expression.TypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyLimit(SqlExpression sqlExpression)
        => Limit = sqlExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyOffset(SqlExpression sqlExpression)
        => Offset = sqlExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ApplyOrdering(OrderingExpression orderingExpression)
    {
        _orderings.Clear();
        _orderings.Add(orderingExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AppendOrdering(OrderingExpression orderingExpression)
    {
        if (_orderings.FirstOrDefault(o => o.Expression.Equals(orderingExpression.Expression)) == null)
        {
            _orderings.Add(orderingExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void ReverseOrderings()
    {
        // TODO: #34122
        if (Limit is not null || Offset is not null)
        {
            throw new InvalidOperationException(CosmosStrings.ReverseAfterSkipTakeNotSupported);
        }

        var existingOrderings = _orderings.ToArray();

        _orderings.Clear();

        foreach (var existingOrdering in existingOrderings)
        {
            _orderings.Add(
                new OrderingExpression(
                    existingOrdering.Expression,
                    !existingOrdering.IsAscending));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Expression AddJoin(ShapedQueryExpression inner, Expression outerShaper, CosmosAliasManager aliasManager)
    {
        var (innerSelect, innerShaper) = ((SelectExpression)inner.QueryExpression, inner.ShaperExpression);

        // Create a new source (JOIN) for the server side of the query; if the inner query represents a bare array, unwrap it and
        // add the JOIN directly
        SourceExpression? joinSource = null;
        string sourceAlias;
        if (inner.TryExtractArray(out var bareArray) && SourceExpression.IsCompatible(bareArray))
        {
            sourceAlias = aliasManager.GenerateSourceAlias(bareArray);

            if (SourceExpression.IsCompatible(bareArray))
            {
                joinSource = new SourceExpression(bareArray, sourceAlias, withIn: true);
            }
        }
        else
        {
            sourceAlias = aliasManager.GenerateSourceAlias("join");
        }

        joinSource ??= new SourceExpression(innerSelect, sourceAlias);

        // Make the necessary modifications to the shaper side, projecting out a TransparentIdentifier (outer/inner)
        var transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaper.Type, innerShaper.Type);
        var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer")!;
        var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner")!;

        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        var mapping = new Dictionary<ProjectionMember, ProjectionMember>();

        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            var remappedProjectionMember = projectionMember.Prepend(outerMemberInfo);
            mapping[projectionMember] = remappedProjectionMember;
            projectionMapping[remappedProjectionMember] = expression;
        }

        outerShaper = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(outerShaper);
        mapping.Clear();

        foreach (var (projectionMember, expression) in innerSelect._projectionMapping)
        {
            var remappedProjectionMember = projectionMember.Prepend(innerMemberInfo);
            mapping[projectionMember] = remappedProjectionMember;

            Expression projectionToAdd;
            if (projectionMember.Last is null)
            {
                projectionToAdd = expression switch
                {
                    SqlExpression e => new ScalarReferenceExpression(joinSource.Alias, e.Type, e.TypeMapping),
                    EntityProjectionExpression e => e.Update(new ObjectReferenceExpression(e.EntityType, joinSource.Alias)),

                    _ => throw new UnreachableException(
                        $"Unexpected expression type in projection when adding join: {expression.GetType().Name}")
                };
            }
            else
            {
                // TODO: #34004
                // The subquery is projecting out a JSON object; for the projection mapping of the outer query, we need to generate
                // property accesses over that object: Scalar/ObjectAccessExpressions over the ObjectReferenceExpression that references
                // the JOIN source.
                // However, the JSON object being projected out of the subquery doesn't correspond to any entity type, and there's currently
                // no way for us to represent a reference to that - ObjectReferenceExpression requires an IEntityType. Changing that
                // requires shaper-side changes (see comment in ObjectReferenceExpression); if we can remove that requirement, we can
                // possibly also merge ScalarReferenceExpression and ObjectReferenceExpression to a single SourceReferenceExpression.
                throw new InvalidOperationException(CosmosStrings.ComplexProjectionInSubqueryNotSupported);
            }

            projectionMapping[remappedProjectionMember] = projectionToAdd;
        }

        innerSelect.ApplyProjection();
        _sources.Add(joinSource);

        innerShaper = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(innerShaper);
        _projectionMapping = projectionMapping;
        innerSelect._projectionMapping.Clear();

        return New(
            transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
            new[] { outerShaper, innerShaper }, outerMemberInfo, innerMemberInfo);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void PushdownIntoSubquery()
    {
        if (Offset is not null || Limit is not null)
        {
            // Cosmos doesn't support OFFSET/LIMIT in subqueries.
            // When we translate Skip/Take in subquery context (see NorthwindQueryableMethodTranslatingEV.Translate{Skip,Take},
            // we instead transform the subquery to an array via ARRAY(), and then compose ARRAY_SLICE as an alternative to OFFSET/LIMIT.
            // But we cannot do this in for top-level queries being pushed down, since Cosmos doesn't allow referencing the root container
            // from an ARRAY() subquery.
            throw new InvalidOperationException(CosmosStrings.LimitOffsetNotSupportedInSubqueries);
        }

        // TODO: This is a hack - UseSingleValueProjection should be removed once #25527 is complete - adding VALUE should just be a detail
        // of SQL generation (one projection -> VALUE, multiple projections -> no VALUE)
        var hasMultipleProjections = _projection.Count > 1 || _projectionMapping.Count > 1;

        var subquery = new SelectExpression(
            _sources.ToList(), Predicate, Projection.ToList(), IsDistinct, _orderings.ToList(), Offset, Limit, _aliasManager)
        {
            UsesSingleValueProjection = UsesSingleValueProjection || !hasMultipleProjections
        };

        _sources.Clear();
        Predicate = null;
        IsDistinct = false;
        _orderings.Clear();
        Offset = null;
        Limit = null;

        // If there's just one source in the select being pushed down, bubble up that source's name as the subquery's alias.
        var subqueryAlias =
            _aliasManager.GenerateSourceAlias(_sources is [{ Alias: string singleSourceAlias }] ? singleSourceAlias : "subquery");
        _sources.Add(new SourceExpression(subquery, subqueryAlias));

        // Lift projections
        // var projectionMap = new Dictionary<SqlExpression, ColumnExpression>(ReferenceEqualityComparer.Instance);
        // TODO: In which case do we actually push down a select that already has its projection populated (not pending)?
        if (_projection.Count > 0)
        {
            var projections = _projection.ToList();
            _projection.Clear();

            if (projections is [var singleProjection])
            {
                // _projection.Add(
                //     new ProjectionExpression(
                //         singleProjection.Expression is SqlExpression sqlExpression
                //             ? new ScalarReferenceExpression(subqueryAlias, sqlExpression.Type)
                //             : new ObjectReferenceExpression(null!, subqueryAlias),
                //         singleProjection.Alias));
                // TODO: Move this out to subquery.GenerateOuterReference() like in relational
                if (singleProjection.Expression is SqlExpression sqlExpression)
                {
                    _projection.Add(
                        new ProjectionExpression(new ScalarReferenceExpression(subqueryAlias, sqlExpression.Type), singleProjection.Alias));
                }
                else
                {
                    // TODO: We have a problem with the need to have IEntityType here
                    throw new NotImplementedException();
                    // _projection.Add(
                    //     new ProjectionExpression(new ObjectReferenceExpression(null!, subqueryAlias), singleProjection.Alias));
                }
            }
            else
            {
                // TODO: #34004
                throw new InvalidOperationException(CosmosStrings.ComplexProjectionInSubqueryNotSupported);
            }
        }

        // Lift projection mappings
        foreach (var (projectionMember, expression) in _projectionMapping.ToList())
        {
            // If projectionMapping's value is ConstantExpression then projection has already been applied
            if (expression is ConstantExpression)
            {
                break;
            }

            subquery._projectionMapping[projectionMember] = expression;

            switch (expression)
            {
                case EntityProjectionExpression entityProjection:
                    if (!projectionMember.IsEmpty)
                    {
                        // TODO: #34004
                        throw new InvalidOperationException(CosmosStrings.ComplexProjectionInSubqueryNotSupported);
                    }

                    _projectionMapping[projectionMember] = new EntityProjectionExpression(
                        new ObjectReferenceExpression(entityProjection.EntityType, subqueryAlias),
                        entityProjection.EntityType);
                    break;

                case SqlExpression sqlExpression:
                    if (!projectionMember.IsEmpty)
                    {
                        // TODO: #34004
                        throw new InvalidOperationException(CosmosStrings.ComplexProjectionInSubqueryNotSupported);
                    }

                    _projectionMapping[projectionMember] = new ScalarReferenceExpression(subqueryAlias, sqlExpression.Type);
                    break;

                default:
                    throw new UnreachableException(
                        $"Unknown expression type '{expression.GetType().Name}' in projection mapping when pushing down");
            }
        }

        subquery.ApplyProjection();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => typeof(object);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;

        var projections = new List<ProjectionExpression>();
        IDictionary<ProjectionMember, Expression> projectionMapping;
        if (Projection.Any())
        {
            projectionMapping = _projectionMapping;
            foreach (var item in Projection)
            {
                var projection = (ProjectionExpression)visitor.Visit(item);
                projections.Add(projection);

                changed |= projection != item;
            }
        }
        else
        {
            projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var (projectionMember, expression) in _projectionMapping)
            {
                var newProjection = visitor.Visit(expression);
                changed |= newProjection != expression;

                projectionMapping[projectionMember] = newProjection;
            }
        }

        var sources = new List<SourceExpression>();
        foreach (var source in _sources)
        {
            var visitedSource = (SourceExpression)visitor.Visit(source);
            changed |= visitedSource != source;
            sources.Add(visitedSource);
        }

        var predicate = (SqlExpression?)visitor.Visit(Predicate);
        changed |= predicate != Predicate;

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in _orderings)
        {
            var orderingExpression = (SqlExpression)visitor.Visit(ordering.Expression);
            changed |= orderingExpression != ordering.Expression;
            orderings.Add(ordering.Update(orderingExpression));
        }

        var offset = (SqlExpression?)visitor.Visit(Offset);
        changed |= offset != Offset;

        var limit = (SqlExpression?)visitor.Visit(Limit);
        changed |= limit != Limit;

        if (changed)
        {
            var newSelectExpression =
                new SelectExpression(sources, predicate, projections, IsDistinct, orderings, Offset, Limit, _aliasManager)
                {
                    _projectionMapping = projectionMapping, UsesSingleValueProjection = UsesSingleValueProjection
                };

            return newSelectExpression;
        }

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression Update(
        List<ProjectionExpression> projections,
        List<SourceExpression> sources,
        SqlExpression? predicate,
        List<OrderingExpression> orderings,
        SqlExpression? limit,
        SqlExpression? offset)
    {
        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            projectionMapping[projectionMember] = expression;
        }

        return new SelectExpression(sources, predicate, projections, IsDistinct, orderings, Offset, Limit, _aliasManager)
        {
            _projectionMapping = projectionMapping,
            UsesSingleValueProjection = UsesSingleValueProjection,
            ReadItemInfo = ReadItemInfo
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpression WithSingleValueProjection()
    {
        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            projectionMapping[projectionMember] = expression;
        }

        return new SelectExpression(
            Sources.ToList(), Predicate, Projection.ToList(), IsDistinct, Orderings.ToList(), Offset, Limit, _aliasManager)
        {
            _projectionMapping = projectionMapping, UsesSingleValueProjection = true
        };
    }

    #region Print

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Print(ExpressionPrinter expressionPrinter)
    {
        PrintProjections(expressionPrinter);
        expressionPrinter.AppendLine();
        PrintSql(expressionPrinter);
    }

    private void PrintProjections(ExpressionPrinter expressionPrinter)
    {
        if (_projectionMapping.Count > 0)
        {
            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMember.ToString()).Append(" -> ");
                    expressionPrinter.Visit(expression);
                }
            }
        }
    }

    private void PrintSql(ExpressionPrinter expressionPrinter, bool withTags = true)
    {
        if (withTags)
        {
            // foreach (var tag in Tags)
            // {
            //     expressionPrinter.Append($"-- {tag}");
            // }
        }

        expressionPrinter.Append("SELECT ");

        if (IsDistinct)
        {
            expressionPrinter.Append("DISTINCT ");
        }

        if (Projection.Any())
        {
            if (UsesSingleValueProjection)
            {
                expressionPrinter.Append("VALUE ");
            }

            expressionPrinter.VisitCollection(Projection);
        }
        else
        {
            expressionPrinter.Append("1");
        }

        if (Sources.Count > 0)
        {
            expressionPrinter.AppendLine().Append("FROM ");
            expressionPrinter.Visit(Sources[0]);

            for (var i = 1; i < Sources.Count; i++)
            {
                expressionPrinter.AppendLine().Append("JOIN ");
                expressionPrinter.Visit(Sources[i]);
            }
        }

        if (Predicate != null)
        {
            expressionPrinter.AppendLine().Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }

        if (Orderings.Any())
        {
            expressionPrinter.AppendLine().Append("ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
        }

        if (Offset != null)
        {
            expressionPrinter.AppendLine().Append("OFFSET ");
            expressionPrinter.Visit(Offset);
            expressionPrinter.Append(" ROWS");

            if (Limit != null)
            {
                expressionPrinter.Append(" FETCH NEXT ");
                expressionPrinter.Visit(Limit);
                expressionPrinter.Append(" ROWS ONLY");
            }
        }
    }

    private string PrintShortSql()
    {
        var expressionPrinter = new ExpressionPrinter();
        PrintSql(expressionPrinter, withTags: false);
        return expressionPrinter.ToString();
    }

    /// <summary>
    ///     <para>
    ///         Expand this property in the debugger for a human-readable representation of this <see cref="SelectExpression" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the debug strings.
    ///         They are designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    [EntityFrameworkInternal]
    public string DebugView
        => this.Print();

    #endregion Print

    private sealed class ProjectionMemberRemappingExpressionVisitor(
        SelectExpression queryExpression,
        Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember is not null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    queryExpression,
                    projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.VisitExtension(expression);
        }
    }
}
