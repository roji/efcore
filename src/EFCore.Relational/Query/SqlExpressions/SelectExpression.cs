// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a SELECT in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     This class is not publicly constructable. If this is a problem for your application or provider, then please file
///     an issue at <see href="https://github.com/dotnet/efcore">github.com/dotnet/efcore</see>.
/// </remarks>
// Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
[DebuggerDisplay("{PrintShortSql(), nq}")]
public sealed partial class SelectExpression : TableExpressionBase
{
    internal const string DiscriminatorColumnAlias = "Discriminator";
    private static readonly IdentifierComparer IdentifierComparerInstance = new();

    private readonly List<ProjectionExpression> _projection = [];
    private readonly List<TableExpressionBase> _tables = [];
    private readonly List<SqlExpression> _groupBy = [];
    private readonly List<OrderingExpression> _orderings = [];

    private List<(ColumnExpression Column, ValueComparer Comparer)> _identifier = [];
    private List<(ColumnExpression Column, ValueComparer Comparer)> _childIdentifiers = [];

    private readonly SqlAliasManager _sqlAliasManager;

    internal bool IsMutable { get; set; } = true;
    private Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private List<Expression> _clientProjections = [];
    private List<string?> _aliasForClientProjections = [];
    private CloningExpressionVisitor? _cloningExpressionVisitor;

    // We need to remember identifiers before GroupBy in case it is final GroupBy and element selector has a collection
    // This state doesn't need to propagate
    // It should be only at top-level otherwise GroupBy won't be final operator.
    // Cloning skips it altogether (we don't clone top level with GroupBy)
    // Pushdown should null it out as if GroupBy was present was pushed down.
    private List<(ColumnExpression Column, ValueComparer Comparer)>? _preGroupByIdentifier;

    private SelectExpression(
        string? alias,
        List<TableExpressionBase> tables,
        List<SqlExpression> groupBy,
        List<ProjectionExpression> projections,
        List<OrderingExpression> orderings,
        IReadOnlyDictionary<string, IAnnotation>? annotations,
        SqlAliasManager sqlAliasManager)
        : base(alias, annotations)
    {
        _projection = projections;
        _tables = tables;
        _groupBy = groupBy;
        _orderings = orderings;
        _sqlAliasManager = sqlAliasManager;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression(
        List<TableExpressionBase> tables,
        Expression projection,
        List<(ColumnExpression Column, ValueComparer Comparer)> identifier,
        SqlAliasManager sqlAliasManager)
        : base(null)
    {
        _tables = tables;
        _projectionMapping[new ProjectionMember()] = projection;
        _identifier = identifier;
        _sqlAliasManager = sqlAliasManager;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression(SqlExpression projection, SqlAliasManager sqlAliasManager)
        : this(tables: [], projection, identifier: [], sqlAliasManager)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Immutable selects no longer need to create tables, so no need for an alias manager (note that in the long term, SelectExpression
    // should have an alias manager at all, so this is temporary).
    [EntityFrameworkInternal]
    public static SelectExpression CreateImmutable(string alias, List<TableExpressionBase> tables, List<ProjectionExpression> projection)
        => new(alias, tables, groupBy: [], projections: projection, orderings: [], annotations: null, sqlAliasManager: null!) { IsMutable = false };

    /// <summary>
    ///     The list of tags applied to this <see cref="SelectExpression" />.
    /// </summary>
    public ISet<string> Tags { get; private set; } = new HashSet<string>();

    /// <summary>
    ///     A bool value indicating if DISTINCT is applied to projection of this <see cref="SelectExpression" />.
    /// </summary>
    public bool IsDistinct { get; private set; }

    /// <summary>
    ///     The list of expressions being projected out from the result set.
    /// </summary>
    public IReadOnlyList<ProjectionExpression> Projection
        => _projection;

    /// <summary>
    ///     The list of tables sources used to generate the result set.
    /// </summary>
    public IReadOnlyList<TableExpressionBase> Tables
        => _tables;

    /// <summary>
    ///     The WHERE predicate for the SELECT.
    /// </summary>
    public SqlExpression? Predicate { get; private set; }

    /// <summary>
    ///     The SQL GROUP BY clause for the SELECT.
    /// </summary>
    public IReadOnlyList<SqlExpression> GroupBy
        => _groupBy;

    /// <summary>
    ///     The HAVING predicate for the SELECT when <see cref="GroupBy" /> clause exists.
    /// </summary>
    public SqlExpression? Having { get; private set; }

    /// <summary>
    ///     The list of orderings used to sort the result set.
    /// </summary>
    public IReadOnlyList<OrderingExpression> Orderings
        => _orderings;

    /// <summary>
    ///     The limit applied to the number of rows in the result set.
    /// </summary>
    public SqlExpression? Limit { get; private set; }

    /// <summary>
    ///     The offset to skip rows from the result set.
    /// </summary>
    public SqlExpression? Offset { get; private set; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public void ApplyTags(ISet<string> tags)
        => Tags = tags;

    /// <summary>
    ///     Applies DISTINCT operator to the projections of the <see cref="SelectExpression" />.
    /// </summary>
    [Pure]
    public SelectExpression ApplyDistinct()
    {
        var select = this;

        if (select._clientProjections.Count > 0
            && select._clientProjections.Any(e => e is ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable }))
        {
            throw new InvalidOperationException(RelationalStrings.DistinctOnCollectionNotSupported);
        }

        if (select.Limit != null
            || select.Offset != null)
        {
            select = select.PushdownIntoSubquery2();
        }

        var newIdentifiers = new List<(ColumnExpression Column, ValueComparer Comparer)>();

        if (select._identifier.Count > 0)
        {
            var typeProjectionIdentifiers = new List<ColumnExpression>();
            var typeProjectionValueComparers = new List<ValueComparer>();
            var otherExpressions = new List<SqlExpression>();
            var allExpressionsAreProcessable = true;

            var projections = select._clientProjections.Count > 0 ? select._clientProjections : select._projectionMapping.Values.ToList();
            foreach (var projection in projections)
            {
                switch (projection)
                {
                    case StructuralTypeProjectionExpression { StructuralType: IEntityType entityType } entityProjection
                        when entityType.IsMappedToJson():
                    {
                        // For JSON entities, the identifier is the key that was generated when we convert from json to query root
                        // (OPENJSON, json_each, etc), but we can't use it for distinct, as it would warp the results.
                        // Instead, we will treat every non-key property as identifier.

                        foreach (var property in entityType.GetDeclaredProperties().Where(p => !p.IsPrimaryKey()))
                        {
                            typeProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                            typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                        }

                        break;
                    }

                    case StructuralTypeProjectionExpression { StructuralType: IEntityType entityType } entityProjection
                        when !entityType.IsMappedToJson():
                    {
                        var primaryKey = entityType.FindPrimaryKey();
                        // We know that there are existing identifiers (see condition above); we know we must have a key since a keyless
                        // entity type would have wiped the identifiers when generating the join.
                        Check.DebugAssert(primaryKey != null, "primary key is null.");

                        foreach (var property in primaryKey.Properties)
                        {
                            typeProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                            typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                        }

                        break;
                    }

                    case StructuralTypeProjectionExpression { StructuralType: IComplexType } complexTypeProjection:
                        // When distinct is applied to complex types, all properties - including ones in nested complex types - become
                        // the identifier.
                        ProcessComplexType(complexTypeProjection);

                        void ProcessComplexType(StructuralTypeProjectionExpression complexTypeProjection)
                        {
                            var complexType = (IComplexType)complexTypeProjection.StructuralType;

                            foreach (var property in complexType.GetProperties())
                            {
                                typeProjectionIdentifiers.Add(complexTypeProjection.BindProperty(property));
                                typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                            }

                            foreach (var complexProperty in complexType.GetComplexProperties())
                            {
                                ProcessComplexType(
                                    (StructuralTypeProjectionExpression)complexTypeProjection.BindComplexProperty(complexProperty)
                                        .ValueBufferExpression);
                            }
                        }

                        break;

                    case JsonQueryExpression jsonQueryExpression:
                        if (jsonQueryExpression.IsCollection)
                        {
                            throw new InvalidOperationException(RelationalStrings.DistinctOnCollectionNotSupported);
                        }

                        var primaryKeyProperties = jsonQueryExpression.EntityType.FindPrimaryKey()!.Properties;
                        var primaryKeyPropertiesCount = jsonQueryExpression.IsCollection
                            ? primaryKeyProperties.Count - 1
                            : primaryKeyProperties.Count;

                        for (var i = 0; i < primaryKeyPropertiesCount; i++)
                        {
                            var keyProperty = primaryKeyProperties[i];
                            typeProjectionIdentifiers.Add((ColumnExpression)jsonQueryExpression.BindProperty(keyProperty));
                            typeProjectionValueComparers.Add(keyProperty.GetKeyValueComparer());
                        }

                        break;

                    case SqlExpression sqlExpression:
                        otherExpressions.Add(sqlExpression);
                        break;

                    default:
                        allExpressionsAreProcessable = false;
                        break;
                }
            }

            if (allExpressionsAreProcessable)
            {
                var allOtherExpressions = typeProjectionIdentifiers.Concat(otherExpressions).ToList();
                if (select._identifier.All(e => allOtherExpressions.Contains(e.Column)))
                {
                    newIdentifiers = select._identifier;
                }
                else
                {
                    if (otherExpressions.Count == 0)
                    {
                        // If there are no other expressions then we can use all entityProjectionIdentifiers
                        newIdentifiers.AddRange(typeProjectionIdentifiers.Zip(typeProjectionValueComparers));
                    }
                    else if (otherExpressions.All(e => e is ColumnExpression))
                    {
                        newIdentifiers.AddRange(typeProjectionIdentifiers.Zip(typeProjectionValueComparers));
                        newIdentifiers.AddRange(otherExpressions.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
                    }
                }
            }
        }

        return new SelectExpression(
            Alias, select.Tables.ToList(), select.GroupBy.ToList(), select.Projection.ToList(), orderings: [], Annotations,
            _sqlAliasManager)
        {
            Predicate = select.Predicate,
            Having = select.Having,
            Offset = select.Offset,
            Limit = select.Limit,
            IsDistinct = true,
            Tags = select.Tags,
            IsMutable = select.IsMutable,
            _projectionMapping = select._projectionMapping,
            _clientProjections = select._clientProjections,
            _identifier = newIdentifiers,
            _childIdentifiers = select._childIdentifiers,
            _aliasForClientProjections = select._aliasForClientProjections,
            _preGroupByIdentifier = select._preGroupByIdentifier
        };
    }

    /// <summary>
    ///     Adds expressions from projection mapping to projection ignoring the shaper expression. This method should only be used
    ///     when populating projection in subquery.
    /// </summary>
    public SelectExpression ApplyProjection(IReadOnlyList<Expression>? projections = null)
    {
        if (!IsMutable)
        {
            throw new InvalidOperationException("Applying projection on already finalized select expression");
        }

        var finalProjections = new List<ProjectionExpression>();
        var generateAlias = Alias is not null;
        var processingClientProjections = projections is null && _clientProjections.Count > 0;
        var inputProjections = (IEnumerable<Expression>?)projections
            ?? (_clientProjections.Count > 0 ? _clientProjections : _projectionMapping.Values);

        var i = -1;
        foreach (var p in inputProjections)
        {
            i++;
            switch (p)
            {
                case StructuralTypeProjectionExpression projection:
                    AddStructuralTypeProjection(projection);
                    break;

                case SqlExpression sqlExpression:
                    AddToProjection2(
                        finalProjections, sqlExpression, generateAlias, processingClientProjections ? _aliasForClientProjections[i] : null);
                    break;

                default:
                    throw new InvalidOperationException(
                        "Invalid type of projection to add when not associated with shaper expression.");
            }
        }

        var select = WithProjections(finalProjections);
        select.IsMutable = false;
        return select;

        void AddStructuralTypeProjection(StructuralTypeProjectionExpression projection)
        {
            if (_projection.Count == 0
                && projection is { StructuralType: IComplexType complexType, IsNullable: true })
            {
                throw new InvalidOperationException(RelationalStrings.CannotProjectNullableComplexType(complexType.DisplayName()));
            }

            ProcessTypeProjection(projection);

            void ProcessTypeProjection(StructuralTypeProjectionExpression projection)
            {
                foreach (var property in projection.StructuralType.GetAllPropertiesInHierarchy())
                {
                    AddToProjection2(finalProjections, projection.BindProperty(property), generateAlias);
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(projection.StructuralType))
                {
                    ProcessTypeProjection(
                        (StructuralTypeProjectionExpression)projection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            if (projection.DiscriminatorExpression != null)
            {
                AddToProjection2(
                    finalProjections, projection.DiscriminatorExpression, generateAlias, DiscriminatorColumnAlias);
            }
        }
    }

    /// <summary>
    ///     Adds expressions from projection mapping to projection and generate updated shaper expression for materialization.
    /// </summary>
    /// <param name="shaperExpression">Current shaper expression which will shape results of this select expression.</param>
    /// <param name="resultCardinality">The result cardinality of this query expression.</param>
    /// <param name="querySplittingBehavior">The query splitting behavior to use when applying projection for nested collections.</param>
    /// <returns>Returns modified shaper expression to shape results of this select expression.</returns>
    public ShapedQueryExpression ApplyProjection(
        Expression shaperExpression,
        ResultCardinality resultCardinality,
        QuerySplittingBehavior querySplittingBehavior)
    {
        if (!IsMutable)
        {
            throw new InvalidOperationException("Applying projection on already finalized select expression");
        }

        IsMutable = false;
        var select = this;
        var finalProjections = new List<ProjectionExpression>();

        if (shaperExpression is RelationalGroupByShaperExpression relationalGroupByShaperExpression)
        {
            // This is final GroupBy operation
            Check.DebugAssert(select._groupBy.Count > 0, "The selectExpression doesn't have grouping terms.");

            if (select._clientProjections.Count == 0)
            {
                // Force client projection because we would be injecting keys and client-side key comparison
                var mapping = ConvertProjectionMappingToClientProjections(select._projectionMapping);
                var innerShaperExpression = new ProjectionMemberToIndexConvertingExpressionVisitor(select, mapping).Visit(
                    relationalGroupByShaperExpression.ElementSelector);
                shaperExpression = new RelationalGroupByShaperExpression(
                    relationalGroupByShaperExpression.KeySelector,
                    innerShaperExpression,
                    relationalGroupByShaperExpression.GroupingEnumerable);
            }

            // Convert GroupBy to OrderBy
            foreach (var groupingTerm in select._groupBy)
            {
                AppendOrdering(new OrderingExpression(groupingTerm, ascending: true));
            }

            select = select.Update(
                select.Projection,
                select.Tables,
                select.Predicate,
                groupBy: [],
                select.Having,
                select.Orderings,
                select.Limit,
                select.Offset);
            // We do processing of adding key terms to projection when applying projection so we can move offsets for other
            // projections correctly
        }

        if (select._clientProjections.Count > 0)
        {
            EntityShaperNullableMarkingExpressionVisitor? entityShaperNullableMarkingExpressionVisitor = null;
            CloningExpressionVisitor? cloningExpressionVisitor = null;
            var pushdownOccurred = false;
            var containsCollection = false;
            var containsSingleResult = false;
            var jsonClientProjectionsCount = 0;

            foreach (var projection in select._clientProjections)
            {
                if (projection is ShapedQueryExpression sqe)
                {
                    if (sqe.ResultCardinality == ResultCardinality.Enumerable)
                    {
                        containsCollection = true;
                    }

                    if (sqe.ResultCardinality is ResultCardinality.Single or ResultCardinality.SingleOrDefault)
                    {
                        containsSingleResult = true;
                    }
                }

                if (projection is JsonQueryExpression)
                {
                    jsonClientProjectionsCount++;
                }
            }

            if (containsSingleResult
                || (querySplittingBehavior == QuerySplittingBehavior.SingleQuery && containsCollection))
            {
                // Pushdown outer since we will be adding join to this
                // For grouping query pushdown will not occur since we don't allow this terms to compose (yet!).
                if (select.Limit != null
                    || select.Offset != null
                    || select.IsDistinct
                    || select.GroupBy.Count > 0)
                {
                    select = PushdownIntoSubquery2();
                    pushdownOccurred = true;
                }

                entityShaperNullableMarkingExpressionVisitor = new EntityShaperNullableMarkingExpressionVisitor();
            }

            if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery
                && (containsSingleResult || containsCollection))
            {
                // SingleResult can lift collection from inner

                // Specifically for here, we want to avoid cloning the client projection; if we do, when applying the projection on the
                // cloned inner query we go into an endless recursion.

                // Note that we create a CloningExpressionVisitor without a SQL alias manager - this means that aliases won't get uniquified
                // as expressions are being cloned. Since we're cloning here to get a completely separate (split) query, that makes sense
                // as we don't want aliases to be unique across different queries (but in other contexts, when the cloned fragment gets
                // integrated back into the same query (e.g. GroupBy) we do want to uniquify aliases).
                cloningExpressionVisitor = new CloningExpressionVisitor(sqlAliasManager: null, cloneClientProjections: false);
            }

            var earlierClientProjectionCount = select._clientProjections.Count;
            var newClientProjections = new List<Expression>();
            var clientProjectionIndexMap = new List<object>();
            var remappingRequired = false;

            if (shaperExpression is RelationalGroupByShaperExpression groupByShaper)
            {
                // We need to add key to projection and generate key selector in terms of projectionBindings
                var projectionBindingMap = new Dictionary<SqlExpression, Expression>();
                // TODO: This mutates
                var keySelector = AddGroupByKeySelectorToProjection(
                    this, newClientProjections, projectionBindingMap, groupByShaper.KeySelector);
                var (keyIdentifier, keyIdentifierValueComparers) = GetIdentifierAccessor(
                    this, newClientProjections, projectionBindingMap, select._identifier);
                select._identifier.Clear();
                select._identifier.AddRange(select._preGroupByIdentifier!);
                select._preGroupByIdentifier!.Clear();

                Expression AddGroupByKeySelectorToProjection(
                    SelectExpression selectExpression,
                    List<Expression> clientProjectionList,
                    Dictionary<SqlExpression, Expression> projectionBindingMap,
                    Expression keySelector)
                {
                    switch (keySelector)
                    {
                        case SqlExpression sqlExpression:
                        {
                            var index = selectExpression.AddToProjection(sqlExpression);
                            var clientProjectionToAdd = Constant(index);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            var projectionBindingExpression = sqlExpression.Type.IsNullableType()
                                ? (Expression)new ProjectionBindingExpression(selectExpression, existingIndex, sqlExpression.Type)
                                : Convert(
                                    new ProjectionBindingExpression(
                                        selectExpression, existingIndex, sqlExpression.Type.MakeNullable()),
                                    sqlExpression.Type);
                            projectionBindingMap[sqlExpression] = projectionBindingExpression;
                            return projectionBindingExpression;
                        }

                        case NewExpression newExpression:
                            var newArguments = new Expression[newExpression.Arguments.Count];
                            for (var i = 0; i < newExpression.Arguments.Count; i++)
                            {
                                var newArgument = AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, newExpression.Arguments[i]);
                                newArguments[i] = newExpression.Arguments[i].Type != newArgument.Type
                                    ? Convert(newArgument, newExpression.Arguments[i].Type)
                                    : newArgument;
                            }

                            return newExpression.Update(newArguments);

                        case MemberInitExpression memberInitExpression:
                            var updatedNewExpression = AddGroupByKeySelectorToProjection(
                                selectExpression, clientProjectionList, projectionBindingMap, memberInitExpression.NewExpression);
                            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
                            for (var i = 0; i < newBindings.Length; i++)
                            {
                                var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                                var newAssignmentExpression = AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, memberAssignment.Expression);
                                newBindings[i] = memberAssignment.Update(
                                    memberAssignment.Expression.Type != newAssignmentExpression.Type
                                        ? Convert(newAssignmentExpression, memberAssignment.Expression.Type)
                                        : newAssignmentExpression);
                            }

                            return memberInitExpression.Update((NewExpression)updatedNewExpression, newBindings);

                        case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression:
                            return unaryExpression.Update(
                                AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, unaryExpression.Operand));

                        case StructuralTypeShaperExpression
                        {
                            ValueBufferExpression: StructuralTypeProjectionExpression projection
                        } shaper:
                        {
                            var clientProjectionToAdd = AddStructuralTypeProjection(projection);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            return shaper.Update(
                                new ProjectionBindingExpression(selectExpression, existingIndex, typeof(ValueBuffer)));
                        }

                        default:
                            throw new InvalidOperationException(
                                RelationalStrings.InvalidKeySelectorForGroupBy(keySelector, keySelector.GetType()));
                    }
                }

                static (Expression, IReadOnlyList<ValueComparer>) GetIdentifierAccessor(
                    SelectExpression selectExpression,
                    List<Expression> clientProjectionList,
                    Dictionary<SqlExpression, Expression> projectionBindingMap,
                    IEnumerable<(ColumnExpression Column, ValueComparer Comparer)> identifyingProjection)
                {
                    var updatedExpressions = new List<Expression>();
                    var comparers = new List<ValueComparer>();
                    foreach (var (column, comparer) in identifyingProjection)
                    {
                        if (!projectionBindingMap.TryGetValue(column, out var mappedExpression))
                        {
                            var index = selectExpression.AddToProjection(column);
                            var clientProjectionToAdd = Constant(index);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            mappedExpression = new ProjectionBindingExpression(selectExpression, existingIndex, column.Type.MakeNullable());
                        }

                        updatedExpressions.Add(
                            mappedExpression.Type.IsValueType
                                ? Convert(mappedExpression, typeof(object))
                                : mappedExpression);
                        comparers.Add(comparer);
                    }

                    return (NewArrayInit(typeof(object), updatedExpressions), comparers);
                }

                remappingRequired = true;
                shaperExpression = new RelationalGroupByResultExpression(
                    keyIdentifier, keyIdentifierValueComparers, keySelector, groupByShaper.ElementSelector);
            }

            SelectExpression? baseSelectExpression = null;
            if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery && containsCollection)
            {
                // Needs to happen after converting final GroupBy so we clone correct form.
                baseSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(this);
                // We mark this as mutable because the split query will combine into this and take it over.
                baseSelectExpression.IsMutable = true;
                if (resultCardinality is ResultCardinality.Single or ResultCardinality.SingleOrDefault)
                {
                    // Update limit since split queries don't need limit 2
                    if (pushdownOccurred)
                    {
                        var subquery = (SelectExpression)select.Tables[0];
                        if (subquery.Limit is SqlConstantExpression { Value: 2 } limitConstantExpression)
                        {
                            select = select.WithTables(
                                [subquery.WithLimit(new SqlConstantExpression(Constant(1), limitConstantExpression.TypeMapping))]);
                        }
                    }
                    else if (select.Limit is SqlConstantExpression { Value: 2 } limitConstantExpression)
                    {
                        select = select.WithLimit(new SqlConstantExpression(Constant(1), limitConstantExpression.TypeMapping));
                    }
                }
            }

            for (var i = 0; i < select._clientProjections.Count; i++)
            {
                if (i == earlierClientProjectionCount)
                {
                    // Since we lift nested client projections for single results up, we may need to re-clone the baseSelectExpression
                    // again so it does contain the single result subquery too. We erase projections for it since it would be non-empty.
                    earlierClientProjectionCount = select._clientProjections.Count;
                    if (cloningExpressionVisitor != null)
                    {
                        // TODO: Remove cloning etc.
                        baseSelectExpression = (SelectExpression)cloningExpressionVisitor.Visit(select);
                        baseSelectExpression.IsMutable = true;
                        baseSelectExpression = baseSelectExpression.WithProjections([]);
                    }
                }

                var value = select._clientProjections[i];
                switch (value)
                {
                    case StructuralTypeProjectionExpression projection:
                    {
                        var result = AddStructuralTypeProjection(projection);
                        newClientProjections.Add(result);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case JsonQueryExpression jsonQueryExpression:
                    {
                        var jsonProjectionResult = AddJsonProjection(jsonQueryExpression);
                        newClientProjections.Add(jsonProjectionResult);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case SqlExpression sqlExpression:
                    {
                        var result = Constant(
                            AddToProjection2(finalProjections, sqlExpression, generateAlias: false, _aliasForClientProjections[i]));
                        newClientProjections.Add(result);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case ShapedQueryExpression
                    {
                        ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault
                    } shapedQueryExpression:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                        var innerShaperExpression = shapedQueryExpression.ShaperExpression;
                        if (innerSelectExpression._clientProjections.Count == 0)
                        {
                            var mapping = innerSelectExpression.ConvertProjectionMappingToClientProjections(
                                innerSelectExpression._projectionMapping);
                            innerShaperExpression =
                                new ProjectionMemberToIndexConvertingExpressionVisitor(innerSelectExpression, mapping)
                                    .Visit(innerShaperExpression);
                        }

                        var innerExpression = RemoveConvert(innerShaperExpression);
                        if (innerExpression is not (StructuralTypeShaperExpression or IncludeExpression))
                        {
                            var sentinelExpression = innerSelectExpression.Limit!;
                            var sentinelNullableType = sentinelExpression.Type.MakeNullable();
                            innerSelectExpression._clientProjections.Add(sentinelExpression);
                            innerSelectExpression._aliasForClientProjections.Add(null);
                            var dummyProjection = new ProjectionBindingExpression(
                                innerSelectExpression, innerSelectExpression._clientProjections.Count - 1, sentinelNullableType);

                            var defaultResult = shapedQueryExpression.ResultCardinality == ResultCardinality.SingleOrDefault
                                ? (Expression)Default(innerShaperExpression.Type)
                                : Block(
                                    Throw(
                                        New(
                                            typeof(InvalidOperationException).GetConstructors()
                                                .Single(
                                                    ci =>
                                                    {
                                                        var parameters = ci.GetParameters();
                                                        return parameters.Length == 1
                                                            && parameters[0].ParameterType == typeof(string);
                                                    }),
                                            Constant(CoreStrings.SequenceContainsNoElements))),
                                    Default(innerShaperExpression.Type));

                            innerShaperExpression = Condition(
                                Equal(dummyProjection, Default(sentinelNullableType)),
                                defaultResult,
                                innerShaperExpression);
                        }

                        AddJoin(JoinType.OuterApply, ref innerSelectExpression, out _);
                        var offset = select._clientProjections.Count;
                        var count = innerSelectExpression._clientProjections.Count;

                        select._clientProjections.AddRange(
                            innerSelectExpression._clientProjections.Select(e => MakeNullable(e, nullable: true)));

                        select._aliasForClientProjections.AddRange(innerSelectExpression._aliasForClientProjections);
                        innerShaperExpression = new ProjectionIndexRemappingExpressionVisitor(
                                innerSelectExpression,
                                select,
                                Enumerable.Range(offset, count).ToArray())
                            .Visit(innerShaperExpression);
                        innerShaperExpression = entityShaperNullableMarkingExpressionVisitor!.Visit(innerShaperExpression);
                        clientProjectionIndexMap.Add(innerShaperExpression);
                        remappingRequired = true;
                        break;

                        static Expression RemoveConvert(Expression expression)
                            => expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                                ? RemoveConvert(unaryExpression.Operand)
                                : expression;
                    }

                    case ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQueryExpression:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                        if (select._identifier.Count == 0
                            || innerSelectExpression._identifier.Count == 0)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin);
                        }

                        var innerShaperExpression = shapedQueryExpression.ShaperExpression;
                        if (innerSelectExpression._clientProjections.Count == 0)
                        {
                            var mapping = innerSelectExpression.ConvertProjectionMappingToClientProjections(
                                innerSelectExpression._projectionMapping);
                            innerShaperExpression =
                                new ProjectionMemberToIndexConvertingExpressionVisitor(innerSelectExpression, mapping)
                                    .Visit(innerShaperExpression);
                        }

                        if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery)
                        {
                            var outerSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(baseSelectExpression!);

                            if (outerSelectExpression.Limit != null
                                || outerSelectExpression.Offset != null
                                || outerSelectExpression.IsDistinct
                                || outerSelectExpression._groupBy.Count > 0)
                            {
                                // We do pushdown after making sure that inner contains references to outer only
                                // so that when we do pushdown, we can update inner and maintain graph
                                (outerSelectExpression, var sqlRemappingVisitor) = outerSelectExpression.PushdownIntoSubqueryInternal2();
                                innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression, out var subquery);
                                outerSelectExpression = outerSelectExpression.WithTables([subquery]);
                            }

                            var actualParentIdentifier = select._identifier.Take(outerSelectExpression._identifier.Count).ToList();
                            var containsOrdering = innerSelectExpression.Orderings.Count > 0;
                            List<OrderingExpression>? orderingsToBeErased = null;
                            if (containsOrdering
                                && innerSelectExpression.Limit == null
                                && innerSelectExpression.Offset == null)
                            {
                                orderingsToBeErased = innerSelectExpression.Orderings.ToList();
                            }

                            var parentIdentifier = GetIdentifierAccessor(select, newClientProjections, actualParentIdentifier).Item1;

                            outerSelectExpression = outerSelectExpression.AddJoin2(
                                JoinType.CrossApply, ref innerSelectExpression, out var pushdownOccurredWhenJoining);
                            outerSelectExpression._clientProjections.AddRange(innerSelectExpression._clientProjections);
                            outerSelectExpression._aliasForClientProjections.AddRange(innerSelectExpression._aliasForClientProjections);
                            innerSelectExpression = outerSelectExpression;

                            var parentOrderings = new List<OrderingExpression>(select.Orderings);
                            var innerOrderings = new List<OrderingExpression>(innerSelectExpression.Orderings);

                            for (var j = 0; j < actualParentIdentifier.Count; j++)
                            {
                                AppendOrdering(
                                    parentOrderings, new OrderingExpression(actualParentIdentifier[j].Column, ascending: true));
                                AppendOrdering(
                                    innerOrderings, new OrderingExpression(innerSelectExpression._identifier[j].Column, ascending: true));
                            }

                            select = select.WithOrderings(parentOrderings);

                            // Copy over any nested ordering if there were any
                            if (containsOrdering)
                            {
                                var collectionJoinedInnerTable = ((JoinExpressionBase)innerSelectExpression._tables[^1]).Table;
                                if (orderingsToBeErased != null)
                                {
                                    // Ordering was present but erased so we add again
                                    if (pushdownOccurredWhenJoining)
                                    {
                                        // We lift from inner subquery if pushdown occurred with ordering erased
                                        var subquery = (SelectExpression)collectionJoinedInnerTable;
                                        foreach (var ordering in orderingsToBeErased)
                                        {
                                            innerOrderings.Add(
                                                new OrderingExpression(
                                                    subquery.GenerateOuterColumn(
                                                        collectionJoinedInnerTable.GetRequiredAlias(), ordering.Expression),
                                                    ordering.IsAscending));
                                        }
                                    }
                                    else
                                    {
                                        // We copy from inner if pushdown did not happen but ordering was left behind when
                                        // generating join
                                        innerOrderings.AddRange(orderingsToBeErased);
                                    }
                                }
                                else
                                {
                                    // If orderings were not erased then they must be present in inner
                                    GetOrderingsFromInnerTable(collectionJoinedInnerTable, innerOrderings);
                                }
                            }

                            innerSelectExpression = innerSelectExpression.WithOrderings(innerOrderings);

                            var innerShapedQuery = innerSelectExpression.ApplyProjection(
                                innerShaperExpression, shapedQueryExpression.ResultCardinality, querySplittingBehavior);

                            innerSelectExpression = (SelectExpression)innerShapedQuery.QueryExpression;
                            innerShaperExpression = innerShapedQuery.ShaperExpression;

                            var (childIdentifier, childIdentifierValueComparers) = GetIdentifierAccessor(
                                innerSelectExpression,
                                innerSelectExpression._clientProjections,
                                innerSelectExpression._identifier.Take(_identifier.Count));

                            var result = new SplitCollectionInfo(
                                parentIdentifier, childIdentifier, childIdentifierValueComparers,
                                innerSelectExpression, innerShaperExpression);
                            clientProjectionIndexMap.Add(result);
                        }
                        else // Single query
                        {
                            var parentIdentifierList =
                                select._identifier.Except(select._childIdentifiers, IdentifierComparerInstance).ToList();
                            var (parentIdentifier, parentIdentifierValueComparers) = GetIdentifierAccessor(
                                select, newClientProjections, parentIdentifierList);
                            var (outerIdentifier, outerIdentifierValueComparers) = GetIdentifierAccessor(
                                select, newClientProjections, select._identifier);

                            var orderings = new List<OrderingExpression>(select.Orderings);
                            foreach (var identifier in select._identifier)
                            {
                                AppendOrdering(orderings, new OrderingExpression(identifier.Column, ascending: true));
                            }

                            select = select.WithOrderings(orderings);

                            innerShaperExpression = innerSelectExpression.ApplyProjection(
                                innerShaperExpression, shapedQueryExpression.ResultCardinality, querySplittingBehavior);

                            var containsOrdering = innerSelectExpression.Orderings.Count > 0;
                            List<OrderingExpression>? orderingsToBeErased = null;
                            if (containsOrdering
                                && innerSelectExpression.Limit == null
                                && innerSelectExpression.Offset == null)
                            {
                                orderingsToBeErased = innerSelectExpression.Orderings.ToList();
                            }

                            select = AddJoin2(JoinType.OuterApply, ref innerSelectExpression, out var pushdownOccurredWhenJoining);

                            // Copy over any nested ordering if there were any
                            if (containsOrdering)
                            {
                                var collectionJoinedInnerTable = innerSelectExpression._tables[0];
                                var innerOrderingExpressions = new List<OrderingExpression>();
                                if (orderingsToBeErased != null)
                                {
                                    // Ordering was present but erased so we add again
                                    if (pushdownOccurredWhenJoining)
                                    {
                                        // We lift from inner subquery if pushdown occurred with ordering erased
                                        var subquery = (SelectExpression)collectionJoinedInnerTable;
                                        foreach (var ordering in orderingsToBeErased)
                                        {
                                            innerOrderingExpressions.Add(
                                                new OrderingExpression(
                                                    subquery.GenerateOuterColumn(
                                                        collectionJoinedInnerTable.GetRequiredAlias(), ordering.Expression),
                                                    ordering.IsAscending));
                                        }
                                    }
                                    else
                                    {
                                        // We copy from inner if pushdown did not happen but ordering was left behind when
                                        // generating join
                                        innerOrderingExpressions.AddRange(orderingsToBeErased);
                                    }
                                }
                                else
                                {
                                    // If orderings were not erased then they must be present in inner
                                    GetOrderingsFromInnerTable(collectionJoinedInnerTable, innerOrderingExpressions);
                                }

                                for (var j = 0; j < innerOrderingExpressions.Count; j++)
                                {
                                    var ordering = innerOrderingExpressions[j];
                                    innerOrderingExpressions[j] = ordering.Update(MakeNullable(ordering.Expression, nullable: true));
                                }

                                select = select.WithOrderings(innerOrderingExpressions);
                            }

                            innerShaperExpression = CopyProjectionToOuter(innerSelectExpression, innerShaperExpression);
                            var (selfIdentifier, selfIdentifierValueComparers) = GetIdentifierAccessor(
                                this,
                                newClientProjections,
                                innerSelectExpression._identifier
                                    .Except(innerSelectExpression._childIdentifiers, IdentifierComparerInstance)
                                    .Select(e => (e.Column.MakeNullable(), e.Comparer)));

                            OrderingExpression? pendingOrdering = null;
                            foreach (var (identifierColumn, identifierComparer) in innerSelectExpression._identifier)
                            {
                                var updatedColumn = identifierColumn.MakeNullable();
                                _childIdentifiers.Add((updatedColumn, identifierComparer));

                                // We omit the last ordering as an optimization
                                var orderingExpression = new OrderingExpression(updatedColumn, ascending: true);

                                if (!_orderings.Any(o => o.Expression.Equals(updatedColumn)))
                                {
                                    if (pendingOrdering is not null)
                                    {
                                        if (orderingExpression.Equals(pendingOrdering))
                                        {
                                            continue;
                                        }

                                        AppendOrderingInternal(pendingOrdering);
                                    }

                                    pendingOrdering = orderingExpression;
                                }
                            }

                            var result = new SingleCollectionInfo(
                                parentIdentifier, outerIdentifier, selfIdentifier,
                                parentIdentifierValueComparers, outerIdentifierValueComparers, selfIdentifierValueComparers,
                                innerShaperExpression);
                            clientProjectionIndexMap.Add(result);
                        }

                        remappingRequired = true;

                        static (Expression, IReadOnlyList<ValueComparer>) GetIdentifierAccessor(
                            SelectExpression selectExpression,
                            List<Expression> clientProjectionList,
                            IEnumerable<(ColumnExpression Column, ValueComparer Comparer)> identifyingProjection)
                        {
                            var updatedExpressions = new List<Expression>();
                            var comparers = new List<ValueComparer>();
                            foreach (var (column, comparer) in identifyingProjection)
                            {
                                var index = selectExpression.AddToProjection(column, null);
                                var clientProjectionToAdd = Constant(index);
                                var existingIndex = clientProjectionList.FindIndex(
                                    e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                                if (existingIndex == -1)
                                {
                                    clientProjectionList.Add(Constant(index));
                                    existingIndex = clientProjectionList.Count - 1;
                                }

                                var projectionBindingExpression = new ProjectionBindingExpression(
                                    selectExpression, existingIndex, column.Type.MakeNullable());

                                updatedExpressions.Add(
                                    projectionBindingExpression.Type.IsValueType
                                        ? Convert(projectionBindingExpression, typeof(object))
                                        : projectionBindingExpression);
                                comparers.Add(comparer);
                            }

                            return (NewArrayInit(typeof(object), updatedExpressions), comparers);
                        }

                        break;
                    }

                    default:
                        throw new InvalidOperationException(value.GetType().ToString());
                }
            }

            if (remappingRequired)
            {
                shaperExpression = new ClientProjectionRemappingExpressionVisitor(clientProjectionIndexMap).Visit(shaperExpression);
            }

            _clientProjections = newClientProjections;
            _aliasForClientProjections.Clear();

            return shaperExpression;

            void GetOrderingsFromInnerTable(TableExpressionBase tableExpressionBase, List<OrderingExpression> orderings)
            {
                var tableAlias = tableExpressionBase.GetRequiredAlias();

                // If operation was converted to predicate join (inner/left join), then ordering will be in a RowNumberExpression
                if (tableExpressionBase is SelectExpression
                    {
                        Tables: [SelectExpression rowNumberSubquery],
                        Predicate: not null
                    } joinedSubquery
                    && rowNumberSubquery.Projection.Select(pe => pe.Expression)
                        .OfType<RowNumberExpression>().SingleOrDefault() is RowNumberExpression rowNumberExpression)
                {
                    var rowNumberSubqueryTableAlias = joinedSubquery.Tables.Single().GetRequiredAlias();
                    foreach (var partition in rowNumberExpression.Partitions)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                joinedSubquery.GenerateOuterColumn(
                                    tableAlias,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableAlias, partition)),
                                ascending: true));
                    }

                    foreach (var ordering in rowNumberExpression.Orderings)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                joinedSubquery.GenerateOuterColumn(
                                    tableAlias,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableAlias, ordering.Expression)),
                                ordering.IsAscending));
                    }
                }
                // If operation remained apply then ordering will be in the subquery
                else if (tableExpressionBase is SelectExpression { Orderings.Count: > 0 } collectionSelectExpression)
                {
                    foreach (var ordering in collectionSelectExpression.Orderings)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                collectionSelectExpression.GenerateOuterColumn(tableAlias, ordering.Expression),
                                ordering.IsAscending));
                    }
                }
            }

            Expression CopyProjectionToOuter(SelectExpression innerSelectExpression, Expression innerShaperExpression)
            {
                var projectionIndexMap = new int[innerSelectExpression._projection.Count];
                for (var j = 0; j < projectionIndexMap.Length; j++)
                {
                    var projection = MakeNullable(innerSelectExpression._projection[j].Expression, nullable: true);
                    var index = AddToProjection(projection);
                    projectionIndexMap[j] = index;
                }

                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var j = 0; j < indexMap.Length; j++)
                {
                    var constantValue = ((ConstantExpression)innerSelectExpression._clientProjections[j]).Value!;
                    ConstantExpression remappedConstant;
                    if (constantValue is Dictionary<IProperty, int> entityDictionary)
                    {
                        var newDictionary = new Dictionary<IProperty, int>(entityDictionary.Count);
                        foreach (var (property, value) in entityDictionary)
                        {
                            newDictionary[property] = projectionIndexMap[value];
                        }

                        remappedConstant = Constant(newDictionary);
                    }
                    else if (constantValue is JsonProjectionInfo jsonProjectionInfo)
                    {
                        var newKeyAccessInfo = new List<(IProperty?, int?, int?)>();
                        foreach (var (keyProperty, constantKeyValue, keyProjectionIndex) in jsonProjectionInfo.KeyAccessInfo)
                        {
                            newKeyAccessInfo.Add(
                                (keyProperty, constantKeyValue,
                                    keyProjectionIndex != null ? projectionIndexMap[keyProjectionIndex.Value] : null));
                        }

                        remappedConstant = Constant(
                            new JsonProjectionInfo(
                                projectionIndexMap[jsonProjectionInfo.JsonColumnIndex],
                                newKeyAccessInfo));
                    }
                    else if (constantValue is QueryableJsonProjectionInfo queryableJsonProjectionInfo)
                    {
                        var newPropertyIndexMap = new Dictionary<IProperty, int>(queryableJsonProjectionInfo.PropertyIndexMap.Count);
                        foreach (var (property, value) in queryableJsonProjectionInfo.PropertyIndexMap)
                        {
                            newPropertyIndexMap[property] = projectionIndexMap[value];
                        }

                        var newChildrenProjectionInfo = new List<(JsonProjectionInfo, INavigation)>();
                        foreach (var childProjectionInfo in queryableJsonProjectionInfo.ChildrenProjectionInfo)
                        {
                            var newKeyAccessInfo = new List<(IProperty?, int?, int?)>();
                            foreach (var (keyProperty, constantKeyValue, keyProjectionIndex) in childProjectionInfo.JsonProjectionInfo
                                         .KeyAccessInfo)
                            {
                                newKeyAccessInfo.Add(
                                    (keyProperty, constantKeyValue,
                                        keyProjectionIndex != null ? projectionIndexMap[keyProjectionIndex.Value] : null));
                            }

                            newChildrenProjectionInfo.Add(
                                (new JsonProjectionInfo(
                                        projectionIndexMap[childProjectionInfo.JsonProjectionInfo.JsonColumnIndex],
                                        newKeyAccessInfo),
                                    childProjectionInfo.Navigation));
                        }

                        remappedConstant = Constant(
                            new QueryableJsonProjectionInfo(newPropertyIndexMap, newChildrenProjectionInfo));
                    }
                    else
                    {
                        remappedConstant = Constant(projectionIndexMap[(int)constantValue]);
                    }

                    newClientProjections.Add(remappedConstant);
                    indexMap[j] = newClientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();
                innerShaperExpression =
                    new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, this, indexMap).Visit(innerShaperExpression);
                innerShaperExpression = entityShaperNullableMarkingExpressionVisitor!.Visit(innerShaperExpression);

                return innerShaperExpression;
            }
        }

        {
            var result = new Dictionary<ProjectionMember, Expression>(_projectionMapping.Count);

            foreach (var (projectionMember, expression) in _projectionMapping)
            {
                result[projectionMember] = expression switch
                {
                    StructuralTypeProjectionExpression projection => AddStructuralTypeProjection(projection),
                    JsonQueryExpression jsonQueryExpression => AddJsonProjection(jsonQueryExpression),
                    _ => Constant(AddToProjection((SqlExpression)expression, projectionMember.Last?.Name))
                };
            }

            _projectionMapping.Clear();
            _projectionMapping = result;

            return shaperExpression;
        }

        ConstantExpression AddStructuralTypeProjection(StructuralTypeProjectionExpression projection)
        {
            if (projection is { StructuralType: IComplexType complexType, IsNullable: true })
            {
                throw new InvalidOperationException(RelationalStrings.CannotProjectNullableComplexType(complexType.DisplayName()));
            }

            // JSON entity that had some query operations applied on it - it has been converted to a query root via OPENJSON/json_each
            // so it requires different materialization path than regular entity
            // e.g. we need to also add all the child navigations, JSON entity builds all the includes as part of it's own materializer
            // rather than relying on IncludeExpressions in the shaper query
            // also, we don't want to add projection map for synthesized keys, whereas regular entity needs to project every single property it has
            if (projection is { StructuralType: IEntityType entityType }
                && entityType.IsMappedToJson())
            {
                var propertyIndexMap = new Dictionary<IProperty, int>();
                var ownerEntity = entityType;

                do
                {
                    var ownership = ownerEntity.FindOwnership();
                    if (ownership != null)
                    {
                        ownerEntity = ownership.PrincipalEntityType;
                    }
                }
                while (ownerEntity.IsMappedToJson());

                var keyPropertyCount = ownerEntity.FindPrimaryKey()!.Properties.Count;
                foreach (var property in entityType.FindPrimaryKey()!.Properties.Take(keyPropertyCount)
                             .Concat(entityType.GetDeclaredProperties().Where(p => p.GetJsonPropertyName() is not null)))
                {
                    propertyIndexMap[property] = AddToProjection2(
                        finalProjections, projection.BindProperty(property), generateAlias: false);
                }

                var childrenProjectionInfo = new List<(JsonProjectionInfo, INavigation)>();
                foreach (var ownedNavigation in entityType.GetNavigations().Where(
                             n => n.TargetEntityType.IsMappedToJson()
                                 && n.ForeignKey.IsOwnership
                                 && n == n.ForeignKey.PrincipalToDependent))
                {
                    var jsonQueryExpression = (JsonQueryExpression)projection.BindNavigation(ownedNavigation)!.ValueBufferExpression;
                    var jsonProjectionInfo = (JsonProjectionInfo)AddJsonProjection(jsonQueryExpression).Value!;
                    childrenProjectionInfo.Add((jsonProjectionInfo, ownedNavigation));
                }

                return Constant(new QueryableJsonProjectionInfo(propertyIndexMap, childrenProjectionInfo));
            }

            var projections = new Dictionary<IProperty, int>();

            ProcessType(projection);

            void ProcessType(StructuralTypeProjectionExpression typeProjection)
            {
                foreach (var property in typeProjection.StructuralType.GetAllPropertiesInHierarchy())
                {
                    if (typeProjection is { StructuralType: IEntityType entityType }
                        && entityType.IsMappedToJson()
                        && property.IsOrdinalKeyProperty())
                    {
                        continue;
                    }

                    projections[property] = AddToProjection(typeProjection.BindProperty(property), alias: null);
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(typeProjection.StructuralType))
                {
                    ProcessType(
                        (StructuralTypeProjectionExpression)typeProjection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            if (projection.DiscriminatorExpression is not null)
            {
                AddToProjection(projection.DiscriminatorExpression, DiscriminatorColumnAlias);
            }

            return Constant(projections);
        }

        ConstantExpression AddJsonProjection(JsonQueryExpression jsonQueryExpression)
        {
            var jsonScalarExpression = new JsonScalarExpression(
                jsonQueryExpression.JsonColumn,
                jsonQueryExpression.Path,
                jsonQueryExpression.JsonColumn.Type,
                jsonQueryExpression.JsonColumn.TypeMapping!,
                jsonQueryExpression.IsNullable);

            finalProjections.Add(new ProjectionExpression(jsonScalarExpression, ""));
            var jsonColumnIndex = select._projection.Count - 1;
            var keyAccessInfo = new List<(IProperty?, int?, int?)>();
            var keyProperties = GetMappedKeyProperties(jsonQueryExpression.EntityType.FindPrimaryKey()!);
            foreach (var keyProperty in keyProperties)
            {
                var keyColumn = jsonQueryExpression.BindProperty(keyProperty);
                keyAccessInfo.Add((keyProperty, null, AddToProjection2(finalProjections, keyColumn, generateAlias: false)));
            }

            foreach (var elementAccessSegment in jsonScalarExpression.Path.Where(x => x.ArrayIndex != null))
            {
                if (elementAccessSegment.ArrayIndex is SqlConstantExpression { Value: int intValue })
                {
                    keyAccessInfo.Add((null, intValue, null));
                }
                else
                {
                    keyAccessInfo.Add(
                        (null, null, AddToProjection2(finalProjections, elementAccessSegment.ArrayIndex!, generateAlias: false)));
                }
            }

            return Constant(
                new JsonProjectionInfo(
                    jsonColumnIndex,
                    keyAccessInfo));
        }

        static IReadOnlyList<IProperty> GetMappedKeyProperties(IKey key)
        {
            if (!key.DeclaringEntityType.IsMappedToJson())
            {
                return key.Properties;
            }

            // TODO: fix this once we enable json entity being owned by another owned non-json entity (issue #28441)

            // for json collections we need to filter out the ordinal key as it's not mapped to any column
            // there could be multiple of these in deeply nested structures,
            // so we traverse to the outermost owner to see how many mapped keys there are
            var currentEntity = key.DeclaringEntityType;
            while (currentEntity.IsMappedToJson())
            {
                currentEntity = currentEntity.FindOwnership()!.PrincipalEntityType;
            }

            var count = currentEntity.FindPrimaryKey()!.Properties.Count;

            return key.Properties.Take(count).ToList();
        }

        static void AppendOrdering(List<OrderingExpression> orderings, OrderingExpression ordering)
        {
            if (!orderings.Any(o => o.Expression.Equals(ordering.Expression)))
            {
                orderings.Add(ordering);
            }
        }
    }

    /// <summary>
    ///     Replaces current projection mapping with a new one to change what is being projected out from this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="projectionMapping">A new projection mapping.</param>
    public void ReplaceProjection(IReadOnlyDictionary<ProjectionMember, Expression> projectionMapping)
    {
        _projectionMapping.Clear();
        foreach (var (projectionMember, expression) in projectionMapping)
        {
            Check.DebugAssert(
                expression is SqlExpression or StructuralTypeProjectionExpression or JsonQueryExpression,
                "Invalid operation in the projection.");
            _projectionMapping[projectionMember] = expression;
        }
    }

    /// <summary>
    ///     Replaces current projection mapping with a new one to change what is being projected out from this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="clientProjections">A new projection mapping.</param>
    public void ReplaceProjection(IReadOnlyList<Expression> clientProjections)
    {
        _projectionMapping.Clear();
        _clientProjections.Clear();
        _aliasForClientProjections.Clear();
        foreach (var expression in clientProjections)
        {
            Check.DebugAssert(
                expression is SqlExpression or StructuralTypeProjectionExpression or ShapedQueryExpression or JsonQueryExpression,
                "Invalid operation in the projection.");
            _clientProjections.Add(expression);
            _aliasForClientProjections.Add(null);
        }
    }

    /// <summary>
    ///     Gets the projection mapped to the given <see cref="ProjectionBindingExpression" />.
    /// </summary>
    /// <param name="projectionBindingExpression">A projection binding to search.</param>
    /// <returns>The mapped projection for given projection binding.</returns>
    public Expression GetProjection(ProjectionBindingExpression projectionBindingExpression)
        => projectionBindingExpression.ProjectionMember is ProjectionMember projectionMember
            ? _projectionMapping[projectionMember]
            : _clientProjections[projectionBindingExpression.Index!.Value];

    /// <summary>
    ///     Adds given <see cref="SqlExpression" /> to the projection.
    /// </summary>
    /// <param name="sqlExpression">An expression to add.</param>
    /// <returns>An int value indicating the index at which the expression was added in the projection list.</returns>
    public int AddToProjection(SqlExpression sqlExpression)
        => AddToProjection(sqlExpression, null);

    private int AddToProjection(SqlExpression sqlExpression, string? alias)
    {
        var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(sqlExpression));
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        var baseAlias = !string.IsNullOrEmpty(alias)
            ? alias
            : (sqlExpression as ColumnExpression)?.Name;
        if (Alias != null)
        {
            baseAlias ??= "c";
            var counter = 0;

            var currentAlias = baseAlias;
            while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            baseAlias = currentAlias;
        }

        _projection.Add(new ProjectionExpression(sqlExpression, baseAlias ?? ""));

        return _projection.Count - 1;
    }

    private static int AddToProjection2(
        List<ProjectionExpression> projections,
        SqlExpression sqlExpression,
        bool generateAlias,
        string? alias = null)
    {
        var existingIndex = projections.FindIndex(pe => pe.Expression.Equals(sqlExpression));
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        var baseAlias = !string.IsNullOrEmpty(alias)
            ? alias
            : (sqlExpression as ColumnExpression)?.Name;
        if (generateAlias)
        {
            baseAlias ??= "c";
            var counter = 0;

            var currentAlias = baseAlias;
            while (projections.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            baseAlias = currentAlias;
        }

        projections.Add(new ProjectionExpression(sqlExpression, baseAlias ?? ""));

        return projections.Count - 1;
    }

    /// <summary>
    ///     Applies filter predicate to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="sqlExpression">An expression to use for filtering.</param>
    public void ApplyPredicate(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlConstantExpression { Value: true })
        {
            return;
        }

        if (Limit != null
            || Offset != null)
        {
            sqlExpression = PushdownIntoSubqueryInternal().Remap(sqlExpression);
        }

        if (_groupBy.Count == 0)
        {
            switch (sqlExpression)
            {
                // If the intersection is empty then we don't remove predicate so that the filter empty out all results.
                case SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal,
                        Left: ColumnExpression leftColumn,
                        Right: SqlConstantExpression { Value: string s1 }
                    }
                    when GetTable(leftColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && leftColumn.Equals(discriminatorColumn):
                {
                    var newList = discriminatorValues.Intersect(new List<string> { s1 }).ToList();
                    if (newList.Count > 0)
                    {
                        tpcExpression.DiscriminatorValues = newList;
                        return;
                    }

                    break;
                }

                case SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal,
                        Left: SqlConstantExpression { Value: string s2 },
                        Right: ColumnExpression rightColumn
                    }
                    when GetTable(rightColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && rightColumn.Equals(discriminatorColumn):
                {
                    var newList = discriminatorValues.Intersect(new List<string> { s2 }).ToList();
                    if (newList.Count > 0)
                    {
                        tpcExpression.DiscriminatorValues = newList;
                        return;
                    }

                    break;
                }

                // Identify application of a predicate which narrows the discriminator (e.g. OfType) for TPC, apply it to
                // _tpcDiscriminatorValues (which will be handled later) instead of as a WHERE predicate.
                case InExpression
                    {
                        Item: ColumnExpression itemColumn,
                        Values: IReadOnlyList<SqlExpression> valueExpressions
                    }
                    when GetTable(itemColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && itemColumn.Equals(discriminatorColumn):
                {
                    var constantValues = new string[valueExpressions.Count];
                    for (var i = 0; i < constantValues.Length; i++)
                    {
                        if (valueExpressions[i] is SqlConstantExpression { Value: string value })
                        {
                            constantValues[i] = value;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var newList = discriminatorValues.Intersect(constantValues).ToList();
                    if (newList.Count > 0)
                    {
                        tpcExpression.DiscriminatorValues = newList;
                        return;
                    }

                    break;
                }
            }
        }

        if (_groupBy.Count > 0)
        {
            Having = Having == null
                ? sqlExpression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Having,
                    sqlExpression,
                    typeof(bool),
                    sqlExpression.TypeMapping);
        }
        else
        {
            Predicate = Predicate == null
                ? sqlExpression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Predicate,
                    sqlExpression,
                    typeof(bool),
                    sqlExpression.TypeMapping);
        }
    }

    /// <summary>
    ///     Applies filter predicate to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="sqlExpression">An expression to use for filtering.</param>
    [Pure]
    public SelectExpression ApplyPredicate2(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlConstantExpression { Value: true })
        {
            return this;
        }

        var select = this;

        if (Limit is not null || Offset is not null)
        {
            (select, var remapper) = PushdownIntoSubqueryInternal2();
            sqlExpression = remapper.Remap(sqlExpression, out var subquery);
            select = select.Update(
                select.Projection,
                [subquery],
                select.Predicate,
                select.GroupBy,
                select.Having,
                select.Orderings,
                select.Limit,
                select.Offset);
        }

        if (GroupBy.Count == 0)
        {
            switch (sqlExpression)
            {
                // If the intersection is empty then we don't remove predicate so that the filter empty out all results.
                case SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal,
                        Left: ColumnExpression leftColumn,
                        Right: SqlConstantExpression { Value: string s1 }
                    }
                    when select.GetTable(leftColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && leftColumn.Equals(discriminatorColumn):
                {
                    var newList = discriminatorValues.Intersect(new List<string> { s1 }).ToList();
                    if (newList.Count > 0)
                    {
                        // TODO: Stop mutating
                        tpcExpression.DiscriminatorValues = newList;
                        return this;
                    }

                    break;
                }

                case SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal,
                        Left: SqlConstantExpression { Value: string s2 },
                        Right: ColumnExpression rightColumn
                    }
                    when select.GetTable(rightColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && rightColumn.Equals(discriminatorColumn):
                {
                    var newList = discriminatorValues.Intersect(new List<string> { s2 }).ToList();
                    if (newList.Count > 0)
                    {
                        // TODO: Stop mutating
                        tpcExpression.DiscriminatorValues = newList;
                        return this;
                    }

                    break;
                }

                // Identify application of a predicate which narrows the discriminator (e.g. OfType) for TPC, apply it to
                // _tpcDiscriminatorValues (which will be handled later) instead of as a WHERE predicate.
                case InExpression
                    {
                        Item: ColumnExpression itemColumn,
                        Values: IReadOnlyList<SqlExpression> valueExpressions
                    }
                    when select.GetTable(itemColumn) is TpcTablesExpression
                    {
                        DiscriminatorColumn: var discriminatorColumn,
                        DiscriminatorValues: var discriminatorValues
                    } tpcExpression
                    && itemColumn.Equals(discriminatorColumn):
                {
                    var constantValues = new string[valueExpressions.Count];
                    for (var i = 0; i < constantValues.Length; i++)
                    {
                        if (valueExpressions[i] is SqlConstantExpression { Value: string value })
                        {
                            constantValues[i] = value;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var newList = discriminatorValues.Intersect(constantValues).ToList();
                    if (newList.Count > 0)
                    {
                        // TODO: Stop mutating
                        tpcExpression.DiscriminatorValues = newList;
                        return this;
                    }

                    break;
                }
            }

            return select.Update(
                select.Projection,
                select.Tables,
                select.Predicate == null
                    ? sqlExpression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        select.Predicate,
                        sqlExpression,
                        typeof(bool),
                        sqlExpression.TypeMapping),
                groupBy: [],
                having: null,
                select.Orderings,
                select.Limit,
                select.Offset);
        }

        // There's a GroupBy, so apply the new predicate to the HAVING clause.
        return select.Update(
            select.Projection,
            select.Tables,
            select.Predicate,
            select.GroupBy,
            select.Having == null
                ? sqlExpression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    select.Having,
                    sqlExpression,
                    typeof(bool),
                    sqlExpression.TypeMapping),
            select.Orderings,
            select.Limit,
            select.Offset);
    }

    /// <summary>
    ///     Applies grouping from given key selector and generate <see cref="RelationalGroupByShaperExpression" /> to shape results.
    /// </summary>
    /// <param name="keySelector">An key selector expression for the GROUP BY.</param>
    /// <param name="shaperExpression">The shaper expression for current query.</param>
    /// <param name="sqlExpressionFactory">The sql expression factory to use.</param>
    /// <returns>A <see cref="RelationalGroupByShaperExpression" /> which represents the result of the grouping operation.</returns>
    public ShapedQueryExpression ApplyGrouping(
        Expression keySelector,
        Expression shaperExpression,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        var select = WithOrderings([]);

        var keySelectorToAdd = keySelector;
        var emptyKey = keySelector is NewExpression { Arguments.Count: 0 };
        if (emptyKey)
        {
            keySelectorToAdd = sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(1));
        }

        var groupByTerms = new List<SqlExpression>();
        var groupByAliases = new List<string?>();
        PopulateGroupByTerms(keySelectorToAdd, groupByTerms, groupByAliases, "Key");

        if (groupByTerms.Any(e => e is not ColumnExpression))
        {
            // emptyKey will always hit this path.
            (select, var sqlRemappingVisitor) = select.PushdownIntoSubqueryInternal2();
            var newGroupByTerms = new List<SqlExpression>(groupByTerms.Count);
            var subquery = (SelectExpression)select.Tables[0];
            for (var i = 0; i < groupByTerms.Count; i++)
            {
                var item = groupByTerms[i];

                SqlExpression newItem;
                if (subquery._projection.Any(e => e.Expression.Equals(item)))
                {
                    newItem = sqlRemappingVisitor.Remap(item, out subquery);
                }
                else
                {
                    newItem = subquery.GenerateOuterColumn2(subquery.Alias!, item, out subquery, groupByAliases[i] ?? "Key");
                    sqlRemappingVisitor.Subquery = subquery;
                }

                select = select.Update(
                    select.Projection,
                    [subquery],
                    select.Predicate,
                    select.GroupBy,
                    select.Having,
                    select.Orderings,
                    select.Limit,
                    select.Offset);
                newGroupByTerms.Add(newItem);
            }

            if (!emptyKey)
            {
                // If non-empty key then we need to regenerate the key selector
                keySelector = new ReplacingExpressionVisitor(groupByTerms, newGroupByTerms).Visit(keySelector);
            }

            groupByTerms = newGroupByTerms;
        }

        select = select.Update(
            select.Projection,
            select.Tables,
            select.Predicate,
            groupBy: [..select.GroupBy, ..groupByTerms],
            select.Having,
            select.Orderings,
            select.Limit,
            select.Offset);

        var clonedSelectExpression = select.Clone(); // TODO: Remove
        var correlationPredicate = groupByTerms.Zip(clonedSelectExpression._groupBy)
            .Select(e => sqlExpressionFactory.Equal(e.First, e.Second))
            .Aggregate(sqlExpressionFactory.AndAlso);

        clonedSelectExpression = clonedSelectExpression
            .Update(
                clonedSelectExpression.Projection,
                clonedSelectExpression.Tables,
                clonedSelectExpression.Predicate,
                groupBy: [],
                clonedSelectExpression.Having,
                clonedSelectExpression.Orderings,
                clonedSelectExpression.Limit,
                clonedSelectExpression.Offset)
            .ApplyPredicate2(correlationPredicate);

        // TODO: Remove mutability
        if (!select._identifier.All(e => select._groupBy.Contains(e.Column)))
        {
            select._preGroupByIdentifier = select._identifier.ToList();
            select._identifier.Clear();
            if (select._groupBy.All(e => e is ColumnExpression))
            {
                select._identifier.AddRange(select._groupBy.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
            }
        }

        return new ShapedQueryExpression(
            select,
            new RelationalGroupByShaperExpression(
                keySelector,
                new QueryExpressionReplacingExpressionVisitor(this, select).Visit(shaperExpression),
                new ShapedQueryExpression(
                    clonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(this, clonedSelectExpression).Visit(shaperExpression))));
    }

    private static void PopulateGroupByTerms(
        Expression keySelector,
        List<SqlExpression> groupByTerms,
        List<string?> groupByAliases,
        string? name)
    {
        switch (keySelector)
        {
            case SqlExpression sqlExpression:
                groupByTerms.Add(sqlExpression);
                groupByAliases.Add(name);
                break;

            case NewExpression newExpression:
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    PopulateGroupByTerms(newExpression.Arguments[i], groupByTerms, groupByAliases, newExpression.Members?[i].Name);
                }

                break;

            case MemberInitExpression memberInitExpression:
                PopulateGroupByTerms(memberInitExpression.NewExpression, groupByTerms, groupByAliases, null);
                foreach (var argument in memberInitExpression.Bindings)
                {
                    var memberAssignment = (MemberAssignment)argument;
                    PopulateGroupByTerms(memberAssignment.Expression, groupByTerms, groupByAliases, memberAssignment.Member.Name);
                }

                break;

            case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression:
                PopulateGroupByTerms(unaryExpression.Operand, groupByTerms, groupByAliases, name);
                break;

            case StructuralTypeShaperExpression { ValueBufferExpression: StructuralTypeProjectionExpression projection }:
                foreach (var property in projection.StructuralType.GetAllPropertiesInHierarchy())
                {
                    PopulateGroupByTerms(projection.BindProperty(property), groupByTerms, groupByAliases, name: null);
                }

                if (projection.DiscriminatorExpression != null)
                {
                    PopulateGroupByTerms(
                        projection.DiscriminatorExpression, groupByTerms, groupByAliases, name: DiscriminatorColumnAlias);
                }

                break;

            default:
                throw new InvalidOperationException(RelationalStrings.InvalidKeySelectorForGroupBy(keySelector, keySelector.GetType()));
        }
    }

    /// <summary>
    ///     Applies ordering to the <see cref="SelectExpression" />. This overwrites any previous ordering specified.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    [Pure]
    public SelectExpression ApplyOrdering(OrderingExpression orderingExpression)
    {
        var select = this;

        if (IsDistinct || Limit is not null || Offset is not null)
        {
            (select, var remapper) = PushdownIntoSubqueryInternal2();
            var expression = remapper.Remap(orderingExpression.Expression, out var subquery);
            select = select.Update(
                select.Projection,
                [subquery],
                select.Predicate,
                select.GroupBy,
                select.Having,
                select.Orderings,
                select.Limit,
                select.Offset);
            orderingExpression = orderingExpression.Update(expression);
        }

        return select.WithOrderings([orderingExpression]);
    }

    /// <summary>
    ///     Appends ordering to the existing orderings of the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    public void AppendOrdering(OrderingExpression orderingExpression)
    {
        if (!_orderings.Any(o => o.Expression.Equals(orderingExpression.Expression)))
        {
            AppendOrderingInternal(orderingExpression);
        }
    }

    private void AppendOrderingInternal(OrderingExpression orderingExpression)
        => _orderings.Add(orderingExpression.Update(orderingExpression.Expression));

    /// <summary>
    ///     Reverses the existing orderings on the <see cref="SelectExpression" />.
    /// </summary>
    [Pure]
    public SelectExpression ReverseOrderings()
    {
        var select = this;

        if (Limit != null || Offset != null)
        {
            select = PushdownIntoSubquery2();
        }

        var reversedOrderings = new OrderingExpression[select.Orderings.Count];
        for (var i = 0; i < reversedOrderings.Length; i++)
        {
            var ordering = select.Orderings[i];
            reversedOrderings[i] = new OrderingExpression(ordering.Expression, !ordering.IsAscending);
        }

        return select.WithOrderings(reversedOrderings);
    }

    /// <summary>
    ///     Clears existing orderings.
    /// </summary>
    public void ClearOrdering()
        => _orderings.Clear();

    /// <summary>
    ///     Applies limit to the <see cref="SelectExpression" /> to limit the number of rows returned in the result set.
    /// </summary>
    /// <param name="sqlExpression">An expression representing limit row count.</param>
    public void ApplyLimit(SqlExpression sqlExpression)
    {
        if (Limit != null)
        {
            PushdownIntoSubquery();
        }

        Limit = sqlExpression;
    }

    /// <summary>
    ///     Applies limit to the <see cref="SelectExpression" /> to limit the number of rows returned in the result set.
    /// </summary>
    /// <param name="limit">An expression representing limit row count.</param>
    [Pure]
    public SelectExpression ApplyLimit2(SqlExpression limit)
    {
        var select = this;

        if (Limit != null)
        {
            select = PushdownIntoSubquery2();
        }

        return select.Update(
            select.Projection, select.Tables, select.Predicate, select.GroupBy, select.Having, select.Orderings, limit, select.Offset);
    }

    /// <summary>
    ///     Applies offset to the <see cref="SelectExpression" /> to skip the number of rows in the result set.
    /// </summary>
    /// <param name="sqlExpression">An expression representing offset row count.</param>
    public void ApplyOffset(SqlExpression sqlExpression)
    {
        if (Limit != null
            || Offset != null
            || (IsDistinct && Orderings.Count == 0))
        {
            PushdownIntoSubquery();
        }

        Offset = sqlExpression;
    }

    /// <summary>
    ///     Applies offset to the <see cref="SelectExpression" /> to skip the number of rows in the result set.
    /// </summary>
    /// <param name="offset">An expression representing offset row count.</param>
    [Pure]
    public SelectExpression ApplyOffset2(SqlExpression offset)
    {
        var select = this;

        if (Limit != null
            || Offset != null
            || (IsDistinct && Orderings.Count == 0))
        {
            select = PushdownIntoSubquery2();
        }

        return select.Update(
            select.Projection, select.Tables, select.Predicate, select.GroupBy, select.Having, select.Orderings, select.Limit, offset);
    }

    private enum SetOperationType
    {
        Except,
        Intersect,
        Union
    }

    /// <summary>
    ///     Applies EXCEPT operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    [Pure]
    public SelectExpression ApplyExcept(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Except, source2, distinct);

    /// <summary>
    ///     Applies INTERSECT operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    [Pure]
    public SelectExpression ApplyIntersect(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Intersect, source2, distinct);

    /// <summary>
    ///     Applies UNION operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    [Pure]
    public SelectExpression ApplyUnion(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Union, source2, distinct);

    [Pure]
    private SelectExpression ApplySetOperation(
        SetOperationType setOperationType,
        SelectExpression select2,
        bool distinct)
    {
        var select1 = this;

        var outerIdentifierColumns = select1._identifier.Count == select2._identifier.Count
            ? new ColumnExpression?[select1._identifier.Count]
            : Array.Empty<ColumnExpression?>();
        var entityProjectionIdentifiers = new List<ColumnExpression>();
        var entityProjectionValueComparers = new List<ValueComparer>();
        var otherExpressions = new List<(SqlExpression Expression, ValueComparer Comparer)>();

        // Push down into a subquery if limit/offset are defined. If not, any orderings can be discarded as set operations don't preserve
        // them.
        // Note that in some databases it may be possible to preserve the internal ordering of the set operands for Concat, but we don't
        // currently support that.
        if (select1.Limit != null || select1.Offset != null)
        {
            select1 = select1.PushdownIntoSubqueryInternal2(liftOrderings: false).Item1;
        }
        else
        {
            select1 = select1.WithOrderings([]);
        }

        // Do the same for the other side of the set operation
        if (select2.Limit != null || select2.Offset != null)
        {
            select2 = select2.PushdownIntoSubqueryInternal2(liftOrderings: false).Item1;
        }
        else
        {
            select2 = select2.WithOrderings([]);
        }

        if (select1._clientProjections.Count > 0
            || select2._clientProjections.Count > 0)
        {
            throw new InvalidOperationException(RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation);
        }

        if (select1._projectionMapping.Count != select2._projectionMapping.Count)
        {
            // For DTO each side can have different projection mapping if some columns are not present.
            // We need to project null for missing columns.
            throw new InvalidOperationException(RelationalStrings.ProjectionMappingCountMismatch);
        }

        var setOperationAlias = _sqlAliasManager.GenerateTableAlias(setOperationType.ToString());
        var outerProjectionMappings = new Dictionary<ProjectionMember, Expression>();
        var (projections1, projections2) = (new List<ProjectionExpression>(), new List<ProjectionExpression>());
        foreach (var (projectionMember, expression1, expression2) in select1._projectionMapping.Join(
                     select2._projectionMapping,
                     kv => kv.Key,
                     kv => kv.Key,
                     (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
        {
            if (expression1 is StructuralTypeProjectionExpression projection1
                && expression2 is StructuralTypeProjectionExpression projection2)
            {
                HandleStructuralTypeProjection(projectionMember, select1, projection1, select2, projection2);
                continue;
            }

            var innerColumn1 = (SqlExpression)expression1;
            var innerColumn2 = (SqlExpression)expression2;

            var projectionAlias = GenerateUniqueColumnAlias(
                projectionMember.Last?.Name
                ?? (innerColumn1 as ColumnExpression)?.Name
                ?? "c");

            var innerProjection1 = new ProjectionExpression(innerColumn1, projectionAlias);
            var innerProjection2 = new ProjectionExpression(innerColumn2, projectionAlias);
            projections1.Add(innerProjection1);
            projections2.Add(innerProjection2);
            var outerProjection = CreateColumnExpression(innerProjection1, setOperationAlias);

            if (IsNullableProjection(innerProjection1)
                || IsNullableProjection(innerProjection2))
            {
                outerProjection = outerProjection.MakeNullable();
            }

            outerProjectionMappings[projectionMember] = outerProjection;

            if (outerIdentifierColumns.Length > 0)
            {
                // If we happen to project identifier columns, make them candidates for lifting up to be the outer identifiers for the
                // set operation result. Note that we check below that *all* identifier columns are projected out, since a partial
                // identifier (e.g. one column in a composite key) is insufficient.
                var index = select1._identifier.FindIndex(e => e.Column.Equals(expression1));
                if (index != -1)
                {
                    if (select2._identifier[index].Column.Equals(expression2))
                    {
                        outerIdentifierColumns[index] = outerProjection;
                    }
                    else
                    {
                        // If select1 matched but select2 did not then we erase all identifiers
                        // TODO: We could make this little more robust by allow the indexes to be different. See issue#24475
                        // i.e. Identifier ordering being different.
                        outerIdentifierColumns = [];
                    }
                }

                // we need comparer (that we get from type mapping) for identifiers
                // it may happen that one side of the set operation comes from collection parameter
                // and therefore doesn't have type mapping (yet - we infer those after the translation is complete)
                // but for set operation at least one side should have type mapping, otherwise whole thing would have been parameterized out
                // this can only happen in compiled query, since we always parameterize parameters there - if this happens we throw
                var outerTypeMapping = innerProjection1.Expression.TypeMapping ?? innerProjection2.Expression.TypeMapping;
                if (outerTypeMapping == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping(setOperationType));
                }

                otherExpressions.Add((outerProjection, outerTypeMapping.KeyComparer));
            }
        }

        select1 = select1.WithProjections(projections1);
        select2 = select2.WithProjections(projections2);
        select1._projectionMapping.Clear();
        select2._projectionMapping.Clear();
        select1.IsMutable = false;
        select2.IsMutable = false;

        // We generate actual set operation after applying projection to lift group by aggregate
        var setExpression = setOperationType switch
        {
            SetOperationType.Except => (SetOperationBase)new ExceptExpression(setOperationAlias, select1, select2, distinct),
            SetOperationType.Intersect => new IntersectExpression(setOperationAlias, select1, select2, distinct),
            SetOperationType.Union => new UnionExpression(setOperationAlias, select1, select2, distinct),
            _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(setOperationType), setOperationType))
        };

        // We should apply _identifiers only when it is distinct and actual select expression had identifiers.
        var outerIdentifiers = new List<(ColumnExpression Column, ValueComparer Comparer)>();
        if (distinct
            && outerIdentifierColumns.Length > 0)
        {
            // If we find matching identifier in outer level then we just use them.
            if (outerIdentifierColumns.All(e => e != null))
            {
                outerIdentifiers.AddRange(outerIdentifierColumns.Zip(select1._identifier, (c, i) => (c!, i.Comparer)));
            }
            else
            {
                outerIdentifiers.Clear();
                if (otherExpressions.Count == 0)
                {
                    // If there are no other expressions then we can use all entityProjectionIdentifiers
                    outerIdentifiers.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                }
                else if (otherExpressions.All(e => e.Expression is ColumnExpression))
                {
                    outerIdentifiers.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                    outerIdentifiers.AddRange(otherExpressions.Select(e => ((ColumnExpression)e.Expression, e.Comparer)));
                }
            }
        }

        return new SelectExpression(
            alias: null, [setExpression], groupBy: [], projections: [], orderings: [], annotations: null, _sqlAliasManager)
        {
            _projectionMapping = outerProjectionMappings,
            _identifier = outerIdentifiers
        };

        void HandleStructuralTypeProjection(
            ProjectionMember projectionMember,
            SelectExpression select1,
            StructuralTypeProjectionExpression projection1,
            SelectExpression select2,
            StructuralTypeProjectionExpression projection2)
        {
            if (projection1.StructuralType != projection2.StructuralType)
            {
                throw new InvalidOperationException(
                    RelationalStrings.SetOperationOverDifferentStructuralTypes(
                        projection1.StructuralType.DisplayName(), projection2.StructuralType.DisplayName()));
            }

            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

            ProcessStructuralType(projection1, projection2);

            void ProcessStructuralType(
                StructuralTypeProjectionExpression nestedProjection1,
                StructuralTypeProjectionExpression nestedProjection2)
            {
                var type = nestedProjection1.StructuralType;

                foreach (var property in type.GetAllPropertiesInHierarchy())
                {
                    var column1 = nestedProjection1.BindProperty(property);
                    var column2 = nestedProjection2.BindProperty(property);
                    var alias = GenerateUniqueColumnAlias(column1.Name);
                    var innerProjection = new ProjectionExpression(column1, alias);
                    projections1.Add(innerProjection);
                    projections2.Add(new ProjectionExpression(column2, alias));
                    var outerColumn = CreateColumnExpression(innerProjection, setOperationAlias);
                    if (column1.IsNullable
                        || column2.IsNullable)
                    {
                        outerColumn = outerColumn.MakeNullable();
                    }

                    propertyExpressions[property] = outerColumn;

                    // Lift up any identifier columns to the set operation result (the outer).
                    // This is typically the entity primary key columns, but can also be all of a complex type's properties if Distinct
                    // was previously called.
                    if (outerIdentifierColumns.Length > 0)
                    {
                        var index = select1._identifier.FindIndex(e => e.Column.Equals(column1));
                        if (index != -1)
                        {
                            if (select2._identifier[index].Column.Equals(column2))
                            {
                                outerIdentifierColumns[index] = outerColumn;
                            }
                            else
                            {
                                // If select1 matched but select2 did not then we erase all identifiers
                                // TODO: We could make this little more robust by allow the indexes to be different. See issue#24475
                                // i.e. Identifier ordering being different.
                                outerIdentifierColumns = [];
                            }
                        }
                        // If the top-level projection - not the current nested one - is a complex type and not an entity type, then add
                        // all its columns to the "otherExpressions" list (i.e. columns not part of a an entity primary key). This is
                        // the same as with a non-structural type projection.
                        else if (projection1.StructuralType is IComplexType)
                        {
                            var outerTypeMapping = column1.TypeMapping ?? column1.TypeMapping;
                            if (outerTypeMapping == null)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping(setOperationType));
                            }

                            otherExpressions.Add((outerColumn, outerTypeMapping.KeyComparer));
                        }
                    }
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(nestedProjection1.StructuralType))
                {
                    ProcessStructuralType(
                        (StructuralTypeProjectionExpression)nestedProjection1.BindComplexProperty(complexProperty).ValueBufferExpression,
                        (StructuralTypeProjectionExpression)nestedProjection2.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            Check.DebugAssert(
                projection1.TableMap.Count == projection2.TableMap.Count,
                "Set operation over entity projections with different table map counts");
            Check.DebugAssert(
                projection1.TableMap.Keys.All(t => projection2.TableMap.ContainsKey(t)),
                "Set operation over entity projections with table map discrepancy");

            var tableMap = projection1.TableMap.ToDictionary(kvp => kvp.Key, _ => setOperationAlias);

            var discriminatorExpression = projection1.DiscriminatorExpression;
            if (projection1.DiscriminatorExpression != null
                && projection2.DiscriminatorExpression != null)
            {
                var alias = GenerateUniqueColumnAlias(DiscriminatorColumnAlias);
                var innerProjection = new ProjectionExpression(projection1.DiscriminatorExpression, alias);
                projections1.Add(innerProjection);
                projections2.Add(new ProjectionExpression(projection2.DiscriminatorExpression, alias));
                discriminatorExpression = CreateColumnExpression(innerProjection, setOperationAlias);
            }

            var outerProjection = new StructuralTypeProjectionExpression(
                projection1.StructuralType, propertyExpressions, tableMap, nullable: false, discriminatorExpression);

            if (outerIdentifierColumns.Length > 0 && outerProjection is { StructuralType: IEntityType entityType })
            {
                var primaryKey = entityType.FindPrimaryKey();

                // We know that there are existing identifiers (see condition above); we know we must have a key since a keyless
                // entity type would have wiped the identifiers when generating the join.
                Check.DebugAssert(primaryKey != null, "primary key is null.");
                foreach (var property in primaryKey.Properties)
                {
                    entityProjectionIdentifiers.Add(outerProjection.BindProperty(property));
                    entityProjectionValueComparers.Add(property.GetKeyValueComparer());
                }
            }

            outerProjectionMappings[projectionMember] = outerProjection;
        }

        string GenerateUniqueColumnAlias(string baseAlias)
        {
            var currentAlias = baseAlias;
            var counter = 0;
            while (projections1.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            return currentAlias;
        }

        static bool IsNullableProjection(ProjectionExpression projectionExpression)
            => projectionExpression.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                _ => true
            };
    }

    /// <summary>
    ///     Applies <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource})" /> on the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="sqlExpressionFactory">A factory to use for generating required sql expressions.</param>
    public void ApplyDefaultIfEmpty(ISqlExpressionFactory sqlExpressionFactory)
    {
        var nullSqlExpression = sqlExpressionFactory.ApplyDefaultTypeMapping(
            new SqlConstantExpression(Constant(null, typeof(string)), null));

        var dummySelectExpression = CreateImmutable(
            _sqlAliasManager.GenerateTableAlias("empty"),
            tables: [],
            [new ProjectionExpression(nullSqlExpression, "empty")]);

        if (Orderings.Any()
            || Limit != null
            || Offset != null
            || IsDistinct
            || Predicate != null
            || Tables.Count > 1
            || GroupBy.Count > 0)
        {
            PushdownIntoSubquery();
        }

        var joinPredicate = sqlExpressionFactory.Equal(sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1));
        var joinTable = new LeftJoinExpression(Tables.Single(), joinPredicate);
        _tables.Clear();
        _tables.Add(dummySelectExpression);
        _tables.Add(joinTable);

        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var projection in _projectionMapping)
        {
            var projectionToAdd = projection.Value;
            if (projectionToAdd is StructuralTypeProjectionExpression typeProjection)
            {
                projectionToAdd = typeProjection.MakeNullable();
            }
            else if (projectionToAdd is ColumnExpression column)
            {
                projectionToAdd = column.MakeNullable();
            }

            projectionMapping[projection.Key] = projectionToAdd;
        }

        // ChildIdentifiers shouldn't be required to be updated since during translation they should be empty.
        for (var i = 0; i < _identifier.Count; i++)
        {
            if (_identifier[i].Column is ColumnExpression column)
            {
                _identifier[i] = (column.MakeNullable(), _identifier[i].Comparer);
            }
        }

        _projectionMapping = projectionMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public StructuralTypeShaperExpression GenerateOwnedReferenceEntityProjectionExpression(
        StructuralTypeProjectionExpression principalEntityProjection,
        INavigation navigation,
        ISqlExpressionFactory sqlExpressionFactory,
        SqlAliasManager sqlAliasManager)
    {
        // We first find the select expression where principal tableExpressionBase is located
        // That is where we find shared tableExpressionBase to pull columns from or add joins
        var identifyingColumn = principalEntityProjection.BindProperty(
            navigation.DeclaringEntityType.FindPrimaryKey()!.Properties.First());

        var expressions = GetPropertyExpressions(sqlExpressionFactory, sqlAliasManager, navigation, this, identifyingColumn);

        var entityShaper = new RelationalStructuralTypeShaperExpression(
            navigation.TargetEntityType,
            new StructuralTypeProjectionExpression(navigation.TargetEntityType, expressions, principalEntityProjection.TableMap),
            identifyingColumn.IsNullable || navigation.DeclaringEntityType.BaseType != null || !navigation.ForeignKey.IsRequiredDependent);
        principalEntityProjection.AddNavigationBinding(navigation, entityShaper);

        return entityShaper;

        // Owned types don't support inheritance See https://github.com/dotnet/efcore/issues/9630
        // So there is no handling for dependent having hierarchy
        // TODO: The following code should also handle Function and SqlQuery mappings when supported on owned type
        static IReadOnlyDictionary<IProperty, ColumnExpression> GetPropertyExpressions(
            ISqlExpressionFactory sqlExpressionFactory,
            SqlAliasManager sqlAliasManager,
            INavigation navigation,
            SelectExpression selectExpression,
            ColumnExpression identifyingColumn)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            var tableExpressionBase = selectExpression.GetTable(identifyingColumn).UnwrapJoin();
            var tableAlias = tableExpressionBase.GetRequiredAlias();
            if (tableExpressionBase is SelectExpression subquery)
            {
                // If identifying column is from a subquery then the owner table is inside subquery
                // so we need to traverse in
                var subqueryIdentifyingColumn = (ColumnExpression)subquery.Projection
                    .Single(e => string.Equals(e.Alias, identifyingColumn.Name, StringComparison.OrdinalIgnoreCase))
                    .Expression;

                var subqueryPropertyExpressions = GetPropertyExpressions(
                    sqlExpressionFactory, sqlAliasManager, navigation, subquery, subqueryIdentifyingColumn);
                var changeNullability = identifyingColumn.IsNullable && !subqueryIdentifyingColumn.IsNullable;
                foreach (var (property, columnExpression) in subqueryPropertyExpressions)
                {
                    var outerColumn = subquery.GenerateOuterColumn(tableAlias, columnExpression);
                    if (changeNullability)
                    {
                        outerColumn = outerColumn.MakeNullable();
                    }

                    propertyExpressions[property] = outerColumn;
                }

                return propertyExpressions;
            }

            // This is the select expression where owner table exists
            // where we would look for same table or generate joins
            var sourceTableForAnnotations = FindRootTableExpressionForColumn(selectExpression, identifyingColumn);
            var ownerType = navigation.DeclaringEntityType;
            var entityType = navigation.TargetEntityType;
            var principalMappings = ownerType.GetViewOrTableMappings().Select(e => e.Table);
            var derivedType = ownerType.BaseType != null;
            var derivedTpt = derivedType && ownerType.GetMappingStrategy() == RelationalAnnotationNames.TptMappingStrategy;
            var parentNullable = identifyingColumn.IsNullable;
            var pkColumnsNullable = parentNullable
                || (derivedType && ownerType.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy);
            var newColumnsNullable = pkColumnsNullable
                || !navigation.ForeignKey.IsRequiredDependent
                || derivedType;
            if (derivedTpt)
            {
                principalMappings = principalMappings.Except(ownerType.BaseType!.GetViewOrTableMappings().Select(e => e.Table));
            }

            var principalTables = principalMappings.ToList();
            var dependentTables = entityType.GetViewOrTableMappings().Select(e => e.Table).ToList();
            var baseTableIndex = selectExpression._tables.FindIndex(teb => ReferenceEquals(teb.UnwrapJoin(), tableExpressionBase));
            var dependentMainTable = dependentTables[0];
            var tableMap = new Dictionary<ITableBase, string>();
            var keyProperties = entityType.FindPrimaryKey()!.Properties;
            if (tableExpressionBase is TableExpression)
            {
                // This has potential to pull data from existing table
                // PrincipalTables count will be 1 except for entity splitting
                var matchingTableIndex = principalTables.FindIndex(e => e == dependentMainTable);
                // If dependent main table is not sharing then there is no table sharing at all in fragment
                if (matchingTableIndex != -1)
                {
                    // Dependent is table sharing with principal in some form, we don't need to generate join to owner
                    // TableExpression from identifying column will point to base type for TPT
                    // This may not be table which originates Owned type
                    if (derivedTpt)
                    {
                        baseTableIndex = selectExpression._tables.FindIndex(
                            teb => ((TableExpression)teb.UnwrapJoin()).Table == principalTables[0]);
                    }

                    var tableIndex = baseTableIndex + matchingTableIndex;
                    var mainTableAlias = selectExpression.Tables[tableIndex].GetRequiredAlias();
                    tableMap[dependentMainTable] = mainTableAlias;
                    if (dependentTables.Count > 1)
                    {
                        var joinColumns = new List<ColumnExpression>();
                        foreach (var property in keyProperties)
                        {
                            var columnExpression = CreateColumnExpression(
                                property, dependentMainTable.FindColumn(property)!, mainTableAlias, pkColumnsNullable);
                            propertyExpressions[property] = columnExpression;
                            joinColumns.Add(columnExpression);
                        }

                        for (var i = 1; i < dependentTables.Count; i++)
                        {
                            var table = dependentTables[i];
                            matchingTableIndex = principalTables.FindIndex(e => e == table);
                            if (matchingTableIndex != -1)
                            {
                                // We don't need to generate join for this
                                tableMap[table] = selectExpression.Tables[baseTableIndex + matchingTableIndex].GetRequiredAlias();
                            }
                            else
                            {
                                var alias = sqlAliasManager.GenerateTableAlias(table);
                                TableExpressionBase tableExpression = new TableExpression(alias, table);
                                foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
                                {
                                    tableExpression = tableExpression.AddAnnotation(annotation.Name, annotation.Value);
                                }

                                tableMap[table] = alias;

                                var innerColumns = keyProperties.Select(
                                    p => CreateColumnExpression(p, table, alias, nullable: false));
                                var joinPredicate = joinColumns
                                    .Zip(innerColumns, sqlExpressionFactory.Equal)
                                    .Aggregate(sqlExpressionFactory.AndAlso);

                                selectExpression._tables.Add(new LeftJoinExpression(tableExpression, joinPredicate, prunable: true));
                            }
                        }
                    }

                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.IsPrimaryKey()
                            && dependentTables.Count > 1)
                        {
                            continue;
                        }

                        var columnBase = dependentTables.Count == 1
                            ? dependentMainTable.FindColumn(property)!
                            : dependentTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                        propertyExpressions[property] = CreateColumnExpression(
                            property, columnBase, tableMap[columnBase.Table],
                            nullable: property.IsPrimaryKey() ? pkColumnsNullable : newColumnsNullable);
                    }

                    return propertyExpressions;
                }
            }

            // Either we encountered a custom table source or dependent is not sharing table
            // In either case we need to generate join to owner
            var ownerJoinColumns = new List<ColumnExpression>();
            foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
            {
                var columnBase = principalTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                var columnExpression = CreateColumnExpression(property, columnBase, tableAlias, pkColumnsNullable);
                ownerJoinColumns.Add(columnExpression);
            }

            var ownedTableAlias = sqlAliasManager.GenerateTableAlias(dependentMainTable);
            TableExpressionBase ownedTable = new TableExpression(ownedTableAlias, dependentMainTable);
            foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
            {
                ownedTable = ownedTable.AddAnnotation(annotation.Name, annotation.Value);
            }

            var outerJoinPredicate = ownerJoinColumns
                .Zip(
                    navigation.ForeignKey.Properties
                        .Select(p => CreateColumnExpression(p, dependentMainTable, ownedTableAlias, nullable: false)))
                .Select(i => sqlExpressionFactory.Equal(i.First, i.Second))
                .Aggregate(sqlExpressionFactory.AndAlso);
            selectExpression._tables.Add(new LeftJoinExpression(ownedTable, outerJoinPredicate));
            tableMap[dependentMainTable] = ownedTableAlias;
            if (dependentTables.Count > 1)
            {
                var joinColumns = new List<ColumnExpression>();
                foreach (var property in keyProperties)
                {
                    var columnExpression = CreateColumnExpression(
                        property, dependentMainTable.FindColumn(property)!, ownedTableAlias, newColumnsNullable);
                    propertyExpressions[property] = columnExpression;
                    joinColumns.Add(columnExpression);
                }

                for (var i = 1; i < dependentTables.Count; i++)
                {
                    var table = dependentTables[i];
                    var alias = sqlAliasManager.GenerateTableAlias(table);
                    TableExpressionBase tableExpression = new TableExpression(alias, table);
                    foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
                    {
                        tableExpression = tableExpression.AddAnnotation(annotation.Name, annotation.Value);
                    }

                    tableMap[table] = alias;

                    var innerColumns = keyProperties.Select(
                        p => CreateColumnExpression(p, table, alias, nullable: false));
                    var joinPredicate = joinColumns
                        .Zip(innerColumns, sqlExpressionFactory.Equal)
                        .Aggregate(sqlExpressionFactory.AndAlso);

                    selectExpression._tables.Add(new LeftJoinExpression(tableExpression, joinPredicate, prunable: true));
                }
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.IsPrimaryKey()
                    && dependentTables.Count > 1)
                {
                    continue;
                }

                var columnBase = dependentTables.Count == 1
                    ? dependentMainTable.FindColumn(property)!
                    : dependentTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                propertyExpressions[property] = CreateColumnExpression(
                    property, columnBase, tableMap[columnBase.Table],
                    nullable: newColumnsNullable);
            }

            foreach (var property in keyProperties)
            {
                selectExpression._identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
            }

            return propertyExpressions;
        }

        static TableExpressionBase FindRootTableExpressionForColumn(SelectExpression select, ColumnExpression column)
        {
            var table = select.GetTable(column).UnwrapJoin();

            if (table is SetOperationBase setOperationBase)
            {
                table = setOperationBase.Source1;
            }

            if (table is SelectExpression innerSelect)
            {
                var matchingProjection = (ColumnExpression)innerSelect.Projection.Single(p => p.Alias == column.Name).Expression;

                return FindRootTableExpressionForColumn(innerSelect, matchingProjection);
            }

            return table;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static StructuralTypeShaperExpression GenerateComplexPropertyShaperExpression(
        StructuralTypeProjectionExpression containerProjection,
        IComplexProperty complexProperty)
    {
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();

        // We do not support complex type splitting, so we will only ever have a single table/view mapping to it.
        // See Issue #32853 and Issue #31248
        var complexTypeTable = complexProperty.ComplexType.GetViewOrTableMappings().Single().Table;
        if (!containerProjection.TableMap.TryGetValue(complexTypeTable, out var tableReferenceExpression))
        {
            complexTypeTable = complexProperty.ComplexType.GetDefaultMappings().Single().Table;
            tableReferenceExpression = containerProjection.TableMap[complexTypeTable];
        }
        var isComplexTypeNullable = containerProjection.IsNullable || complexProperty.IsNullable;

        // If the complex property is declared on a type that's derived relative to the type being projected, the projected column is
        // nullable.
        if (!isComplexTypeNullable
            && containerProjection.StructuralType is IEntityType entityType
            && !entityType.GetAllBaseTypesInclusiveAscending().Contains(complexProperty.DeclaringType))
        {
            isComplexTypeNullable = true;
        }

        foreach (var property in complexProperty.ComplexType.GetProperties())
        {
            // TODO: Reimplement EntityProjectionExpression via TableMap, and then use that here
            var column = complexTypeTable.FindColumn(property)!;
            propertyExpressionMap[property] = CreateColumnExpression(
                property, column, tableReferenceExpression, isComplexTypeNullable || column.IsNullable);
        }

        // The table map of the target complex type should only ever contains a single table (no table splitting).
        // If the source is itself a complex type (nested complex type), its table map is already suitable and we can just pass it on.
        var newTableMap = containerProjection.TableMap.Count == 1
            ? containerProjection.TableMap
            : new Dictionary<ITableBase, string> { [complexTypeTable] = tableReferenceExpression };

        Check.DebugAssert(newTableMap.Single().Key == complexTypeTable, "Bad new table map");

        var entityShaper = new RelationalStructuralTypeShaperExpression(
            complexProperty.ComplexType,
            new StructuralTypeProjectionExpression(complexProperty.ComplexType, propertyExpressionMap, newTableMap, isComplexTypeNullable),
            isComplexTypeNullable);

        return entityShaper;
    }

    /// <summary>
    ///     Retrieves the <see cref="TableExpressionBase" /> referenced by the given column, looking it up on this
    ///     <see cref="SelectExpression" /> based on its alias.
    /// </summary>
    public TableExpressionBase GetTable(ColumnExpression column)
    {
        foreach (var table in Tables)
        {
            if (table.UnwrapJoin().Alias == column.TableAlias)
            {
                return table;
            }
        }

        throw new InvalidOperationException($"Table not found with alias '{column.TableAlias}'");
    }

    private bool ContainsReferencedTable(ColumnExpression column)
    {
        foreach (var table in Tables)
        {
            var unwrappedTable = table.UnwrapJoin();
            if (unwrappedTable.Alias == column.TableAlias)
            {
                return true;
            }
        }

        return false;
    }

    private enum JoinType
    {
        InnerJoin,
        LeftJoin,
        CrossJoin,
        CrossApply,
        OuterApply
    }

    private static ShapedQueryExpression AddJoin2(
        ShapedQueryExpression outerSource,
        ShapedQueryExpression innerSource,
        JoinType joinType,
        SqlExpression? joinPredicate = null)
    {
        var outerSelectExpression = (SelectExpression)outerSource.QueryExpression;
        var innerSelectExpression = (SelectExpression)innerSource.QueryExpression;
        var (innerShaper, outerShaper) = (innerSource.ShaperExpression, outerSource.ShaperExpression);

        outerSelectExpression = outerSelectExpression.AddJoin2(joinType, ref innerSelectExpression, out _, joinPredicate);

        var transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaper.Type, innerShaper.Type);
        var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer")!;
        var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner")!;
        var outerClientEval = outerSelectExpression._clientProjections.Count > 0;
        var innerClientEval = innerSelectExpression._clientProjections.Count > 0;
        var innerNullable = joinType is JoinType.LeftJoin or JoinType.OuterApply;

        if (outerClientEval)
        {
            // Outer projection are already populated
            if (innerClientEval)
            {
                // Add inner to projection and update indexes
                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var i = 0; i < innerSelectExpression._clientProjections.Count; i++)
                {
                    var projectionToAdd = innerSelectExpression._clientProjections[i];
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    outerSelectExpression._clientProjections.Add(projectionToAdd);
                    outerSelectExpression._aliasForClientProjections.Add(innerSelectExpression._aliasForClientProjections[i]);
                    indexMap[i] = outerSelectExpression._clientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();

                innerShaper = new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, outerSelectExpression, indexMap).Visit(innerShaper);
            }
            else
            {
                // Apply inner projection mapping and convert projection member binding to indexes
                var mapping = outerSelectExpression.ConvertProjectionMappingToClientProjections(
                    innerSelectExpression._projectionMapping, innerNullable);
                innerShaper = new ProjectionMemberToIndexConvertingExpressionVisitor(outerSelectExpression, mapping).Visit(innerShaper);
            }
        }
        else
        {
            // Depending on inner, we may either need to populate outer projection or update projection members
            if (innerClientEval)
            {
                // Since inner projections are populated, we need to populate outer also
                var mapping = outerSelectExpression.ConvertProjectionMappingToClientProjections(outerSelectExpression._projectionMapping);
                outerShaper = new ProjectionMemberToIndexConvertingExpressionVisitor(outerSelectExpression, mapping).Visit(outerShaper);

                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var i = 0; i < innerSelectExpression._clientProjections.Count; i++)
                {
                    var projectionToAdd = innerSelectExpression._clientProjections[i];
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    outerSelectExpression._clientProjections.Add(projectionToAdd);
                    outerSelectExpression._aliasForClientProjections.Add(innerSelectExpression._aliasForClientProjections[i]);
                    indexMap[i] = outerSelectExpression._clientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();

                innerShaper =
                    new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, outerSelectExpression, indexMap)
                        .Visit(innerShaper);
            }
            else
            {
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                var mapping = new Dictionary<ProjectionMember, ProjectionMember>();

                foreach (var (projectionMember, expression) in outerSelectExpression._projectionMapping)
                {
                    var remappedProjectionMember = projectionMember.Prepend(outerMemberInfo);
                    mapping[projectionMember] = remappedProjectionMember;
                    projectionMapping[remappedProjectionMember] = expression;
                }

                outerShaper = new ProjectionMemberRemappingExpressionVisitor(outerSelectExpression, mapping).Visit(outerShaper);
                mapping.Clear();

                foreach (var projection in innerSelectExpression._projectionMapping)
                {
                    var projectionMember = projection.Key;
                    var remappedProjectionMember = projection.Key.Prepend(innerMemberInfo);
                    mapping[projectionMember] = remappedProjectionMember;
                    var projectionToAdd = projection.Value;
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    projectionMapping[remappedProjectionMember] = projectionToAdd;
                }

                innerShaper = new ProjectionMemberRemappingExpressionVisitor(outerSelectExpression, mapping).Visit(innerShaper);
                outerSelectExpression._projectionMapping = projectionMapping;
                innerSelectExpression._projectionMapping.Clear();
            }
        }

        if (innerNullable)
        {
            innerShaper = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaper);
        }

        return outerSource
            .UpdateShaperExpression(
                New(
                    transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { outerShaper, innerShaper }, outerMemberInfo, innerMemberInfo))
            .UpdateQueryExpression(outerSelectExpression);
    }

    private void AddJoin(
        JoinType joinType,
        ref SelectExpression innerSelectExpression,
        out bool innerPushdownOccurred,
        SqlExpression? joinPredicate = null)
    {
        innerPushdownOccurred = false;
        // Try to convert Apply to normal join
        if (joinType is JoinType.CrossApply or JoinType.OuterApply)
        {
            var limit = innerSelectExpression.Limit;
            var offset = innerSelectExpression.Offset;
            if (!innerSelectExpression.IsDistinct
                || (limit == null && offset == null))
            {
                innerSelectExpression.Limit = null;
                innerSelectExpression.Offset = null;

                var originalInnerSelectPredicate = innerSelectExpression.GroupBy.Count > 0
                    ? innerSelectExpression.Having
                    : innerSelectExpression.Predicate;

                joinPredicate = TryExtractJoinKey(this, innerSelectExpression, allowNonEquality: limit == null && offset == null);
                if (joinPredicate != null)
                {
                    var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(this)
                        .ContainsOuterReference(innerSelectExpression);
                    if (!containsOuterReference)
                    {
                        if (limit != null || offset != null)
                        {
                            var partitions = new List<SqlExpression>();
                            GetPartitions(innerSelectExpression, joinPredicate, partitions);
                            var orderings = innerSelectExpression.Orderings.Count > 0
                                ? innerSelectExpression.Orderings
                                : innerSelectExpression._identifier.Count > 0
                                    ? innerSelectExpression._identifier.Select(e => new OrderingExpression(e.Column, true))
                                    : new[] { new OrderingExpression(new SqlFragmentExpression("(SELECT 1)"), true) };

                            var rowNumberExpression = new RowNumberExpression(
                                partitions, orderings.ToList(), (limit ?? offset)!.TypeMapping);
                            innerSelectExpression.ClearOrdering();

                            joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);

                            var outerColumn = ((SelectExpression)innerSelectExpression.Tables[0]).GenerateOuterColumn(
                                innerSelectExpression.Tables[0].Alias!, rowNumberExpression, "row");
                            SqlExpression? offsetPredicate = null;
                            SqlExpression? limitPredicate = null;
                            if (offset != null)
                            {
                                offsetPredicate = new SqlBinaryExpression(
                                    ExpressionType.LessThan, offset, outerColumn, typeof(bool), joinPredicate.TypeMapping);
                            }

                            if (limit != null)
                            {
                                if (offset != null)
                                {
                                    limit = offset is SqlConstantExpression offsetConstant
                                        && limit is SqlConstantExpression limitConstant
                                            ? new SqlConstantExpression(
                                                Constant((int)offsetConstant.Value! + (int)limitConstant.Value!),
                                                limit.TypeMapping)
                                            : new SqlBinaryExpression(ExpressionType.Add, offset, limit, limit.Type, limit.TypeMapping);
                                }

                                limitPredicate = new SqlBinaryExpression(
                                    ExpressionType.LessThanOrEqual, outerColumn, limit, typeof(bool), joinPredicate.TypeMapping);
                            }

                            var predicate = offsetPredicate != null
                                ? limitPredicate != null
                                    ? new SqlBinaryExpression(
                                        ExpressionType.AndAlso, offsetPredicate, limitPredicate, typeof(bool),
                                        joinPredicate.TypeMapping)
                                    : offsetPredicate
                                : limitPredicate;
                            innerSelectExpression.ApplyPredicate(predicate!);
                        }

                        AddJoin(
                            joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                            ref innerSelectExpression,
                            out innerPushdownOccurred,
                            joinPredicate);

                        return;
                    }

                    if (originalInnerSelectPredicate != null)
                    {
                        if (innerSelectExpression.GroupBy.Count > 0)
                        {
                            innerSelectExpression.Having = originalInnerSelectPredicate;
                        }
                        else
                        {
                            innerSelectExpression.Predicate = originalInnerSelectPredicate;
                        }
                    }

                    joinPredicate = null;
                }

                // Order matters Apply Offset before Limit
                if (offset != null)
                {
                    innerSelectExpression.ApplyOffset(offset);
                }

                if (limit != null)
                {
                    innerSelectExpression.ApplyLimit(limit);
                }
            }
        }

        if (Limit != null
            || Offset != null
            || IsDistinct
            || GroupBy.Count > 0)
        {
            var sqlRemappingVisitor = PushdownIntoSubqueryInternal();
            innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression);
            joinPredicate = sqlRemappingVisitor.Remap(joinPredicate);
        }

        if (innerSelectExpression.Limit != null
            || innerSelectExpression.Offset != null
            || innerSelectExpression.IsDistinct
            || innerSelectExpression.Predicate != null
            || innerSelectExpression.Tables.Count > 1
            || innerSelectExpression.GroupBy.Count > 0)
        {
            joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);
            innerPushdownOccurred = true;
        }

        if (_identifier.Count > 0
            && innerSelectExpression._identifier.Count > 0)
        {
            if (joinType is JoinType.LeftJoin or JoinType.OuterApply)
            {
                _identifier.AddRange(innerSelectExpression._identifier.Select(e => (e.Column.MakeNullable(), e.Comparer)));
            }
            else
            {
                _identifier.AddRange(innerSelectExpression._identifier);
            }
        }
        else
        {
            // if the subquery that is joined to can't be uniquely identified
            // then the entire join should also not be marked as non-identifiable
            _identifier.Clear();
            innerSelectExpression._identifier.Clear();
        }

        var innerTable = innerSelectExpression.Tables.Single();
        var joinTable = joinType switch
        {
            JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate!),
            JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate!),
            JoinType.CrossJoin => new CrossJoinExpression(innerTable),
            JoinType.CrossApply => new CrossApplyExpression(innerTable),
            JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
            _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(joinType), joinType))
        };

        _tables.Add(joinTable);

        static void GetPartitions(SelectExpression selectExpression, SqlExpression sqlExpression, List<SqlExpression> partitions)
        {
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    if (sqlBinaryExpression.Left is ColumnExpression columnExpression
                        && selectExpression.ContainsReferencedTable(columnExpression))
                    {
                        partitions.Add(sqlBinaryExpression.Left);
                    }
                    else
                    {
                        partitions.Add(sqlBinaryExpression.Right);
                    }
                }
                else if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    GetPartitions(selectExpression, sqlBinaryExpression.Left, partitions);
                    GetPartitions(selectExpression, sqlBinaryExpression.Right, partitions);
                }
            }
        }

        static SqlExpression? TryExtractJoinKey(SelectExpression outer, SelectExpression inner, bool allowNonEquality)
        {
            if (inner.Limit != null
                || inner.Offset != null)
            {
                return null;
            }

            var predicate = inner.GroupBy.Count > 0 ? inner.Having : inner.Predicate;
            if (predicate == null)
            {
                return null;
            }

            var outerColumnExpressions = new List<SqlExpression>();
            var joinPredicate = TryExtractJoinKey(
                outer,
                inner,
                predicate,
                outerColumnExpressions,
                allowNonEquality,
                out var updatedPredicate);

            if (joinPredicate != null)
            {
                joinPredicate = RemoveRedundantNullChecks(joinPredicate, outerColumnExpressions);
            }

            // we can't convert apply to join in case of distinct and group by, if the projection doesn't already contain the join keys
            // since we can't add the missing keys to the projection - only convert to join if all the keys are already there
            if (joinPredicate != null
                && (inner.IsDistinct
                    || inner.GroupBy.Count > 0))
            {
                var innerKeyColumns = new List<ColumnExpression>();
                PopulateInnerKeyColumns(inner, joinPredicate, innerKeyColumns);

                // if projection has already been applied we can use it directly
                // otherwise we extract future projection columns from projection mapping
                // and based on that we determine whether we can convert from APPLY to JOIN
                var projectionColumns = inner.Projection.Count > 0
                    ? inner.Projection.Select(p => p.Expression)
                    : ExtractColumnsFromProjectionMapping(inner._projectionMapping);

                foreach (var innerColumn in innerKeyColumns)
                {
                    if (!projectionColumns.Contains(innerColumn))
                    {
                        return null;
                    }
                }
            }

            if (inner.GroupBy.Count > 0)
            {
                inner.Having = updatedPredicate;
            }
            else
            {
                inner.Predicate = updatedPredicate;
            }

            return joinPredicate;

            static SqlExpression? TryExtractJoinKey(
                SelectExpression outer,
                SelectExpression inner,
                SqlExpression predicate,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality,
                out SqlExpression? updatedPredicate)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    var joinPredicate = ValidateKeyComparison(
                        outer, inner, sqlBinaryExpression, outerColumnExpressions, allowNonEquality);
                    if (joinPredicate != null)
                    {
                        updatedPredicate = null;

                        return joinPredicate;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Left, outerColumnExpressions, allowNonEquality, out var leftPredicate);
                        var rightJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Right, outerColumnExpressions, allowNonEquality, out var rightPredicate);

                        updatedPredicate = CombineNonNullExpressions(leftPredicate, rightPredicate);

                        return CombineNonNullExpressions(leftJoinKey, rightJoinKey);
                    }
                }

                updatedPredicate = predicate;

                return null;
            }

            static SqlBinaryExpression? ValidateKeyComparison(
                SelectExpression outer,
                SelectExpression inner,
                SqlBinaryExpression sqlBinaryExpression,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    || (allowNonEquality
                        && sqlBinaryExpression.OperatorType is ExpressionType.NotEqual
                            or ExpressionType.GreaterThan
                            or ExpressionType.GreaterThanOrEqual
                            or ExpressionType.LessThan
                            or ExpressionType.LessThanOrEqual))
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && IsContainedSql(inner, sqlBinaryExpression.Right))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Left);

                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && IsContainedSql(inner, sqlBinaryExpression.Left))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Right);

                        var mirroredOperation = sqlBinaryExpression.OperatorType switch
                        {
                            ExpressionType.Equal => ExpressionType.Equal,
                            ExpressionType.NotEqual => ExpressionType.NotEqual,
                            ExpressionType.LessThan => ExpressionType.GreaterThan,
                            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                            ExpressionType.GreaterThan => ExpressionType.LessThan,
                            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,

                            _ => throw new UnreachableException()
                        };

                        return new SqlBinaryExpression(
                            mirroredOperation,
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Type,
                            sqlBinaryExpression.TypeMapping);
                    }
                }

                // null checks are considered part of join key
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && sqlBinaryExpression.Left is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }

                return null;
            }

            static bool IsContainedSql(SelectExpression selectExpression, SqlExpression sqlExpression)
                => sqlExpression switch
                {
                    ColumnExpression columnExpression => selectExpression.ContainsReferencedTable(columnExpression),

                    // We check condition in a separate function to avoid matching structure of condition outside of case block
                    CaseExpression
                        {
                            Operand: null,
                            WhenClauses: [{ Result: ColumnExpression resultColumn } whenClause],
                            ElseResult: null
                        }
                        => IsContainedCondition(selectExpression, whenClause.Test) && selectExpression.ContainsReferencedTable(resultColumn),

                    _ => false
                };

            static bool IsContainedCondition(SelectExpression selectExpression, SqlExpression condition)
            {
                if (condition is not SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse or ExpressionType.NotEqual
                    } sqlBinaryExpression)
                {
                    return false;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    // We don't check left/right inverted because we generate this.
                    return sqlBinaryExpression is { Left: ColumnExpression column, Right: SqlConstantExpression { Value: null } }
                        && selectExpression.ContainsReferencedTable(column);
                }

                return IsContainedCondition(selectExpression, sqlBinaryExpression.Left)
                    && IsContainedCondition(selectExpression, sqlBinaryExpression.Right);
            }

            static void PopulateInnerKeyColumns(
                SelectExpression select,
                SqlExpression joinPredicate,
                List<ColumnExpression> resultColumns)
            {
                switch (joinPredicate)
                {
                    case SqlBinaryExpression binary:
                        PopulateInnerKeyColumns(select, binary.Left, resultColumns);
                        PopulateInnerKeyColumns(select, binary.Right, resultColumns);
                        break;
                    case ColumnExpression columnExpression when select.ContainsReferencedTable(columnExpression):
                        resultColumns.Add(columnExpression);
                        break;
                }
            }

            static List<ColumnExpression> ExtractColumnsFromProjectionMapping(
                IDictionary<ProjectionMember, Expression> projectionMapping)
            {
                var result = new List<ColumnExpression>();
                foreach (var (_, expression) in projectionMapping)
                {
                    if (expression is StructuralTypeProjectionExpression projection)
                    {
                        foreach (var property in projection.StructuralType.GetAllPropertiesInHierarchy())
                        {
                            result.Add(projection.BindProperty(property));
                        }

                        if (projection.DiscriminatorExpression is ColumnExpression discriminatorColumn)
                        {
                            result.Add(discriminatorColumn);
                        }
                    }
                    else if (expression is ColumnExpression column)
                    {
                        result.Add(column);
                    }
                }

                return result;
            }

            static SqlExpression? CombineNonNullExpressions(SqlExpression? left, SqlExpression? right)
                => left != null
                    ? right != null
                        ? new SqlBinaryExpression(ExpressionType.AndAlso, left, right, left.Type, left.TypeMapping)
                        : left
                    : right;

            static SqlExpression? RemoveRedundantNullChecks(SqlExpression predicate, List<SqlExpression> outerColumnExpressions)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                        && outerColumnExpressions.Contains(sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
                    {
                        return null;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, outerColumnExpressions);
                        var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, outerColumnExpressions);

                        return CombineNonNullExpressions(leftPredicate, rightPredicate);
                    }
                }

                return predicate;
            }
        }
    }

    private SelectExpression AddJoin2(
        JoinType joinType,
        ref SelectExpression innerSelectExpression,
        out bool innerPushdownOccurred,
        SqlExpression? joinPredicate = null)
    {
        return AddJoinCore(this, joinType, ref innerSelectExpression, out innerPushdownOccurred, joinPredicate);

        static SelectExpression AddJoinCore(
            SelectExpression outerSelectExpression,
            JoinType joinType,
            ref SelectExpression innerSelectExpression,
            out bool innerPushdownOccurred,
            SqlExpression? joinPredicate = null)
        {
            innerPushdownOccurred = false;
            // Try to convert Apply to normal join
            if (joinType is JoinType.CrossApply or JoinType.OuterApply)
            {
                var limit = innerSelectExpression.Limit;
                var offset = innerSelectExpression.Offset;
                if (!innerSelectExpression.IsDistinct
                    || (limit == null && offset == null))
                {
                    innerSelectExpression.Limit = null;
                    innerSelectExpression.Offset = null;

                    var originalInnerSelectPredicate = innerSelectExpression.GroupBy.Count > 0
                        ? innerSelectExpression.Having
                        : innerSelectExpression.Predicate;

                    joinPredicate = TryExtractJoinKey(outerSelectExpression, innerSelectExpression, allowNonEquality: limit == null && offset == null);
                    if (joinPredicate != null)
                    {
                        var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(outerSelectExpression)
                            .ContainsOuterReference(innerSelectExpression);
                        if (!containsOuterReference)
                        {
                            if (limit != null || offset != null)
                            {
                                var partitions = new List<SqlExpression>();
                                GetPartitions(innerSelectExpression, joinPredicate, partitions);
                                var orderings = innerSelectExpression.Orderings.Count > 0
                                    ? innerSelectExpression.Orderings
                                    : innerSelectExpression._identifier.Count > 0
                                        ? innerSelectExpression._identifier.Select(e => new OrderingExpression(e.Column, true))
                                        : new[] { new OrderingExpression(new SqlFragmentExpression("(SELECT 1)"), true) };

                                var rowNumberExpression = new RowNumberExpression(
                                    partitions, orderings.ToList(), (limit ?? offset)!.TypeMapping);
                                innerSelectExpression.ClearOrdering();

                                joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);

                                var outerColumn = ((SelectExpression)innerSelectExpression.Tables[0]).GenerateOuterColumn(
                                    innerSelectExpression.Tables[0].Alias!, rowNumberExpression, "row");
                                SqlExpression? offsetPredicate = null;
                                SqlExpression? limitPredicate = null;
                                if (offset != null)
                                {
                                    offsetPredicate = new SqlBinaryExpression(
                                        ExpressionType.LessThan, offset, outerColumn, typeof(bool), joinPredicate.TypeMapping);
                                }

                                if (limit != null)
                                {
                                    if (offset != null)
                                    {
                                        limit = offset is SqlConstantExpression offsetConstant
                                            && limit is SqlConstantExpression limitConstant
                                                ? new SqlConstantExpression(
                                                    Constant((int)offsetConstant.Value! + (int)limitConstant.Value!),
                                                    limit.TypeMapping)
                                                : new SqlBinaryExpression(ExpressionType.Add, offset, limit, limit.Type, limit.TypeMapping);
                                    }

                                    limitPredicate = new SqlBinaryExpression(
                                        ExpressionType.LessThanOrEqual, outerColumn, limit, typeof(bool), joinPredicate.TypeMapping);
                                }

                                var predicate = offsetPredicate != null
                                    ? limitPredicate != null
                                        ? new SqlBinaryExpression(
                                            ExpressionType.AndAlso, offsetPredicate, limitPredicate, typeof(bool),
                                            joinPredicate.TypeMapping)
                                        : offsetPredicate
                                    : limitPredicate;
                                innerSelectExpression.ApplyPredicate(predicate!);
                            }

                            return AddJoinCore(
                                outerSelectExpression,
                                joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                                ref innerSelectExpression,
                                out innerPushdownOccurred,
                                joinPredicate);
                        }

                        if (originalInnerSelectPredicate != null)
                        {
                            if (innerSelectExpression.GroupBy.Count > 0)
                            {
                                innerSelectExpression.Having = originalInnerSelectPredicate;
                            }
                            else
                            {
                                innerSelectExpression.Predicate = originalInnerSelectPredicate;
                            }
                        }

                        joinPredicate = null;
                    }

                    // Order matters Apply Offset before Limit
                    if (offset != null)
                    {
                        innerSelectExpression = innerSelectExpression.ApplyOffset2(offset);
                    }

                    if (limit != null)
                    {
                        innerSelectExpression = innerSelectExpression.ApplyLimit2(limit);
                    }
                }
            }

            if (outerSelectExpression.Limit != null
                || outerSelectExpression.Offset != null
                || outerSelectExpression.IsDistinct
                || outerSelectExpression.GroupBy.Count > 0)
            {
                (outerSelectExpression, var sqlRemappingVisitor) = outerSelectExpression.PushdownIntoSubqueryInternal2();
                innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression, out var subquery);
                if (joinPredicate is not null)
                {
                    joinPredicate = sqlRemappingVisitor.Remap(joinPredicate, out subquery);
                }

                outerSelectExpression = outerSelectExpression.WithTables([subquery]);
            }

            if (innerSelectExpression.Limit != null
                || innerSelectExpression.Offset != null
                || innerSelectExpression.IsDistinct
                || innerSelectExpression.Predicate != null
                || innerSelectExpression.Tables.Count > 1
                || innerSelectExpression.GroupBy.Count > 0)
            {
                joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);
                innerPushdownOccurred = true;
            }

            var newIdentifiers = new List<(ColumnExpression Column, ValueComparer Comparer)>();
            if (outerSelectExpression._identifier.Count > 0
                && innerSelectExpression._identifier.Count > 0)
            {
                newIdentifiers.AddRange(outerSelectExpression._identifier);
                newIdentifiers.AddRange(
                    joinType is JoinType.LeftJoin or JoinType.OuterApply
                        ? innerSelectExpression._identifier.Select(e => (e.Column.MakeNullable(), e.Comparer))
                        : innerSelectExpression._identifier);
            }
            else
            {
                // If the subquery that is joined to can't be uniquely identified, then the entire join should also not be marked as
                // non-identifiable. Leave the outer identifier list empty and clear the inner's.
                innerSelectExpression._identifier.Clear();
            }

            var innerTable = innerSelectExpression.Tables.Single();
            var joinTable = joinType switch
            {
                JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate!),
                JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate!),
                JoinType.CrossJoin => new CrossJoinExpression(innerTable),
                JoinType.CrossApply => new CrossApplyExpression(innerTable),
                JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
                _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(joinType), joinType))
            };

            outerSelectExpression = outerSelectExpression.Update(
                outerSelectExpression.Projection,
                [..outerSelectExpression.Tables, joinTable],
                outerSelectExpression.Predicate,
                outerSelectExpression.GroupBy,
                outerSelectExpression.Having,
                outerSelectExpression.Orderings,
                outerSelectExpression.Limit,
                outerSelectExpression.Offset);

            outerSelectExpression._identifier = newIdentifiers;
            return outerSelectExpression;
        }

        static void GetPartitions(SelectExpression selectExpression, SqlExpression sqlExpression, List<SqlExpression> partitions)
        {
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    if (sqlBinaryExpression.Left is ColumnExpression columnExpression
                        && selectExpression.ContainsReferencedTable(columnExpression))
                    {
                        partitions.Add(sqlBinaryExpression.Left);
                    }
                    else
                    {
                        partitions.Add(sqlBinaryExpression.Right);
                    }
                }
                else if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    GetPartitions(selectExpression, sqlBinaryExpression.Left, partitions);
                    GetPartitions(selectExpression, sqlBinaryExpression.Right, partitions);
                }
            }
        }

        static SqlExpression? TryExtractJoinKey(SelectExpression outer, SelectExpression inner, bool allowNonEquality)
        {
            if (inner.Limit != null
                || inner.Offset != null)
            {
                return null;
            }

            var predicate = inner.GroupBy.Count > 0 ? inner.Having : inner.Predicate;
            if (predicate == null)
            {
                return null;
            }

            var outerColumnExpressions = new List<SqlExpression>();
            var joinPredicate = TryExtractJoinKey(
                outer,
                inner,
                predicate,
                outerColumnExpressions,
                allowNonEquality,
                out var updatedPredicate);

            if (joinPredicate != null)
            {
                joinPredicate = RemoveRedundantNullChecks(joinPredicate, outerColumnExpressions);
            }

            // we can't convert apply to join in case of distinct and group by, if the projection doesn't already contain the join keys
            // since we can't add the missing keys to the projection - only convert to join if all the keys are already there
            if (joinPredicate != null
                && (inner.IsDistinct
                    || inner.GroupBy.Count > 0))
            {
                var innerKeyColumns = new List<ColumnExpression>();
                PopulateInnerKeyColumns(inner, joinPredicate, innerKeyColumns);

                // if projection has already been applied we can use it directly
                // otherwise we extract future projection columns from projection mapping
                // and based on that we determine whether we can convert from APPLY to JOIN
                var projectionColumns = inner.Projection.Count > 0
                    ? inner.Projection.Select(p => p.Expression)
                    : ExtractColumnsFromProjectionMapping(inner._projectionMapping);

                foreach (var innerColumn in innerKeyColumns)
                {
                    if (!projectionColumns.Contains(innerColumn))
                    {
                        return null;
                    }
                }
            }

            if (inner.GroupBy.Count > 0)
            {
                inner.Having = updatedPredicate;
            }
            else
            {
                inner.Predicate = updatedPredicate;
            }

            return joinPredicate;

            static SqlExpression? TryExtractJoinKey(
                SelectExpression outer,
                SelectExpression inner,
                SqlExpression predicate,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality,
                out SqlExpression? updatedPredicate)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    var joinPredicate = ValidateKeyComparison(
                        outer, inner, sqlBinaryExpression, outerColumnExpressions, allowNonEquality);
                    if (joinPredicate != null)
                    {
                        updatedPredicate = null;

                        return joinPredicate;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Left, outerColumnExpressions, allowNonEquality, out var leftPredicate);
                        var rightJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Right, outerColumnExpressions, allowNonEquality, out var rightPredicate);

                        updatedPredicate = CombineNonNullExpressions(leftPredicate, rightPredicate);

                        return CombineNonNullExpressions(leftJoinKey, rightJoinKey);
                    }
                }

                updatedPredicate = predicate;

                return null;
            }

            static SqlBinaryExpression? ValidateKeyComparison(
                SelectExpression outer,
                SelectExpression inner,
                SqlBinaryExpression sqlBinaryExpression,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    || (allowNonEquality
                        && sqlBinaryExpression.OperatorType is ExpressionType.NotEqual
                            or ExpressionType.GreaterThan
                            or ExpressionType.GreaterThanOrEqual
                            or ExpressionType.LessThan
                            or ExpressionType.LessThanOrEqual))
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && IsContainedSql(inner, sqlBinaryExpression.Right))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Left);

                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && IsContainedSql(inner, sqlBinaryExpression.Left))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Right);

                        var mirroredOperation = sqlBinaryExpression.OperatorType switch
                        {
                            ExpressionType.Equal => ExpressionType.Equal,
                            ExpressionType.NotEqual => ExpressionType.NotEqual,
                            ExpressionType.LessThan => ExpressionType.GreaterThan,
                            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                            ExpressionType.GreaterThan => ExpressionType.LessThan,
                            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,

                            _ => throw new UnreachableException()
                        };

                        return new SqlBinaryExpression(
                            mirroredOperation,
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Type,
                            sqlBinaryExpression.TypeMapping);
                    }
                }

                // null checks are considered part of join key
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && sqlBinaryExpression.Left is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }

                return null;
            }

            static bool IsContainedSql(SelectExpression selectExpression, SqlExpression sqlExpression)
                => sqlExpression switch
                {
                    ColumnExpression columnExpression => selectExpression.ContainsReferencedTable(columnExpression),

                    // We check condition in a separate function to avoid matching structure of condition outside of case block
                    CaseExpression
                        {
                            Operand: null,
                            WhenClauses: [{ Result: ColumnExpression resultColumn } whenClause],
                            ElseResult: null
                        }
                        => IsContainedCondition(selectExpression, whenClause.Test) && selectExpression.ContainsReferencedTable(resultColumn),

                    _ => false
                };

            static bool IsContainedCondition(SelectExpression selectExpression, SqlExpression condition)
            {
                if (condition is not SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse or ExpressionType.NotEqual
                    } sqlBinaryExpression)
                {
                    return false;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    // We don't check left/right inverted because we generate this.
                    return sqlBinaryExpression is { Left: ColumnExpression column, Right: SqlConstantExpression { Value: null } }
                        && selectExpression.ContainsReferencedTable(column);
                }

                return IsContainedCondition(selectExpression, sqlBinaryExpression.Left)
                    && IsContainedCondition(selectExpression, sqlBinaryExpression.Right);
            }

            static void PopulateInnerKeyColumns(
                SelectExpression select,
                SqlExpression joinPredicate,
                List<ColumnExpression> resultColumns)
            {
                switch (joinPredicate)
                {
                    case SqlBinaryExpression binary:
                        PopulateInnerKeyColumns(select, binary.Left, resultColumns);
                        PopulateInnerKeyColumns(select, binary.Right, resultColumns);
                        break;
                    case ColumnExpression columnExpression when select.ContainsReferencedTable(columnExpression):
                        resultColumns.Add(columnExpression);
                        break;
                }
            }

            static List<ColumnExpression> ExtractColumnsFromProjectionMapping(
                IDictionary<ProjectionMember, Expression> projectionMapping)
            {
                var result = new List<ColumnExpression>();
                foreach (var (_, expression) in projectionMapping)
                {
                    if (expression is StructuralTypeProjectionExpression projection)
                    {
                        foreach (var property in projection.StructuralType.GetAllPropertiesInHierarchy())
                        {
                            result.Add(projection.BindProperty(property));
                        }

                        if (projection.DiscriminatorExpression is ColumnExpression discriminatorColumn)
                        {
                            result.Add(discriminatorColumn);
                        }
                    }
                    else if (expression is ColumnExpression column)
                    {
                        result.Add(column);
                    }
                }

                return result;
            }

            static SqlExpression? CombineNonNullExpressions(SqlExpression? left, SqlExpression? right)
                => left != null
                    ? right != null
                        ? new SqlBinaryExpression(ExpressionType.AndAlso, left, right, left.Type, left.TypeMapping)
                        : left
                    : right;

            static SqlExpression? RemoveRedundantNullChecks(SqlExpression predicate, List<SqlExpression> outerColumnExpressions)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                        && outerColumnExpressions.Contains(sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
                    {
                        return null;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, outerColumnExpressions);
                        var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, outerColumnExpressions);

                        return CombineNonNullExpressions(leftPredicate, rightPredicate);
                    }
                }

                return predicate;
            }
        }
    }

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using INNER JOIN and combine shapers.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="outerSource">The <see cref="ShapedQueryExpression" /> representing the outer query.</param>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <returns>An outer <see cref="ShapedQueryExpression" /> which is the result of this join.</returns>
    [EntityFrameworkInternal]
    public static ShapedQueryExpression AddInnerJoin(ShapedQueryExpression outerSource, ShapedQueryExpression innerSource, SqlExpression joinPredicate)
        => AddJoin2(outerSource, innerSource, JoinType.InnerJoin, joinPredicate);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using LEFT JOIN and combine shapers.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="outerSource">The <see cref="ShapedQueryExpression" /> representing the outer query.</param>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <returns>An outer <see cref="ShapedQueryExpression" /> which is the result of this join.</returns>
    [EntityFrameworkInternal]
    public static ShapedQueryExpression AddLeftJoin(
        ShapedQueryExpression outerSource,
        ShapedQueryExpression innerSource,
        SqlExpression joinPredicate)
        => AddJoin2(outerSource, innerSource, JoinType.LeftJoin, joinPredicate);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS JOIN and combine shapers.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="outerSource">The <see cref="ShapedQueryExpression" /> representing the outer query.</param>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <returns>An outer <see cref="ShapedQueryExpression" /> which is the result of this join.</returns>
    [EntityFrameworkInternal]
    public static ShapedQueryExpression AddCrossJoin(ShapedQueryExpression outerSource, ShapedQueryExpression innerSource)
        => AddJoin2(outerSource, innerSource, JoinType.CrossJoin);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS APPLY and combine shapers.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="outerSource">The <see cref="ShapedQueryExpression" /> representing the outer query.</param>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <returns>An outer <see cref="ShapedQueryExpression" /> which is the result of this join.</returns>
    [EntityFrameworkInternal]
    public static ShapedQueryExpression AddCrossApply(ShapedQueryExpression outerSource, ShapedQueryExpression innerSource)
        => AddJoin2(outerSource, innerSource, JoinType.CrossApply);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using OUTER APPLY and combine shapers.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="outerSource">The <see cref="ShapedQueryExpression" /> representing the outer query.</param>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    [EntityFrameworkInternal]
    public static ShapedQueryExpression AddOuterApply(ShapedQueryExpression outerSource, ShapedQueryExpression innerSource)
        => AddJoin2(outerSource, innerSource, JoinType.OuterApply);

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    public void PushdownIntoSubquery()
        => PushdownIntoSubqueryInternal();

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    /// <param name="liftOrderings">Whether orderings on the query should be lifted out of the subquery.</param>
    private SqlRemappingVisitor PushdownIntoSubqueryInternal(bool liftOrderings = true)
    {
        // If there's just one table in the select being pushed down, bubble up that table's name as the subquery's alias.
        var subqueryAlias =
            _sqlAliasManager.GenerateTableAlias(_tables is [{ Alias: string singleTableAlias }] ? singleTableAlias : "subquery");

        var subquery = new SelectExpression(
            subqueryAlias, _tables.ToList(), _groupBy.ToList(), [], _orderings.ToList(), Annotations, _sqlAliasManager)
        {
            IsDistinct = IsDistinct,
            Predicate = Predicate,
            Having = Having,
            Offset = Offset,
            Limit = Limit,
            IsMutable = false
        };
        _tables.Clear();
        _groupBy.Clear();
        _orderings.Clear();
        IsDistinct = false;
        Predicate = null;
        Having = null;
        Offset = null;
        Limit = null;
        _preGroupByIdentifier = null;

        _tables.Add(subquery);

        var projectionMap = new Dictionary<SqlExpression, ColumnExpression>(ReferenceEqualityComparer.Instance);

        if (_projection.Count > 0)
        {
            var projections = _projection.ToList();
            _projection.Clear();
            foreach (var projection in projections)
            {
                var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, projection.Expression, projection.Alias);
                AddToProjection(outerColumn, null);
                projectionMap[projection.Expression] = outerColumn;
            }
        }

        var nestedQueryInProjection = false;
        // Projection would be present for client eval case
        if (_clientProjections.Count > 0)
        {
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                var item = _clientProjections[i];
                // If item's value is ConstantExpression then projection has already been applied
                if (item is ConstantExpression)
                {
                    break;
                }

                if (item is StructuralTypeProjectionExpression projection)
                {
                    _clientProjections[i] = LiftEntityProjectionFromSubquery(projection, subqueryAlias);
                }
                else if (item is JsonQueryExpression jsonQueryExpression)
                {
                    _clientProjections[i] = LiftJsonQueryFromSubquery(jsonQueryExpression);
                }
                else if (item is SqlExpression sqlExpression)
                {
                    var alias = _aliasForClientProjections[i];
                    var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, sqlExpression, alias);
                    projectionMap[sqlExpression] = outerColumn;
                    _clientProjections[i] = outerColumn;
                    _aliasForClientProjections[i] = null;
                }
                else
                {
                    nestedQueryInProjection = true;
                }
            }
        }
        else
        {
            foreach (var (projectionMember, expression) in _projectionMapping.ToList())
            {
                // If projectionMapping's value is ConstantExpression then projection has already been applied
                if (expression is ConstantExpression)
                {
                    break;
                }

                if (expression is StructuralTypeProjectionExpression projection)
                {
                    _projectionMapping[projectionMember] = LiftEntityProjectionFromSubquery(projection, subqueryAlias);
                }
                else if (expression is JsonQueryExpression jsonQueryExpression)
                {
                    _projectionMapping[projectionMember] = LiftJsonQueryFromSubquery(jsonQueryExpression);
                }
                else
                {
                    var innerColumn = (SqlExpression)expression;
                    var outerColumn = subquery.GenerateOuterColumn(
                        subqueryAlias, innerColumn, projectionMember.Last?.Name);
                    projectionMap[innerColumn] = outerColumn;
                    _projectionMapping[projectionMember] = outerColumn;
                }
            }
        }

        if (subquery._groupBy.Count > 0
            && !subquery.IsDistinct)
        {
            foreach (var key in subquery._groupBy)
            {
                projectionMap[key] = subquery.GenerateOuterColumn(subqueryAlias, key);
            }
        }

        var identifiers = _identifier.ToList();
        _identifier.Clear();
        foreach (var (column, comparer) in identifiers)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!projectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn(subqueryAlias, column);
            }

            _identifier.Add((outerColumn, Comparer: comparer));
        }

        var childIdentifiers = _childIdentifiers.ToList();
        _childIdentifiers.Clear();
        foreach (var (column, comparer) in childIdentifiers)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!projectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn(subqueryAlias, column);
            }

            _childIdentifiers.Add((outerColumn, Comparer: comparer));
        }

        foreach (var ordering in subquery._orderings)
        {
            var orderingExpression = ordering.Expression;
            if (liftOrderings && projectionMap.TryGetValue(orderingExpression, out var outerColumn))
            {
                _orderings.Add(ordering.Update(outerColumn));
            }
            else if (liftOrderings
                     && (!IsDistinct
                         && GroupBy.Count == 0
                         || GroupBy.Contains(orderingExpression)))
            {
                _orderings.Add(
                    ordering.Update(
                        subquery.GenerateOuterColumn(subqueryAlias, orderingExpression)));
            }
            else
            {
                _orderings.Clear();
                break;
            }
        }

        if (subquery.Offset == null
            && subquery.Limit == null)
        {
            subquery.ClearOrdering();
        }

        var sqlRemappingVisitor = new SqlRemappingVisitor(projectionMap, subquery, subqueryAlias);

        if (nestedQueryInProjection)
        {
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                if (_clientProjections[i] is ShapedQueryExpression shapedQueryExpression)
                {
                    _clientProjections[i] = shapedQueryExpression.UpdateQueryExpression(
                        sqlRemappingVisitor.Remap((SelectExpression)shapedQueryExpression.QueryExpression));
                }
            }
        }

        return sqlRemappingVisitor;

        StructuralTypeProjectionExpression LiftEntityProjectionFromSubquery(
            StructuralTypeProjectionExpression projection,
            string subqueryAlias)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

            HandleTypeProjection(projection);

            void HandleTypeProjection(StructuralTypeProjectionExpression typeProjection)
            {
                foreach (var property in typeProjection.StructuralType.GetAllPropertiesInHierarchy())
                {
                    // json entity projection (i.e. JSON entity that was transformed into query root) may have synthesized keys
                    // but they don't correspond to any columns - we need to skip those
                    if (typeProjection is { StructuralType: IEntityType entityType }
                        && entityType.IsMappedToJson()
                        && property.IsOrdinalKeyProperty())
                    {
                        continue;
                    }

                    var innerColumn = typeProjection.BindProperty(property);
                    var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    propertyExpressions[property] = outerColumn;
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(typeProjection.StructuralType))
                {
                    HandleTypeProjection(
                        (StructuralTypeProjectionExpression)typeProjection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            ColumnExpression? discriminatorExpression = null;
            if (projection.DiscriminatorExpression != null)
            {
                discriminatorExpression = subquery.GenerateOuterColumn(
                    subqueryAlias, projection.DiscriminatorExpression, DiscriminatorColumnAlias);
                projectionMap[projection.DiscriminatorExpression] = discriminatorExpression;
            }

            var tableMap = projection.TableMap.ToDictionary(kvp => kvp.Key, _ => subqueryAlias);

            var newEntityProjection = new StructuralTypeProjectionExpression(
                projection.StructuralType, propertyExpressions, tableMap, nullable: false, discriminatorExpression);

            if (projection.StructuralType is IEntityType entityType2)
            {
                // Also lift nested entity projections
                foreach (var navigation in entityType2
                             .GetAllBaseTypes().Concat(entityType2.GetDerivedTypesInclusive())
                             .SelectMany(t => t.GetDeclaredNavigations()))
                {
                    var boundEntityShaperExpression = projection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var newValueBufferExpression =
                            boundEntityShaperExpression.ValueBufferExpression is StructuralTypeProjectionExpression innerEntityProjection
                                ? (Expression)LiftEntityProjectionFromSubquery(innerEntityProjection, subqueryAlias)
                                : LiftJsonQueryFromSubquery((JsonQueryExpression)boundEntityShaperExpression.ValueBufferExpression);

                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newValueBufferExpression);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }
            }

            return newEntityProjection;
        }

        JsonQueryExpression LiftJsonQueryFromSubquery(JsonQueryExpression jsonQueryExpression)
        {
            var jsonScalarExpression = new JsonScalarExpression(
                jsonQueryExpression.JsonColumn,
                jsonQueryExpression.Path,
                jsonQueryExpression.JsonColumn.TypeMapping!.ClrType,
                jsonQueryExpression.JsonColumn.TypeMapping,
                jsonQueryExpression.IsNullable);

            var newJsonColumn = subquery.GenerateOuterColumn(subqueryAlias, jsonScalarExpression);

            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var keyProperties = jsonQueryExpression.KeyPropertyMap.Keys.ToList();
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var keyProperty = keyProperties[i];
                var innerColumn = jsonQueryExpression.BindProperty(keyProperty);
                var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, innerColumn);
                projectionMap[innerColumn] = outerColumn;
                newKeyPropertyMap[keyProperty] = outerColumn;
            }

            // clear up the json path - we start from empty path after pushdown
            return new JsonQueryExpression(
                jsonQueryExpression.EntityType,
                newJsonColumn,
                newKeyPropertyMap,
                jsonQueryExpression.Type,
                jsonQueryExpression.IsCollection);
        }
    }

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    [Pure]
    public SelectExpression PushdownIntoSubquery2()
        => PushdownIntoSubqueryInternal2().Item1;

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    /// <param name="liftOrderings">Whether orderings on the query should be lifted out of the subquery.</param>
    /// <returns>A new <see cref="SelectExpression" /> which wraps this <see cref="SelectExpression" /> as a subquery.</returns>
    [Pure]
    private (SelectExpression, SqlRemappingVisitor2) PushdownIntoSubqueryInternal2(bool liftOrderings = true)
    {
        var select = this;

        // If there's just one table in the select being pushed down, bubble up that table's name as the subquery's alias.
        var subqueryAlias =
            select._sqlAliasManager.GenerateTableAlias(
                select._tables is [{ Alias: string singleTableAlias }] ? singleTableAlias : "subquery");
        var subquery = select
            .WithAlias(subqueryAlias)
            .Update(
                projections: [], select.Tables, select.Predicate, select.GroupBy, select.Having, select.Orderings, select.Limit,
                select.Offset);

        var innerOuterProjectionMap = new Dictionary<SqlExpression, ColumnExpression>(ReferenceEqualityComparer.Instance);

        var outerProjections = new List<ProjectionExpression>();
        var outerOrderings = new List<OrderingExpression>(liftOrderings ? subquery.Orderings.Count : 0);
        var outerProjectionMapping = new Dictionary<ProjectionMember, Expression>();
        var outerClientProjections = new List<Expression>(subquery._clientProjections);
        var outerAliasForClientProjections = subquery._aliasForClientProjections.ToList();
        var outerIdentifiers = new List<(ColumnExpression Column, ValueComparer Comparer)>();
        var outerChildIdentifiers = new List<(ColumnExpression Column, ValueComparer Comparer)>();

        if (subquery.Projection.Count > 0)
        {
            foreach (var projection in subquery.Projection)
            {
                var outerColumn = subquery.GenerateOuterColumn2(subqueryAlias, projection.Expression, out subquery, projection.Alias);
                AddToProjection2(outerProjections, outerColumn, generateAlias: true);
                innerOuterProjectionMap[projection.Expression] = outerColumn;
            }
        }

        var nestedQueryInProjection = false;
        // Projection would be present for client eval case
        if (subquery._clientProjections.Count > 0)
        {
            for (var i = 0; i < subquery._clientProjections.Count; i++)
            {
                switch (subquery._clientProjections[i])
                {
                    // If item's value is ConstantExpression then projection has already been applied
                    case ConstantExpression:
                        goto End;

                    case StructuralTypeProjectionExpression projection:
                        outerClientProjections[i] = LiftEntityProjectionFromSubquery(projection, subqueryAlias);
                        continue;

                    case JsonQueryExpression jsonQueryExpression:
                        outerClientProjections[i] = LiftJsonQueryFromSubquery(jsonQueryExpression);
                        continue;

                    case SqlExpression sqlExpression:
                    {
                        var alias = subquery._aliasForClientProjections[i];
                        var outerColumn = subquery.GenerateOuterColumn2(subqueryAlias, sqlExpression, out subquery, alias);
                        innerOuterProjectionMap[sqlExpression] = outerColumn;
                        outerClientProjections[i] = outerColumn;
                        outerAliasForClientProjections[i] = null;
                        continue;
                    }

                    default:
                        nestedQueryInProjection = true;
                        continue;
                }
            }

            End: ;
        }
        else
        {
            foreach (var (projectionMember, expression) in subquery._projectionMapping)
            {
                switch (expression)
                {
                    // If projectionMapping's value is ConstantExpression then projection has already been applied
                    case ConstantExpression:
                        goto End;

                    case StructuralTypeProjectionExpression projection:
                        outerProjectionMapping[projectionMember] = LiftEntityProjectionFromSubquery(projection, subqueryAlias);
                        continue;

                    case JsonQueryExpression jsonQueryExpression:
                        outerProjectionMapping[projectionMember] = LiftJsonQueryFromSubquery(jsonQueryExpression);
                        continue;

                    default:
                    {
                        var innerColumn = (SqlExpression)expression;
                        var outerColumn = subquery.GenerateOuterColumn2(
                            subqueryAlias, innerColumn, out subquery, projectionMember.Last?.Name);
                        innerOuterProjectionMap[innerColumn] = outerColumn;
                        outerProjectionMapping[projectionMember] = outerColumn;
                        continue;
                    }
                }
            }

            End: ;
        }

        if (subquery.GroupBy.Count > 0 && !subquery.IsDistinct)
        {
            foreach (var key in subquery.GroupBy)
            {
                innerOuterProjectionMap[key] = subquery.GenerateOuterColumn2(subqueryAlias, key, out subquery);
            }
        }

        // Note: Leaving the identifiers on the subquery (at least for now). They're not used (only meaningful on the top-level select),
        // but they don't hurt either. The ultimate plan is to remove these from SelectExpression altogether.
        foreach (var (column, comparer) in subquery._identifier)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!innerOuterProjectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn2(subqueryAlias, column, out subquery);
            }

            outerIdentifiers.Add((outerColumn, Comparer: comparer));
        }

        foreach (var (column, comparer) in subquery._childIdentifiers)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!innerOuterProjectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn2(subqueryAlias, column, out subquery);
            }

            outerChildIdentifiers.Add((outerColumn, Comparer: comparer));
        }

        if (liftOrderings)
        {
            foreach (var ordering in subquery.Orderings)
            {
                var orderingExpression = ordering.Expression;
                outerOrderings.Add(
                    ordering.Update(
                        innerOuterProjectionMap.TryGetValue(orderingExpression, out var outerColumn)
                            ? outerColumn
                            : subquery.GenerateOuterColumn(subqueryAlias, orderingExpression)));
            }
        }

        if (subquery.Offset == null && subquery.Limit == null)
        {
            subquery = subquery.WithOrderings([]);
        }

        var sqlRemappingVisitor = new SqlRemappingVisitor2(innerOuterProjectionMap, subquery, subqueryAlias);

        if (nestedQueryInProjection)
        {
            for (var i = 0; i < outerClientProjections.Count; i++)
            {
                if (outerClientProjections[i] is ShapedQueryExpression shapedQueryExpression)
                {
                    var projectedSelect = sqlRemappingVisitor.Remap((SelectExpression)shapedQueryExpression.QueryExpression, out subquery);
                    outerClientProjections[i] = shapedQueryExpression.UpdateQueryExpression(projectedSelect);
                }
            }
        }

        subquery._identifier = [];
        subquery._childIdentifiers = [];
        subquery.IsMutable = false;

        var outerSelect = new SelectExpression(
            alias: null, // TODO
            tables: [subquery],
            groupBy: [],
            projections: outerProjections,
            orderings: outerOrderings,
            annotations: null,
            select._sqlAliasManager)
        {
            _projectionMapping = outerProjectionMapping,
            _clientProjections = outerClientProjections,
            _aliasForClientProjections = outerAliasForClientProjections,
            _identifier = outerIdentifiers,
            _childIdentifiers = outerChildIdentifiers
        };

        return (outerSelect, sqlRemappingVisitor);

        StructuralTypeProjectionExpression LiftEntityProjectionFromSubquery(
            StructuralTypeProjectionExpression projection,
            string subqueryAlias)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

            HandleTypeProjection(projection);

            void HandleTypeProjection(StructuralTypeProjectionExpression typeProjection)
            {
                foreach (var property in typeProjection.StructuralType.GetAllPropertiesInHierarchy())
                {
                    // json entity projection (i.e. JSON entity that was transformed into query root) may have synthesized keys
                    // but they don't correspond to any columns - we need to skip those
                    if (typeProjection is { StructuralType: IEntityType entityType }
                        && entityType.IsMappedToJson()
                        && property.IsOrdinalKeyProperty())
                    {
                        continue;
                    }

                    var innerColumn = typeProjection.BindProperty(property);
                    var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, innerColumn);
                    innerOuterProjectionMap[innerColumn] = outerColumn;
                    propertyExpressions[property] = outerColumn;
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(typeProjection.StructuralType))
                {
                    HandleTypeProjection(
                        (StructuralTypeProjectionExpression)typeProjection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            ColumnExpression? discriminatorExpression = null;
            if (projection.DiscriminatorExpression != null)
            {
                discriminatorExpression = subquery.GenerateOuterColumn(
                    subqueryAlias, projection.DiscriminatorExpression, DiscriminatorColumnAlias);
                innerOuterProjectionMap[projection.DiscriminatorExpression] = discriminatorExpression;
            }

            var tableMap = projection.TableMap.ToDictionary(kvp => kvp.Key, _ => subqueryAlias);

            var newEntityProjection = new StructuralTypeProjectionExpression(
                projection.StructuralType, propertyExpressions, tableMap, nullable: false, discriminatorExpression);

            if (projection.StructuralType is IEntityType entityType2)
            {
                // Also lift nested entity projections
                foreach (var navigation in entityType2
                             .GetAllBaseTypes().Concat(entityType2.GetDerivedTypesInclusive())
                             .SelectMany(t => t.GetDeclaredNavigations()))
                {
                    var boundEntityShaperExpression = projection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var newValueBufferExpression =
                            boundEntityShaperExpression.ValueBufferExpression is StructuralTypeProjectionExpression
                                innerEntityProjection
                                ? (Expression)LiftEntityProjectionFromSubquery(innerEntityProjection, subqueryAlias)
                                : LiftJsonQueryFromSubquery((JsonQueryExpression)boundEntityShaperExpression.ValueBufferExpression);

                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newValueBufferExpression);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }
            }

            return newEntityProjection;
        }

        JsonQueryExpression LiftJsonQueryFromSubquery(JsonQueryExpression jsonQueryExpression)
        {
            var jsonScalarExpression = new JsonScalarExpression(
                jsonQueryExpression.JsonColumn,
                jsonQueryExpression.Path,
                jsonQueryExpression.JsonColumn.TypeMapping!.ClrType,
                jsonQueryExpression.JsonColumn.TypeMapping,
                jsonQueryExpression.IsNullable);

            var newJsonColumn = subquery.GenerateOuterColumn(subqueryAlias, jsonScalarExpression);

            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var keyProperties = jsonQueryExpression.KeyPropertyMap.Keys.ToList();
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var keyProperty = keyProperties[i];
                var innerColumn = jsonQueryExpression.BindProperty(keyProperty);
                var outerColumn = subquery.GenerateOuterColumn(subqueryAlias, innerColumn);
                innerOuterProjectionMap[innerColumn] = outerColumn;
                newKeyPropertyMap[keyProperty] = outerColumn;
            }

            // clear up the json path - we start from empty path after pushdown
            return new JsonQueryExpression(
                jsonQueryExpression.EntityType,
                newJsonColumn,
                newKeyPropertyMap,
                jsonQueryExpression.Type,
                jsonQueryExpression.IsCollection);
        }
    }

    /// <summary>
    ///     Checks whether this <see cref="SelectExpression" /> represents a <see cref="FromSqlExpression" /> which is not composed upon.
    /// </summary>
    /// <returns>A bool value indicating a non-composed <see cref="FromSqlExpression" />.</returns>
    public bool IsNonComposedFromSql()
        => Limit == null
            && Offset == null
            && !IsDistinct
            && Predicate == null
            && GroupBy.Count == 0
            && Having == null
            && Orderings.Count == 0
            && Tables is [FromSqlExpression fromSql]
            && Projection.All(
                pe => pe.Expression is ColumnExpression column
                    && string.Equals(fromSql.Alias, column.TableAlias, StringComparison.OrdinalIgnoreCase))
            && _projectionMapping.TryGetValue(new ProjectionMember(), out var mapping)
            && mapping.Type == (fromSql.Table == null ? typeof(int) : typeof(Dictionary<IProperty, int>));

    /// <summary>
    ///     Prepares the <see cref="SelectExpression" /> to apply aggregate operation over it.
    /// </summary>
    public SelectExpression PrepareForAggregate(bool liftOrderings = true)
        => IsDistinct || Limit is not null || Offset is not null || _groupBy.Count > 0
            ? PushdownIntoSubqueryInternal2(liftOrderings).Item1
            : this;

    // TODO: Remove
    /// <summary>
    ///     Creates a <see cref="ColumnExpression" /> that references a table on this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="tableExpression">The table expression referenced by the column.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="type">The column CLR type.</param>
    /// <param name="typeMapping">The column's type mapping.</param>
    /// <param name="columnNullable">Whether the column is nullable.</param>
    public ColumnExpression CreateColumnExpression(
        TableExpressionBase tableExpression,
        string columnName,
        Type type,
        RelationalTypeMapping? typeMapping,
        bool? columnNullable = null)
        => new(
            columnName,
            tableExpression.GetRequiredAlias(),
            type.UnwrapNullableType(),
            typeMapping,
            columnNullable ?? type.IsNullableType());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression Clone()
    {
        _cloningExpressionVisitor ??= new CloningExpressionVisitor(_sqlAliasManager);

        return (SelectExpression)_cloningExpressionVisitor.Visit(this);
    }

    /// <inheritdoc />
    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
        => Clone(alias, cloningExpressionVisitor, cloneClientProjections: true);

    private TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor, bool cloneClientProjections)
    {
        var newTables = _tables.Select(cloningExpressionVisitor.Visit).ToList<TableExpressionBase>();
        var tpcTablesMap = _tables.Select(TableExpressionExtensions.UnwrapJoin).Zip(newTables.Select(TableExpressionExtensions.UnwrapJoin))
            .Where(e => e.First is TpcTablesExpression)
            .ToDictionary(e => (TpcTablesExpression)e.First, e => (TpcTablesExpression)e.Second);

        var newProjectionMappings = new Dictionary<ProjectionMember, Expression>(_projectionMapping.Count);
        foreach (var (projectionMember, value) in _projectionMapping)
        {
            newProjectionMappings[projectionMember] = cloningExpressionVisitor.Visit(value);
        }

        var newClientProjections = cloneClientProjections
            ? _clientProjections.Select(p => cloningExpressionVisitor.Visit(p)).ToList()
            : [];

        var newProjections = _projection.Select(cloningExpressionVisitor.Visit).ToList<ProjectionExpression>();

        var predicate = (SqlExpression?)cloningExpressionVisitor.Visit(Predicate);
        var newGroupBy = _groupBy.Select(cloningExpressionVisitor.Visit)
            .Where(e => e is not (SqlConstantExpression or SqlParameterExpression))
            .ToList<SqlExpression>();
        var havingExpression = (SqlExpression?)cloningExpressionVisitor.Visit(Having);
        var newOrderings = _orderings.Select(cloningExpressionVisitor.Visit).ToList<OrderingExpression>();
        var offset = (SqlExpression?)cloningExpressionVisitor.Visit(Offset);
        var limit = (SqlExpression?)cloningExpressionVisitor.Visit(Limit);

        var newSelectExpression = new SelectExpression(
            alias, newTables, newGroupBy, newProjections, newOrderings, Annotations, _sqlAliasManager)
        {
            Predicate = predicate,
            Having = havingExpression,
            Offset = offset,
            Limit = limit,
            IsDistinct = IsDistinct,
            Tags = Tags,
            _projectionMapping = newProjectionMappings,
            _clientProjections = newClientProjections,
            IsMutable = IsMutable
        };

        foreach (var (column, comparer) in _identifier)
        {
            newSelectExpression._identifier.Add(((ColumnExpression)cloningExpressionVisitor.Visit(column), comparer));
        }

        foreach (var (column, comparer) in _childIdentifiers)
        {
            newSelectExpression._childIdentifiers.Add(((ColumnExpression)cloningExpressionVisitor.Visit(column), comparer));
        }

        return newSelectExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // TODO: Look into TPC handling and possibly clean this up, #32873
    [EntityFrameworkInternal]
    public SelectExpression RemoveTpcTableExpression()
        => (SelectExpression)new TpcTableExpressionRemovingExpressionVisitor().Visit(this);

    private Dictionary<ProjectionMember, int> ConvertProjectionMappingToClientProjections(
        Dictionary<ProjectionMember, Expression> projectionMapping,
        bool makeNullable = false)
    {
        var mapping = new Dictionary<ProjectionMember, int>();
        var typeProjectionCache = new Dictionary<StructuralTypeProjectionExpression, int>(ReferenceEqualityComparer.Instance);
        foreach (var projection in projectionMapping)
        {
            var projectionMember = projection.Key;
            var projectionToAdd = projection.Value;

            if (projectionToAdd is StructuralTypeProjectionExpression typeProjection)
            {
                if (!typeProjectionCache.TryGetValue(typeProjection, out var value))
                {
                    var entityProjectionToCache = typeProjection;
                    if (makeNullable)
                    {
                        typeProjection = typeProjection.MakeNullable();
                    }

                    _clientProjections.Add(typeProjection);
                    _aliasForClientProjections.Add(null);
                    value = _clientProjections.Count - 1;
                    typeProjectionCache[entityProjectionToCache] = value;
                }

                mapping[projectionMember] = value;
            }
            else
            {
                projectionToAdd = MakeNullable(projectionToAdd, makeNullable);
                var existingIndex = _clientProjections.FindIndex(e => e.Equals(projectionToAdd));
                if (existingIndex == -1)
                {
                    _clientProjections.Add(projectionToAdd);
                    _aliasForClientProjections.Add(projectionMember.Last?.Name);
                    existingIndex = _clientProjections.Count - 1;
                }

                mapping[projectionMember] = existingIndex;
            }
        }

        projectionMapping.Clear();

        return mapping;
    }

    private static SqlExpression MakeNullable(SqlExpression expression, bool nullable)
        => nullable && expression is ColumnExpression column ? column.MakeNullable() : expression;

    private static Expression MakeNullable(Expression expression, bool nullable)
        => nullable
            ? expression switch
            {
                StructuralTypeProjectionExpression projection => projection.MakeNullable(),
                ColumnExpression column => column.MakeNullable(),
                JsonQueryExpression jsonQueryExpression => jsonQueryExpression.MakeNullable(),
                _ => expression
            }
            : expression;

    private static IEnumerable<IComplexProperty> GetAllComplexPropertiesInHierarchy(ITypeBase structuralType)
        => structuralType switch
        {
            IEntityType entityType => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredComplexProperties()),
            IComplexType complexType => complexType.GetDeclaredComplexProperties(),
            _ => throw new UnreachableException()
        };

    private static ColumnExpression CreateColumnExpression(
        IProperty property,
        ITableBase table,
        string tableAlias,
        bool nullable)
        => CreateColumnExpression(property, table.FindColumn(property)!, tableAlias, nullable);

    private static ColumnExpression CreateColumnExpression(
        IProperty property,
        IColumnBase column,
        string tableAlias,
        bool nullable)
        => new(column.Name,
            tableAlias,
            property.ClrType.UnwrapNullableType(),
            column.PropertyMappings.First(m => m.Property == property).TypeMapping,
            nullable || column.IsNullable);

    private static ColumnExpression CreateColumnExpression(ProjectionExpression subqueryProjection, string tableAlias)
        => new(
            subqueryProjection.Alias,
            tableAlias,
            subqueryProjection.Type,
            subqueryProjection.Expression.TypeMapping!,
            subqueryProjection.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                _ => true
            });

    private ColumnExpression GenerateOuterColumn(
        string tableAlias,
        SqlExpression projection,
        string? columnAlias = null)
    {
        // TODO: Add check if we can add projection in subquery to generate out column
        // Subquery having Distinct or GroupBy can block it.
        var index = AddToProjection(projection, columnAlias);

        return CreateColumnExpression(_projection[index], tableAlias);
    }

    private ColumnExpression GenerateOuterColumn2(
        string tableAlias,
        SqlExpression projection,
        out SelectExpression modifiedSelect,
        string? columnAlias = null)
    {
        // TODO: Add check if we can add projection in subquery to generate out column
        // Subquery having Distinct or GroupBy can block it.

        // TODO: Make this better
        var projections = Projection.ToList();
        var oldProjectionCount = projections.Count;

        var index = AddToProjection2(projections, projection, generateAlias: Alias is not null, columnAlias);

        modifiedSelect = projections.Count == oldProjectionCount
            ? this
            : Update(projections, Tables, Predicate, GroupBy, Having, Orderings, Limit, Offset);

        return CreateColumnExpression(projections[index], tableAlias);
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        if (IsMutable)
        {
            VisitList(_tables, inPlace: true, out _);

            // If projection is not populated then we need to treat this as mutable object since it is not final yet.
            if (_clientProjections.Count > 0)
            {
                VisitList(_clientProjections, inPlace: true, out _);
            }
            else
            {
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    var newProjection = visitor.Visit(expression);

                    projectionMapping[projectionMember] = newProjection;
                }

                _projectionMapping = projectionMapping;
            }

            Predicate = (SqlExpression?)visitor.Visit(Predicate);

            var newGroupBy = _groupBy;
            for (var i = 0; i < _groupBy.Count; i++)
            {
                var groupingKey = _groupBy[i];
                var newGroupingKey = (SqlExpression)visitor.Visit(groupingKey);
                if (newGroupingKey != groupingKey
                    || newGroupingKey is SqlConstantExpression
                    || newGroupingKey is SqlParameterExpression)
                {
                    if (newGroupBy == _groupBy)
                    {
                        newGroupBy = new List<SqlExpression>(_groupBy.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newGroupBy.Add(_groupBy[j]);
                        }
                    }
                }

                if (newGroupBy != _groupBy
                    && newGroupingKey is not (SqlConstantExpression or SqlParameterExpression))
                {
                    newGroupBy.Add(newGroupingKey);
                }
            }

            if (newGroupBy != _groupBy)
            {
                _groupBy.Clear();
                _groupBy.AddRange(newGroupBy);
            }

            Having = (SqlExpression?)visitor.Visit(Having);

            VisitList(_orderings, inPlace: true, out _);

            Offset = (SqlExpression?)visitor.Visit(Offset);
            Limit = (SqlExpression?)visitor.Visit(Limit);

            var identifier = VisitList(_identifier.Select(e => e.Column).ToList(), inPlace: true, out _)
                .Zip(_identifier, (a, b) => (a, b.Comparer))
                .ToList();
            _identifier.Clear();
            _identifier.AddRange(identifier);

            var childIdentifier = VisitList(_childIdentifiers.Select(e => e.Column).ToList(), inPlace: true, out _)
                .Zip(_childIdentifiers, (a, b) => (a, b.Comparer))
                .ToList();
            _childIdentifiers.Clear();
            _childIdentifiers.AddRange(childIdentifier);

            return this;
        }
        else
        {
            var changed = false;

            var newTables = VisitList(_tables, inPlace: false, out var tablesChanged);
            changed |= tablesChanged;

            // If projection is populated then
            // Either this SelectExpression is not bound to a shaped query expression
            // Or it is post-translation phase where it will update the shaped query expression
            // So we will treat it as immutable
            var newProjections = VisitList(_projection, inPlace: false, out var projectionChanged);
            changed |= projectionChanged;

            // We don't need to visit _clientProjection/_projectionMapping here
            // because once projection is populated both of them contains expressions for client binding rather than a server query.

            var predicate = (SqlExpression?)visitor.Visit(Predicate);
            changed |= predicate != Predicate;

            var newGroupBy = _groupBy;
            for (var i = 0; i < _groupBy.Count; i++)
            {
                var groupingKey = _groupBy[i];
                var newGroupingKey = (SqlExpression)visitor.Visit(groupingKey);
                if (newGroupingKey != groupingKey
                    || newGroupingKey is SqlConstantExpression
                    || newGroupingKey is SqlParameterExpression)
                {
                    if (newGroupBy == _groupBy)
                    {
                        newGroupBy = new List<SqlExpression>(_groupBy.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newGroupBy.Add(_groupBy[j]);
                        }
                    }

                    changed = true;
                }

                if (newGroupBy != _groupBy
                    && newGroupingKey is not (SqlConstantExpression or SqlParameterExpression))
                {
                    newGroupBy.Add(newGroupingKey);
                }
            }

            var havingExpression = (SqlExpression?)visitor.Visit(Having);
            changed |= havingExpression != Having;

            var newOrderings = VisitList(_orderings, inPlace: false, out var orderingChanged);
            changed |= orderingChanged;

            var offset = (SqlExpression?)visitor.Visit(Offset);
            changed |= offset != Offset;

            var limit = (SqlExpression?)visitor.Visit(Limit);
            changed |= limit != Limit;

            var identifier = VisitList(_identifier.Select(e => e.Column).ToList(), inPlace: false, out var identifierChanged);
            changed |= identifierChanged;

            var childIdentifier = VisitList(
                _childIdentifiers.Select(e => e.Column).ToList(), inPlace: false, out var childIdentifierChanged);
            changed |= childIdentifierChanged;

            if (changed)
            {
                var newSelectExpression = new SelectExpression(
                    Alias, newTables, newGroupBy, newProjections, newOrderings, Annotations, _sqlAliasManager)
                {
                    _clientProjections = _clientProjections,
                    _projectionMapping = _projectionMapping,
                    Predicate = predicate,
                    Having = havingExpression,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct,
                    Tags = Tags,
                    IsMutable = false
                };

                newSelectExpression._identifier.AddRange(identifier.Zip(_identifier).Select(e => (e.First, e.Second.Comparer)));
                newSelectExpression._childIdentifiers.AddRange(
                    childIdentifier.Zip(_childIdentifiers).Select(e => (e.First, e.Second.Comparer)));

                return newSelectExpression;
            }

            return this;
        }

        List<T> VisitList<T>(List<T> list, bool inPlace, out bool changed)
            where T : Expression
        {
            changed = false;
            var newList = list;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var newItem = item is ShapedQueryExpression shapedQueryExpression
                    ? shapedQueryExpression.UpdateQueryExpression(visitor.Visit(shapedQueryExpression.QueryExpression))
                    : visitor.Visit(item);
                if (newItem != item
                    && newList == list)
                {
                    newList = new List<T>(list.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newList.Add(list[j]);
                    }

                    changed = true;
                }

                if (newList != list)
                {
                    newList.Add((T)newItem);
                }
            }

            if (inPlace
                && changed)
            {
                list.Clear();
                list.AddRange(newList);

                return list;
            }

            return newList;
        }
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="projections">The <see cref="Projection" /> property of the result.</param>
    /// <param name="tables">The <see cref="Tables" /> property of the result.</param>
    /// <param name="predicate">The <see cref="Predicate" /> property of the result.</param>
    /// <param name="groupBy">The <see cref="GroupBy" /> property of the result.</param>
    /// <param name="having">The <see cref="Having" /> property of the result.</param>
    /// <param name="orderings">The <see cref="Orderings" /> property of the result.</param>
    /// <param name="limit">The <see cref="Limit" /> property of the result.</param>
    /// <param name="offset">The <see cref="Offset" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    // This does not take internal states since when using this method SelectExpression should be finalized
    public SelectExpression Update(
        IReadOnlyList<ProjectionExpression> projections,
        IReadOnlyList<TableExpressionBase> tables,
        SqlExpression? predicate,
        IReadOnlyList<SqlExpression> groupBy,
        SqlExpression? having,
        IReadOnlyList<OrderingExpression> orderings,
        SqlExpression? limit,
        SqlExpression? offset)
        => projections == Projection
            && tables == Tables
            && predicate == Predicate
            && groupBy == GroupBy
            && having == Having
            && orderings == Orderings
            && limit == Limit
            && offset == Offset
                ? this
                : new SelectExpression(
                    Alias, tables.ToList(), groupBy.ToList(), projections.ToList(), orderings.ToList(), Annotations, _sqlAliasManager)
                {
                    Predicate = predicate,
                    Having = having,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct,
                    Tags = Tags,
                    IsMutable = IsMutable,
                    _projectionMapping = _projectionMapping,
                    _clientProjections = _clientProjections,
                    _identifier = _identifier,
                    _childIdentifiers = _childIdentifiers,
                    _aliasForClientProjections = _aliasForClientProjections,
                    _preGroupByIdentifier = _preGroupByIdentifier
                };

    /// <summary>
    ///     Returns a copy of this <see cref="SelectExpression" />, replacing the orderings with the provided ones.
    /// </summary>
    public SelectExpression WithTables(IReadOnlyList<TableExpressionBase> tables)
        => Update(Projection, tables, Predicate, GroupBy, Having, Orderings, Limit, Offset);

    /// <summary>
    ///     Returns a copy of this <see cref="SelectExpression" />, replacing the orderings with the provided ones.
    /// </summary>
    public SelectExpression WithOrderings(IReadOnlyList<OrderingExpression> orderings)
        => Update(Projection, Tables, Predicate, GroupBy, Having, orderings, Limit, Offset);

    /// <summary>
    ///     Returns a copy of this <see cref="SelectExpression" />, replacing the projections with the provided ones.
    /// </summary>
    public SelectExpression WithLimit(SqlExpression limit)
        => Update(Projection, Tables, Predicate, GroupBy, Having, Orderings, limit, Offset);

    /// <summary>
    ///     Returns a copy of this <see cref="SelectExpression" />, replacing the projections with the provided ones.
    /// </summary>
    public SelectExpression WithProjections(IReadOnlyList<ProjectionExpression> projections)
        => Update(projections, Tables, Predicate, GroupBy, Having, Orderings, Limit, Offset);

    /// <inheritdoc />
    protected override SelectExpression WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => throw new UnreachableException();

    /// <inheritdoc />
    public override SelectExpression WithAlias(string newAlias)
        => new(newAlias, _tables, _groupBy, _projection, _orderings, Annotations, _sqlAliasManager)
        {
            Predicate = Predicate,
            Having = Having,
            Offset = Offset,
            Limit = Limit,
            IsDistinct = IsDistinct,
            Tags = Tags,
            IsMutable = IsMutable,
            _projectionMapping = _projectionMapping,
            _clientProjections = _clientProjections,
            _identifier = _identifier,
            _childIdentifiers = _childIdentifiers,
            _aliasForClientProjections = _aliasForClientProjections,
            _preGroupByIdentifier = _preGroupByIdentifier
        };

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        PrintProjections(expressionPrinter);
        expressionPrinter.AppendLine();
        PrintSql(expressionPrinter);
    }

    private void PrintProjections(ExpressionPrinter expressionPrinter)
    {
        if (_clientProjections.Count > 0)
        {
            expressionPrinter.AppendLine("Client Projections:");
            using (expressionPrinter.Indent())
            {
                for (var i = 0; i < _clientProjections.Count; i++)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(i.ToString()).Append(" -> ");
                    expressionPrinter.Visit(_clientProjections[i]);
                }
            }
        }
        else if (_projectionMapping.Count > 0)
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
            foreach (var tag in Tags)
            {
                expressionPrinter.Append($"-- {tag}");
            }
        }

        IDisposable? indent = null;

        if (Alias != null)
        {
            expressionPrinter.AppendLine("(");
            indent = expressionPrinter.Indent();
        }

        expressionPrinter.Append("SELECT ");

        if (IsDistinct)
        {
            expressionPrinter.Append("DISTINCT ");
        }

        if (Limit != null
            && Offset == null)
        {
            expressionPrinter.Append("TOP(");
            expressionPrinter.Visit(Limit);
            expressionPrinter.Append(") ");
        }

        if (Projection.Any())
        {
            expressionPrinter.VisitCollection(Projection);
        }
        else
        {
            expressionPrinter.Append("1");
        }

        if (Tables.Any())
        {
            expressionPrinter.AppendLine().Append("FROM ");

            expressionPrinter.VisitCollection(Tables, p => p.AppendLine());
        }

        if (Predicate != null)
        {
            expressionPrinter.AppendLine().Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }

        if (GroupBy.Any())
        {
            expressionPrinter.AppendLine().Append("GROUP BY ");
            expressionPrinter.VisitCollection(GroupBy);
        }

        if (Having != null)
        {
            expressionPrinter.AppendLine().Append("HAVING ");
            expressionPrinter.Visit(Having);
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

        PrintAnnotations(expressionPrinter);

        if (Alias != null)
        {
            indent?.Dispose();
            expressionPrinter.AppendLine().Append(") AS " + Alias);
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

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SelectExpression selectExpression
                && Equals(selectExpression));

    // Note that we vary our Equals/GetHashCode logic based on whether the SelectExpression is mutable or not; in the former case we use
    // reference logic, whereas once the expression becomes immutable (after translation), we switch to value logic.
    // This isn't a good state of affairs (e.g. it's impossible to keep a SelectExpression - or any expression containing one - as a
    // dictionary key across the state change from mutable to immutable (we fortunately don't do that).
    private bool Equals(SelectExpression selectExpression)
        => IsMutable
            ? ReferenceEquals(this, selectExpression)
            : base.Equals(selectExpression)
            && Tables.SequenceEqual(selectExpression.Tables)
            && (Predicate is null && selectExpression.Predicate is null
                || Predicate is not null && Predicate.Equals(selectExpression.Predicate))
            && GroupBy.SequenceEqual(selectExpression.GroupBy)
            && (Having is null && selectExpression.Having is null
                || Having is not null && Having.Equals(selectExpression.Having))
            && Projection.SequenceEqual(selectExpression.Projection)
            && Orderings.SequenceEqual(selectExpression.Orderings)
            && (Limit is null && selectExpression.Limit is null
                || Limit is not null && Limit.Equals(selectExpression.Limit))
            && (Offset is null && selectExpression.Offset is null
                || Offset is not null && Offset.Equals(selectExpression.Offset));

    // ReSharper disable NonReadonlyMemberInGetHashCode
    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (IsMutable)
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        var hash = new HashCode();
        hash.Add(base.GetHashCode());

        foreach (var table in Tables)
        {
            hash.Add(table);
        }

        if (Predicate is not null)
        {
            hash.Add(Predicate);
        }

        foreach (var groupingKey in GroupBy)
        {
            hash.Add(groupingKey);
        }

        if (Having is not null)
        {
            hash.Add(Having);
        }

        foreach (var projection in Projection)
        {
            hash.Add(projection);
        }

        foreach (var ordering in Orderings)
        {
            hash.Add(ordering);
        }

        if (Limit is not null)
        {
            hash.Add(Limit);
        }

        if (Offset is not null)
        {
            hash.Add(Offset);
        }

        return hash.ToHashCode();

    }
    // ReSharper restore NonReadonlyMemberInGetHashCode
}
