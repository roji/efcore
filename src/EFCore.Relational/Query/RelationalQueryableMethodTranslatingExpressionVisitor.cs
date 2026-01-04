// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public partial class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    private const string SqlQuerySingleColumnAlias = "Value";

    private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
    private readonly RelationalQueryCompilationContext _queryCompilationContext;
    private readonly RelationalTranslationContext _translationContext;
    private readonly SqlAliasManager _sqlAliasManager;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _subquery;
    private readonly ParameterTranslationMode _collectionParameterTranslationMode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string ValuesOrderingColumnName = "_ord";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string ValuesValueColumnName = "Value";

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        RelationalDependencies = relationalDependencies;

        var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        _queryCompilationContext = queryCompilationContext;
        _translationContext = new RelationalTranslationContext();
        _sqlAliasManager = queryCompilationContext.SqlAliasManager;
        _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(
            queryCompilationContext, _translationContext, this);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _subquery = false;
        _collectionParameterTranslationMode =
            RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).ParameterizedCollectionMode;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryableMethodTranslatingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="parentVisitor">A parent visitor to create subquery visitor for.</param>
    protected RelationalQueryableMethodTranslatingExpressionVisitor(
        RelationalQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
    {
        RelationalDependencies = parentVisitor.RelationalDependencies;
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _translationContext = parentVisitor._translationContext;
        _sqlAliasManager = _queryCompilationContext.SqlAliasManager;
        _sqlTranslator = RelationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(
            parentVisitor._queryCompilationContext, _translationContext, parentVisitor);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _subquery = true;
        _collectionParameterTranslationMode = RelationalOptionsExtension.Extract(parentVisitor._queryCompilationContext.ContextOptions)
            .ParameterizedCollectionMode;
    }

    public override Expression Translate(Expression expression)
    {
        var translation = base.Translate(expression);

        // TODO: Move this out to postprocessing; the challenge is that this requires both queryable method translator
        // and SQL translator...
        if (!_subquery && translation != QueryCompilationContext.NotTranslatedExpression)
        {
            var shapedQuery = (ShapedQueryExpression)translation;

            translation = new IncludeProcessor(this).ProcessIncludes(shapedQuery);
        }

        return translation;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case FromSqlQueryRootExpression fromSqlQueryRootExpression:
            {
                var table = fromSqlQueryRootExpression.EntityType.GetDefaultMappings().Single().Table;
                var alias = _sqlAliasManager.GenerateTableAlias(table);

                return CreateShapedQueryExpression(
                    fromSqlQueryRootExpression.EntityType,
                    CreateSelect(
                        fromSqlQueryRootExpression.EntityType,
                        new FromSqlExpression(alias, table, fromSqlQueryRootExpression.Sql, fromSqlQueryRootExpression.Argument)));
            }

            case TableValuedFunctionQueryRootExpression tableValuedFunctionQueryRootExpression:
            {
                var function = tableValuedFunctionQueryRootExpression.Function;
                var arguments = new List<SqlExpression>();
                foreach (var arg in tableValuedFunctionQueryRootExpression.Arguments)
                {
                    var sqlArgument = TranslateExpression(arg);
                    if (sqlArgument == null)
                    {
                        string call;
                        var methodInfo = function.DbFunctions.Last().MethodInfo;
                        if (methodInfo != null)
                        {
                            var methodCall = Expression.Call(
                                // Declaring types would be derived db context.
                                Expression.Default(methodInfo.DeclaringType!),
                                methodInfo,
                                tableValuedFunctionQueryRootExpression.Arguments);

                            call = methodCall.Print();
                        }
                        else
                        {
                            call = $"{function.DbFunctions.Last().Name}()";
                        }

                        throw new InvalidOperationException(
                            TranslationErrorDetails == null
                                ? CoreStrings.TranslationFailed(call)
                                : CoreStrings.TranslationFailedWithDetails(call, TranslationErrorDetails));
                    }

                    arguments.Add(sqlArgument);
                }

                var entityType = tableValuedFunctionQueryRootExpression.EntityType;
                var alias = _sqlAliasManager.GenerateTableAlias(function);
                var translation = new TableValuedFunctionExpression(alias, function, arguments);
                var queryExpression = CreateSelect(entityType, translation);

                return CreateShapedQueryExpression(entityType, queryExpression);
            }

            case EntityQueryRootExpression entityQueryRootExpression
                when entityQueryRootExpression.GetType() == typeof(EntityQueryRootExpression)
                && entityQueryRootExpression.EntityType.GetSqlQueryMappings().FirstOrDefault(m => m.IsDefaultSqlQueryMapping)?.SqlQuery is
                    { } sqlQuery:
            {
                var table = entityQueryRootExpression.EntityType.GetDefaultMappings().Single().Table;
                var alias = _sqlAliasManager.GenerateTableAlias(table);

                return CreateShapedQueryExpression(
                    entityQueryRootExpression.EntityType,
                    CreateSelect(
                        entityQueryRootExpression.EntityType,
                        new FromSqlExpression(alias, table, sqlQuery.Sql, Expression.Constant(Array.Empty<object>(), typeof(object[])))));
            }

            case GroupByShaperExpression groupByShaperExpression:
                var groupShapedQueryExpression = groupByShaperExpression.GroupingEnumerable;
                var groupClonedSelectExpression = ((SelectExpression)groupShapedQueryExpression.QueryExpression).Clone();
                return new ShapedQueryExpression(
                    groupClonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(
                            groupShapedQueryExpression.QueryExpression, groupClonedSelectExpression)
                        .Visit(groupShapedQueryExpression.ShaperExpression));

            case ShapedQueryExpression shapedQueryExpression:
                var clonedSelectExpression = ((SelectExpression)shapedQueryExpression.QueryExpression).Clone();
                return new ShapedQueryExpression(
                    clonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(shapedQueryExpression.QueryExpression, clonedSelectExpression)
                        .Visit(shapedQueryExpression.ShaperExpression));

            // TODO: Weird, specifically there for e.g. Where_collection_navigation_ToArray_Count - Select of a ToList() over collection
            // (which produces a CollectionResultExpression) followed by a regular operator like Where
            case CollectionResultExpression { QueryExpression: ProjectionBindingExpression projectionBinding } collectionResult:
                return Visit(
                    (ShapedQueryExpression)((SelectExpression)projectionBinding.QueryExpression).GetProjection(projectionBinding));

            case SqlQueryRootExpression sqlQueryRootExpression:
            {
                var typeMapping = RelationalDependencies.TypeMappingSource.FindMapping(
                    sqlQueryRootExpression.ElementType, RelationalDependencies.Model);

                if (typeMapping == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.SqlQueryUnmappedType(sqlQueryRootExpression.ElementType.DisplayName()));
                }

                var alias = _sqlAliasManager.GenerateTableAlias("sql");
                var selectExpression = new SelectExpression(
                    [new FromSqlExpression(alias, sqlQueryRootExpression.Sql, sqlQueryRootExpression.Argument)],
                    new ColumnExpression(
                        SqlQuerySingleColumnAlias,
                        alias,
                        sqlQueryRootExpression.Type.UnwrapNullableType(),
                        typeMapping,
                        sqlQueryRootExpression.Type.IsNullableType()),
                    identifier: [],
                    _sqlAliasManager);

                Expression shaperExpression = new ProjectionBindingExpression(
                    selectExpression, new ProjectionMember(), sqlQueryRootExpression.ElementType.MakeNullable());

                if (sqlQueryRootExpression.ElementType != shaperExpression.Type)
                {
                    Check.DebugAssert(
                        sqlQueryRootExpression.ElementType.MakeNullable() == shaperExpression.Type,
                        "expression.Type must be nullable of targetType");

                    shaperExpression = Expression.Convert(shaperExpression, sqlQueryRootExpression.ElementType);
                }

                return new ShapedQueryExpression(selectExpression, shaperExpression);
            }

            case JsonQueryExpression jsonQueryExpression:
                return TransformJsonQueryToTable(jsonQueryExpression) ?? base.VisitExtension(extensionExpression);

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        if (method.DeclaringType == typeof(RelationalQueryableMethodTranslatingExpressionVisitor)
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == _fakeDefaultIfEmptyMethodInfo.Value
            && Visit(methodCallExpression.Arguments[0]) is ShapedQueryExpression source)
        {
            ((SelectExpression)source.QueryExpression).MakeProjectionNullable(_sqlExpressionFactory, source.ShaperExpression.Type.IsNullableType());
            return source.UpdateShaperExpression(MarkShaperNullable(source.ShaperExpression));
        }

        var translated = base.VisitMethodCall(methodCallExpression);

        // For Contains over a collection parameter, if the provider hasn't implemented TranslateCollection (e.g. OPENJSON on SQL
        // Server), we need to fall back to the previous IN translation.
        if (translated == QueryCompilationContext.NotTranslatedExpression
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == QueryableMethods.Contains
            && methodCallExpression.Arguments[0] is ParameterQueryRootExpression parameterSource
            && TranslateExpression(methodCallExpression.Arguments[1]) is { } item
            && _sqlTranslator.Visit(parameterSource.QueryParameterExpression) is SqlParameterExpression sqlParameterExpression
            && (parameterSource.QueryParameterExpression.TranslationMode is ParameterTranslationMode.Constant
                or null))
        {
            var inExpression = _sqlExpressionFactory.In(item, sqlParameterExpression);
            var selectExpression = new SelectExpression(inExpression, _sqlAliasManager);
            var shaperExpression = Expression.Convert(
                new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool));
            var shapedQueryExpression = new ShapedQueryExpression(selectExpression, shaperExpression)
                .UpdateResultCardinality(ResultCardinality.Single);
            return shapedQueryExpression;
        }

        return translated;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMemberAccess(Expression source, MemberIdentity member)
    {
        // Attempt to translate access into a collection property (scalar collection, complex collection or collection navigation)
        if (_sqlTranslator.TryBindMember(_sqlTranslator.Visit(source), member, out var translatedExpression, out var property))
        {
            switch (property)
            {
                // Complex property or owned navigation pointing to a JSON document.
                // Transform the JsonQueryExpression representing the JSON document into a table/rowset, e.g. via SQL Server OPENJSON.
                case IComplexProperty { IsCollection: true, ComplexType: var complexType }:
                case INavigationBase { IsCollection: true } navigation when navigation.TargetEntityType.IsMappedToJson():
                    var jsonQuery = (JsonQueryExpression)((CollectionResultExpression)translatedExpression).QueryExpression;
                    return TransformJsonQueryToTable(jsonQuery);

                // TODO: non-collection regular navigation?
                // Regular (JOIN-based) collection navigation.
                case INavigationBase { IsCollection: true }:
                    return (ShapedQueryExpression)((CollectionResultExpression)translatedExpression).QueryExpression;

                // Scalar/primitive collection.
                // Transform the scalar representing the JSON collection into a table/rowset, e.g. via SQL Server OPENJSON.
                case IProperty { IsPrimitiveCollection: true } scalarProperty
                    when translatedExpression is SqlExpression sqlExpression
                    && TranslatePrimitiveCollection(
                        sqlExpression,
                        scalarProperty,
                        _sqlAliasManager.GenerateTableAlias(GenerateTableAlias(sqlExpression))) is { } primitiveCollectionTranslation:
                {
                    return primitiveCollectionTranslation;
                }

                default:
                    throw new UnreachableException();
            }
        }

        // Scalar subquery terminating with List<>.Length.
        // TODO: Also array.Length
        // TODO: Any other collection types? Is that actually supported? any Count on a type that extends ICollection<>?
        if (member.MemberInfo is { Name: "Count", DeclaringType: Type declaringType }
            && declaringType.IsGenericType
            && declaringType.GetGenericTypeDefinition() is var declaringTypeDefinition
            && (declaringTypeDefinition == typeof(List<>) || declaringTypeDefinition == typeof(ICollection<>))
            && Visit(source) is ShapedQueryExpression shapedQuery)
            // && Visit(source) is CollectionResultExpression { QueryExpression: ShapedQueryExpression shapedQuery })
        {
            shapedQuery = shapedQuery.UpdateResultCardinality(ResultCardinality.Single);
            return TranslateCount(shapedQuery, predicate: null);
        }

        return null;

        string GenerateTableAlias(SqlExpression sqlExpression)
            => sqlExpression switch
            {
                ColumnExpression c => c.Name,
                JsonScalarExpression jsonScalar
                    => jsonScalar.Path.Select(s => s.PropertyName).LastOrDefault()
                    ?? GenerateTableAlias(jsonScalar.Json),
                ScalarSubqueryExpression scalarSubquery => scalarSubquery.Subquery.Projection[0].Alias,

                _ => "collection"
            };
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateParameterQueryRoot(ParameterQueryRootExpression parameterQueryRootExpression)
    {
        var queryParameter = parameterQueryRootExpression.QueryParameterExpression;
        var sqlParameterExpression = _sqlTranslator.Visit(queryParameter) as SqlParameterExpression;

        Check.DebugAssert(sqlParameterExpression is not null);

        var tableAlias = _sqlAliasManager.GenerateTableAlias(sqlParameterExpression.Name.TrimStart('_'));

        return (queryParameter.TranslationMode ?? _collectionParameterTranslationMode) switch
        {
            ParameterTranslationMode.Constant or ParameterTranslationMode.MultipleParameters
                => CreateShapedQueryExpressionForValuesExpression(
                    new ValuesExpression(
                        tableAlias,
                        sqlParameterExpression,
                        [ValuesOrderingColumnName, ValuesValueColumnName]),
                    tableAlias,
                    parameterQueryRootExpression.ElementType,
                    sqlParameterExpression.TypeMapping,
                    sqlParameterExpression.IsNullable),

            ParameterTranslationMode.Parameter
                => TranslatePrimitiveCollection(sqlParameterExpression, property: null, tableAlias),

            _ => throw new UnreachableException()
        };
    }

    /// <summary>
    ///     Translates a parameter or column collection of primitive values. Providers can override this to translate e.g. int[] columns or
    ///     parameters to a queryable table (OPENJSON on SQL Server, unnest on PostgreSQL...). The default implementation always returns
    ///     <see langword="null" /> (no translation).
    /// </summary>
    /// <remarks>
    ///     Inline collections aren't passed to this method; see <see cref="TranslateInlineQueryRoot" /> for the translation of inline
    ///     collections.
    /// </remarks>
    /// <param name="sqlExpression">The expression to try to translate as a primitive collection expression.</param>
    /// <param name="property">
    ///     If the primitive collection is a property, contains the <see cref="IProperty" /> for that property. Otherwise, the collection
    ///     represents a parameter, and this contains <see langword="null" />.
    /// </param>
    /// <param name="tableAlias">
    ///     Provides an alias to be used for the table returned from translation, which will represent the collection.
    /// </param>
    /// <returns>A <see cref="ShapedQueryExpression" /> if the translation was successful, otherwise <see langword="null" />.</returns>
    protected virtual ShapedQueryExpression? TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
        string tableAlias)
        => null;

    /// <summary>
    ///     Invoked when LINQ operators are composed over a collection within a JSON document.
    ///     Transforms the provided <see cref="JsonQueryExpression" /> - representing access to the collection - into a provider-specific
    ///     means to expand the JSON array into a relational table/rowset (e.g. SQL Server OPENJSON).
    /// </summary>
    /// <param name="jsonQueryExpression">The <see cref="JsonQueryExpression" /> referencing the JSON array.</param>
    /// <returns>A <see cref="ShapedQueryExpression" /> if the translation was successful, otherwise <see langword="null" />.</returns>
    protected virtual ShapedQueryExpression? TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        AddTranslationErrorDetails(RelationalStrings.JsonQueryLinqOperatorsNotSupported);
        return null;
    }

    /// <summary>
    ///     Translates an inline collection into a queryable SQL VALUES expression.
    /// </summary>
    /// <param name="inlineQueryRootExpression">The inline collection to be translated.</param>
    /// <returns>A queryable SQL VALUES expression.</returns>
    protected override ShapedQueryExpression? TranslateInlineQueryRoot(InlineQueryRootExpression inlineQueryRootExpression)
    {
        var elementType = inlineQueryRootExpression.ElementType;

        var encounteredNull = false;
        var intTypeMapping = _typeMappingSource.FindMapping(typeof(int), RelationalDependencies.Model);
        RelationalTypeMapping? inferredTypeMaping = null;
        var sqlExpressions = new SqlExpression[inlineQueryRootExpression.Values.Count];

        // Do a first pass, translating the elements and inferring type mappings/nullability.
        for (var i = 0; i < inlineQueryRootExpression.Values.Count; i++)
        {
            // Note that we specifically don't apply the default type mapping to the translation, to allow it to get inferred later based
            // on usage.
            if (TranslateExpression(inlineQueryRootExpression.Values[i], applyDefaultTypeMapping: false)
                is not { } translatedValue)
            {
                return null;
            }

            // Infer the type mapping from the different inline elements, applying the type mapping of a column to constants/parameters, and
            // also to the projection of the VALUES expression as a whole.
            // TODO: This currently picks up the first type mapping; we can do better once we have a type compatibility chart (#15586)
            // TODO: See similarity with SqlExpressionFactory.ApplyTypeMappingOnIn()
            inferredTypeMaping ??= translatedValue.TypeMapping;

            // TODO: Poor man's null semantics: in SqlNullabilityProcessor we don't fully handle the nullability of SelectExpression
            // projections. Whether the SelectExpression's projection is nullable or not is determined here in translation, but at this
            // point we don't know how to properly calculate nullability (and can't look at parameters).
            // So for now, we assume the projected column is nullable if we see anything but non-null constants and non-nullable columns.
            encounteredNull |=
                translatedValue is not SqlConstantExpression { Value: not null } and not ColumnExpression { IsNullable: false };

            sqlExpressions[i] = translatedValue;
        }

        // Second pass: create the VALUES expression's row value expressions.
        var rowExpressions = new RowValueExpression[sqlExpressions.Length];
        for (var i = 0; i < sqlExpressions.Length; i++)
        {
            var sqlExpression = sqlExpressions[i];
            rowExpressions[i] =
                new RowValueExpression(
                [
                    // Since VALUES may not guarantee row ordering, we add an _ord value by which we'll order.
                    _sqlExpressionFactory.Constant(i, intTypeMapping),
                    // If no type mapping was inferred (i.e. no column in the inline collection), it's left null, to allow it to get
                    // inferred later based on usage. Note that for the element in the VALUES expression, we'll also apply an explicit
                    // CONVERT to make sure the database gets the right type (see
                    // RelationalTypeMappingPostprocessor.ApplyTypeMappingsOnValuesExpression)
                    sqlExpression.TypeMapping is null && inferredTypeMaping is not null
                        ? _sqlExpressionFactory.ApplyTypeMapping(sqlExpression, inferredTypeMaping)
                        : sqlExpression
                ]);
        }

        var alias = _sqlAliasManager.GenerateTableAlias("values");
        var valuesExpression = new ValuesExpression(alias, rowExpressions, [ValuesOrderingColumnName, ValuesValueColumnName]);

        return CreateShapedQueryExpressionForValuesExpression(
            valuesExpression,
            alias,
            elementType,
            inferredTypeMaping,
            encounteredNull);
    }

    [EntityFrameworkInternal]
    public virtual RelationalQueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor2()
        => CreateSubqueryVisitor();

    /// <inheritdoc />
    protected override RelationalQueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new RelationalQueryableMethodTranslatingExpressionVisitor(this);

    /// <inheritdoc />
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
        => CreateShapedQueryExpression(entityType, CreateSelect(entityType));

    [EntityFrameworkInternal] // TODO
    public ShapedQueryExpression CreateShapedQueryExpression2(IEntityType entityType)
        => CreateShapedQueryExpression(entityType);

    private ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
    {
        var shaper = new RelationalStructuralTypeShaperExpression(
            entityType,
            new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(ValueBuffer)),
            nullable: false);

        // TODO: Add query filters

        AddAutoIncludes(shaper.IncludeTree);

        return new ShapedQueryExpression(selectExpression, shaper);

        void AddAutoIncludes(IncludeTreeNode includeTreeNode)
        {
            var autoIncludedNavigations = includeTreeNode.EntityType.GetNavigations()
                // TODO
                // .Cast<INavigationBase>()
                // .Concat(entityType.GetSkipNavigations())
                // .Concat(entityType.GetDerivedNavigations())
                // .Concat(entityType.GetDerivedSkipNavigations())
                .Where(n => n.IsEagerLoaded);

            // If ignoring auto-includes, filter everything except owned navigations, which we always auto-include.
            if (_queryCompilationContext.IgnoreAutoIncludes)
            {
                autoIncludedNavigations = autoIncludedNavigations.Where(n => n is INavigation { ForeignKey.IsOwnership: true });
            }

            foreach (var navigation in autoIncludedNavigations)
            {
                var innerIncludeNode = new IncludeTreeNode(navigation.TargetEntityType, setLoaded: true /* TODO? */);

                AddAutoIncludes(innerIncludeNode);

                includeTreeNode.Add(navigation, innerIncludeNode);
            }
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        var subquery = (SelectExpression)source.QueryExpression;

        // Negate the predicate, unless it's already negated, in which case remove that.
        subquery.ApplyPredicate(
            translation is SqlUnaryExpression { OperatorType: ExpressionType.Not, Operand: var nestedOperand }
                ? nestedOperand
                : _sqlExpressionFactory.Not(translation));

        subquery.ReplaceProjection(new List<Expression>());
        subquery.ApplyProjection();
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        subquery.IsDistinct = false;

        translation = _sqlExpressionFactory.Not(_sqlExpressionFactory.Exists(subquery));
        subquery = new SelectExpression(translation, _sqlAliasManager);

        return source.Update(
            subquery,
            Expression.Convert(new ProjectionBindingExpression(subquery, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var subquery = (SelectExpression)source.QueryExpression;
        subquery.ReplaceProjection(new List<Expression>());
        subquery.ApplyProjection();
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        subquery.IsDistinct = false;

        var translation = _sqlExpressionFactory.Exists(subquery);
        var selectExpression = new SelectExpression(translation, _sqlAliasManager);

        return source.Update(
            selectExpression,
            Expression.Convert(new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetAverageWithoutSelector, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType)
        => source.ShaperExpression.Type != resultType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, resultType))
            : source;

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: false);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        // Note that we don't apply the default type mapping to the item in order to allow it to be inferred from e.g. the subquery
        // projection on the other side.
        if (TranslateExpression(source, item, applyDefaultTypeMapping: false) is not { } translatedItem
            || !TryGetProjection(source, out var projection))
        {
            // If the item can't be translated, we can't translate to an IN expression.

            // We do attempt one thing: if this is a contains over an entity type which has a single key property (non-composite key),
            // we can project its key property (entity equality/containment) and translate to InExpression over that.
            if (item is StructuralTypeShaperExpression { StructuralType: IEntityType entityType }
                && entityType.FindPrimaryKey()?.Properties is [var singleKeyProperty])
            {
                var keySelectorParam = Expression.Parameter(source.Type);

                return TranslateContains(
                    TranslateSelect(
                        source,
                        Expression.Lambda(keySelectorParam.CreateEFPropertyExpression(singleKeyProperty), keySelectorParam)),
                    item.CreateEFPropertyExpression(singleKeyProperty));
            }

            // Otherwise, attempt to translate as Any since that passes through Where predicate translation. This will e.g. take care of
            // entity, which e.g. does entity equality/containment for entities with composite keys.
            var anyLambdaParameter = Expression.Parameter(item.Type, "p");
            var anyLambda = Expression.Lambda(
                Infrastructure.ExpressionExtensions.CreateEqualsExpression(anyLambdaParameter, item),
                anyLambdaParameter);

            return TranslateAny(source, anyLambda);
        }

        // Pattern-match Contains over ValuesExpression, translating to simplified 'item IN (1, 2, 3)' with constant elements.
        if (TryExtractBareInlineCollectionValues(source, out var values, out var valuesParameter))
        {
            var inExpression = (values, valuesParameter) switch
            {
                (not null, null) => _sqlExpressionFactory.In(translatedItem, values),
                (null, not null) => _sqlExpressionFactory.In(translatedItem, valuesParameter),
                _ => throw new UnreachableException(),
            };
            return source.Update(new SelectExpression(inExpression, _sqlAliasManager), source.ShaperExpression);
        }

        // Translate to IN with a subquery.
        // Note that because of null semantics, this may get transformed to an EXISTS subquery in SqlNullabilityProcessor.
        var subquery = (SelectExpression)source.QueryExpression;
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        subquery.IsDistinct = false;

        subquery.ReplaceProjection(new List<Expression> { projection });
        subquery.ApplyProjection();

        var translation = _sqlExpressionFactory.In(translatedItem, subquery);
        subquery = new SelectExpression(translation, _sqlAliasManager);

        return source.Update(
            subquery,
            Expression.Convert(
                new ProjectionBindingExpression(subquery, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.CountWithoutPredicate, liftOrderings: false);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
    {
        if (defaultValue == null)
        {
            ((SelectExpression)source.QueryExpression).ApplyDefaultIfEmpty(_sqlExpressionFactory, source.ShaperExpression.Type.IsNullableType());
            return source.UpdateShaperExpression(MarkShaperNullable(source.ShaperExpression));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;

        if (selectExpression is { Orderings.Count: > 0, Limit: null, Offset: null }
            && !IsNaturallyOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.DistinctAfterOrderByWithoutRowLimitingOperatorWarning();
        }

        selectExpression.ApplyDistinct();
        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(source, index);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyOffset(translation);
        ApplyLimit(selectExpression, TranslateExpression(source, Expression.Constant(1))!);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyExcept((SelectExpression)source2.QueryExpression, distinct: true);

        // Since except has result from source1, we don't need to change shaper
        return source1;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Predicate == null
            && selectExpression.Orderings.Count == 0)
        {
            _queryCompilationContext.Logger.FirstWithoutOrderByAndFilterWarning();
        }

        ApplyLimit(selectExpression, TranslateExpression(source, Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.PrepareForAggregate();

        var remappedKeySelector = RemapLambdaBody(source, keySelector);
        var translatedKey = TranslateGroupingKey(remappedKeySelector);
        switch (translatedKey)
        {
            // Special handling for GroupBy over entity type: get the entity projection expression out.
            // For GroupBy over a complex type, we already get the projection expression out.
            case StructuralTypeShaperExpression { StructuralType: IEntityType } shaper:
                if (shaper.ValueBufferExpression is not ProjectionBindingExpression pbe)
                {
                    // ValueBufferExpression can be JsonQuery, ProjectionBindingExpression, EntityProjection
                    // We only allow ProjectionBindingExpression which represents a regular entity
                    return null;
                }

                translatedKey = shaper.Update(((SelectExpression)pbe.QueryExpression).GetProjection(pbe));
                break;

            case null:
                return null;
        }

        if (elementSelector != null)
        {
            source = TranslateSelect(source, elementSelector);
        }

        var groupByShaper = selectExpression.ApplyGrouping(translatedKey, source.ShaperExpression, _sqlExpressionFactory);
        if (resultSelector == null)
        {
            return source.UpdateShaperExpression(groupByShaper);
        }

        var original1 = resultSelector.Parameters[0];
        var original2 = resultSelector.Parameters[1];

        var newResultSelectorBody = new ReplacingExpressionVisitor(
                [original1, original2],
                [groupByShaper.KeySelector, groupByShaper])
            .Visit(resultSelector.Body);

        newResultSelectorBody = ExpandSharedTypeEntities(selectExpression, newResultSelectorBody);

        return source.UpdateShaperExpression(
            _projectionBindingExpressionVisitor.Translate(selectExpression, newResultSelectorBody));
    }

    private Expression? TranslateGroupingKey(Expression expression)
    {
        switch (expression)
        {
            case NewExpression newExpression:
                if (newExpression.Arguments.Count == 0)
                {
                    return newExpression;
                }

                var newArguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    var key = TranslateGroupingKey(newExpression.Arguments[i]);
                    if (key == null)
                    {
                        return null;
                    }

                    newArguments[i] = key;
                }

                return newExpression.Update(newArguments);

            case MemberInitExpression memberInitExpression:
                var updatedNewExpression = (NewExpression?)TranslateGroupingKey(memberInitExpression.NewExpression);
                if (updatedNewExpression == null)
                {
                    return null;
                }

                var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                for (var i = 0; i < newBindings.Length; i++)
                {
                    var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                    var visitedExpression = TranslateGroupingKey(memberAssignment.Expression);
                    if (visitedExpression == null)
                    {
                        return null;
                    }

                    newBindings[i] = memberAssignment.Update(visitedExpression);
                }

                return memberInitExpression.Update(updatedNewExpression, newBindings);

            default:
                var translation = TranslateProjection(expression);
                if (translation == null)
                {
                    return null;
                }

                return translation.Type == expression.Type
                    ? translation
                    : Expression.Convert(translation, expression.Type);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyIntersect((SelectExpression)source2.QueryExpression, distinct: true);

        // For intersect since result comes from both sides, if one of them is non-nullable then both are non-nullable
        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: false));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddLeftJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateRightJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddRightJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    private SqlExpression CreateJoinPredicate(
        ShapedQueryExpression outer,
        LambdaExpression outerKeySelector,
        ShapedQueryExpression inner,
        LambdaExpression innerKeySelector)
    {
        var outerKey = RemapLambdaBody(outer, outerKeySelector);
        var innerKey = RemapLambdaBody(inner, innerKeySelector);

        if (outerKey is not NewExpression { Arguments.Count: > 0 } outerNew)
        {
            return CreateJoinPredicate(outerKey, innerKey);
        }

        var innerNew = (NewExpression)innerKey;

        SqlExpression? result = null;
        for (var i = 0; i < outerNew.Arguments.Count; i++)
        {
            var joinPredicate = CreateJoinPredicate(outerNew.Arguments[i], innerNew.Arguments[i]);
            result = result == null
                ? joinPredicate
                : _sqlExpressionFactory.AndAlso(result, joinPredicate);
        }

        // In LINQ equijoins, null is not equal null, just like in SQL
        // (https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/join-clause#the-equals-operator)
        // As a result, in SqlNullabilityProcessor.ProcessJoinPredicate(), we have special handling for an equality
        // immediately inside a join predicate - we bypass null compensation for that, to make sure the SQL behavior
        // matches the LINQ behavior.
        // However, when two anonymous types are being compared, the LINQ behavior *does* treat nulls as equal; as a result, in
        // SqlNullabilityProcessor.ProcessJoinPredicate() we differentiate between a single top-level comparison
        // and multiple comparisons with ANDs.
        // Unfortunately, when we have a an anonymous type with a single property (on new { Foo = x } equals new { Foo = y }),
        // we produce the same predicate as the single comparison case (without an anonymous type), bypassing the null
        // compensation and generating incorrect results.
        // To work around this, we add an always-true predicate here, and the AND will cause
        // SqlNullabilityProcessor.ProcessJoinPredicate() to go into the multiple-property anonymous type logic,
        // and not bypass null compensation.
        if (outerNew.Arguments.Count == 1)
        {
            result = _sqlExpressionFactory.AndAlso(
                result!,
                CreateJoinPredicate(Expression.Constant(true), Expression.Constant(true)));
        }

        return result ?? _sqlExpressionFactory.Constant(true);

        SqlExpression CreateJoinPredicate(Expression outerKey, Expression innerKey)
            => TranslateExpression(Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey))!;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.LastUsedWithoutOrderBy(returnDefault ? nameof(Queryable.LastOrDefault) : nameof(Queryable.Last)));
        }

        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        selectExpression.ReverseOrderings();
        ApplyLimit(selectExpression, TranslateExpression(source, Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.LongCountWithoutPredicate, liftOrderings: false);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.IsDistinct = false;

        // For Max() over an inline array, translate to GREATEST() if possible; otherwise use the default translation of aggregate SQL
        // MAX().
        // Note that some providers propagate NULL arguments (SQLite, MySQL), while others only return NULL if all arguments evaluate to
        // NULL (SQL Server, PostgreSQL). If the argument is a nullable value type, don't translate to GREATEST() if it propagates NULLs,
        // to match the .NET behavior.
        if (TryExtractBareInlineCollectionValues(source, out var values)
            && _sqlTranslator.GenerateGreatest(values, resultType.UnwrapNullableType()) is SqlFunctionExpression greatestExpression
            && (Nullable.GetUnderlyingType(resultType) is null
                || greatestExpression.ArgumentsPropagateNullability?.All(a => a == false) == true))
        {
            return source.Update(new SelectExpression(greatestExpression, _sqlAliasManager), source.ShaperExpression);
        }

        return TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MaxWithoutSelector.MakeGenericMethod(t), resultType);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.IsDistinct = false;

        // See comments above in TranslateMax()
        if (TryExtractBareInlineCollectionValues(source, out var values)
            && _sqlTranslator.GenerateLeast(values, resultType.UnwrapNullableType()) is SqlFunctionExpression leastExpression
            && (Nullable.GetUnderlyingType(resultType) is null
                || leastExpression.ArgumentsPropagateNullability?.All(a => a == false) == true))
        {
            return source.Update(new SelectExpression(leastExpression, _sqlAliasManager), source.ShaperExpression);
        }

        return TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MinWithoutSelector.MakeGenericMethod(t), resultType);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        if (source.ShaperExpression is StructuralTypeShaperExpression { StructuralType: IEntityType entityType } shaper)
        {
            if (entityType.ClrType == resultType)
            {
                return source;
            }

            var parameterExpression = Expression.Parameter(shaper.Type);
            var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation == null)
            {
                // EntityType is not part of hierarchy
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (translation is not SqlConstantExpression { Value: true })
            {
                selectExpression.ApplyPredicate(translation);
            }

            var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
            if (baseType != null)
            {
                return source.UpdateShaperExpression(shaper.WithType(baseType));
            }

            var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
            var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;

            var projectionMember = projectionBindingExpression.ProjectionMember;
            Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

            var projection = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
            selectExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression> { { projectionMember, projection.UpdateEntityType(derivedType) } });

            return source.UpdateShaperExpression(shaper.WithType(derivedType));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOrderBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.MissingOrderingInSelectExpression);
            return null;
        }

        selectExpression.ReverseOrderings();

        return source;
    }

    [EntityFrameworkInternal]
    public virtual ShapedQueryExpression TranslateSelect2(ShapedQueryExpression source, LambdaExpression selector)
        => TranslateSelect(source, selector);

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
    {
        if (selector.Body == selector.Parameters[0])
        {
            return source;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct)
        {
            selectExpression.PushdownIntoSubquery();
        }

        var newSelectorBody = RemapLambdaBody(source, selector);

        return source.UpdateShaperExpression(_projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector)
    {
        var select = (SelectExpression)source.QueryExpression;
        var (newCollectionSelector, correlated, defaultIfEmpty)
            = new CorrelationFindingExpressionVisitor().IsCorrelated(collectionSelector);
        if (correlated)
        {
            var collectionSelectorBody = RemapLambdaBody(source, newCollectionSelector);
            if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var shaper = defaultIfEmpty
                    ? select.AddOuterApply(inner, source.ShaperExpression)
                    : select.AddCrossApply(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }
        else
        {
            if (Visit(newCollectionSelector.Body) is ShapedQueryExpression inner)
            {
                if (defaultIfEmpty)
                {
                    var translatedInner = TranslateDefaultIfEmpty(inner, null);
                    if (translatedInner == null)
                    {
                        return null;
                    }

                    inner = translatedInner;
                }

                var shaper = select.AddCrossJoin(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }

        return null;
    }

    private sealed class CorrelationFindingExpressionVisitor : ExpressionVisitor
    {
        private ParameterExpression? _outerParameter;
        private bool _correlated;
        private bool _defaultIfEmpty;
        private bool _canLiftDefaultIfEmpty;

        public (LambdaExpression, bool, bool) IsCorrelated(LambdaExpression lambdaExpression)
        {
            Check.DebugAssert(
                lambdaExpression.Parameters.Count == 1, "Multi-parameter lambda passed to CorrelationFindingExpressionVisitor");

            _correlated = false;
            _defaultIfEmpty = false;
            _canLiftDefaultIfEmpty = true;
            _outerParameter = lambdaExpression.Parameters[0];

            var result = Visit(lambdaExpression.Body);

            return (Expression.Lambda(result, _outerParameter), _correlated, _defaultIfEmpty);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression == _outerParameter)
            {
                _correlated = true;
            }

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (_canLiftDefaultIfEmpty
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
            {
                _defaultIfEmpty = true;

                return Expression.Call(
                    _fakeDefaultIfEmptyMethodInfo.Value.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]),
                    Visit(methodCallExpression.Arguments[0]));
            }

            if (!SupportsLiftingDefaultIfEmpty(methodCallExpression.Method))
            {
                // Set state to indicate that any DefaultIfEmpty encountered below this operator cannot be lifted out, since
                // doing so would change meaning.
                // For example, with blogs.SelectMany(b => b.Posts.DefaultIfEmpty().Select(p => p.Id)) we can lift
                // the DIE out, translating the SelectMany as a LEFT JOIN (or OUTER APPLY).
                // But with blogs.SelectMany(b => b.Posts.DefaultIfEmpty().Where(p => p.Id > 3)), we can't do that since that
                // what result in different results.
                _canLiftDefaultIfEmpty = false;
            }

            if (methodCallExpression.Arguments.Count == 0)
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            // We need to visit the method call as usual, but the first argument - the source (other operators we're composed over) -
            // needs to be handled differently. For the purpose of lifting DefaultIfEmpty, we can only do so for DIE at the top-level
            // operator chain, and not some DIE embedded in e.g. the lambda argument of a Where clause. So we visit the source first,
            // and then set _canLiftDefaultIfEmpty to false to avoid lifting any DIEs encountered there (see e.g. #33343).
            // Note: we assume that the first argument is the source.
            var newObject = Visit(methodCallExpression.Object);

            var arguments = methodCallExpression.Arguments;
            Expression[]? newArguments = null;

            var newSource = Visit(arguments[0]);
            if (!ReferenceEquals(newSource, arguments[0]))
            {
                newArguments = new Expression[arguments.Count];
                newArguments[0] = newSource;
            }

            var previousCanLiftDefaultIfEmpty = _canLiftDefaultIfEmpty;
            _canLiftDefaultIfEmpty = false;

            for (var i = 1; i < arguments.Count; i++)
            {
                var newArgument = Visit(arguments[i]);

                if (newArguments is not null)
                {
                    newArguments[i] = newArgument;
                }
                else if (!ReferenceEquals(newArgument, arguments[i]))
                {
                    newArguments = new Expression[arguments.Count];
                    newArguments[0] = newSource;

                    for (var j = 1; j < i; j++)
                    {
                        newArguments[j] = arguments[j];
                    }

                    newArguments[i] = newArgument;
                }
            }

            _canLiftDefaultIfEmpty = previousCanLiftDefaultIfEmpty;

            return methodCallExpression.Update(newObject, newArguments ?? (IEnumerable<Expression>)arguments);

            static bool SupportsLiftingDefaultIfEmpty(MethodInfo methodInfo)
                => methodInfo.IsGenericMethod
                    && methodInfo.GetGenericMethodDefinition() is var definition
                    && (definition == QueryableMethods.Select
                        || definition == QueryableMethods.OrderBy
                        || definition == QueryableMethods.OrderByDescending
                        || definition == QueryableMethods.Reverse);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
    {
        var innerParameter = Expression.Parameter(selector.ReturnType.GetSequenceType(), "i");
        var resultSelector = Expression.Lambda(
            innerParameter, Expression.Parameter(source.Type.GetSequenceType()), innerParameter);

        return TranslateSelectMany(source, selector, resultSelector);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        ApplyLimit(selectExpression, TranslateExpression(source, Expression.Constant(_subquery ? 1 : 2))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(source, count);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyOffset(translation);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetSumWithoutSelector, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(source, count);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        ApplyLimit(selectExpression, translation);

        return source;
    }

    private void ApplyLimit(SelectExpression selectExpression, SqlExpression limit)
    {
        var oldLimit = selectExpression.Limit;

        if (oldLimit is null)
        {
            selectExpression.SetLimit(limit);
            return;
        }

        if (oldLimit is SqlConstantExpression { Value: int oldConst } && limit is SqlConstantExpression { Value: int newConst })
        {
            // if both the old and new limit are constants, use the smaller one
            // (aka constant-fold LEAST(constA, constB))
            if (oldConst > newConst)
            {
                selectExpression.SetLimit(limit);
            }

            return;
        }

        if (oldLimit.Equals(limit))
        {
            return;
        }

        // if possible, use LEAST(oldLimit, limit); otherwise, use nested queries
        if (_sqlTranslator.GenerateLeast([oldLimit, limit], limit.Type) is { } newLimit)
        {
            selectExpression.SetLimit(newLimit);
        }
        else
        {
            selectExpression.ApplyLimit(limit);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateThenBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: true);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

        return source;
    }

    #region Join / Include

    private void AddJoin(
        SelectExpression select,
        INavigation navigation,
        Expression outerShaper,
        StructuralTypeShaperExpression innerShaper)
    {
        var innerSelect = (SelectExpression)((ProjectionBindingExpression)innerShaper.ValueBufferExpression).QueryExpression;

        // Because we sometimes attempt to translate something more than once (the first attempt fails), a join may already
        // have been added form the first attempt. We use the alias as a dedu
        if (select.Tables.Any(t => t.Alias == innerSelect.Tables.Single().Alias))
        {
            return;
        }

        var foreignKey = navigation.ForeignKey;

        // Strip off any Include expressions from the outer shaper, as these will interfere with key property binding
        // TODO: Probably no longer needed as we've moved Include processing to the end
        while (outerShaper is IncludeExpression includeExpression)
        {
            outerShaper = includeExpression.EntityExpression;
        }

        // var outerStructuralTypeShaper = (StructuralTypeShaperExpression)outerShaper;

        // TODO: Figure out the makeNullable
        var (outerKey, innerKey) = navigation.IsOnDependent
            ? (outerShaper.CreateKeyValuesExpression(foreignKey.Properties, makeNullable: true),
                innerShaper.CreateKeyValuesExpression(foreignKey.PrincipalKey.Properties, makeNullable: true))
            : (outerShaper.CreateKeyValuesExpression(foreignKey.PrincipalKey.Properties, makeNullable: true),
                innerShaper.CreateKeyValuesExpression(foreignKey.Properties, makeNullable: true));

        var joinPredicate = Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey);

        // TODO: Look into using CreateJoinPredicate instead; figure out null semantics of predicate when used in
        // navigation binding (as opposed to explicit Join)
        var translatedJoinPredicate = _sqlTranslator.Translate(joinPredicate) ?? throw new InvalidOperationException();

        // TODO: Verify the condition. Possibly look at the inverse navigation's foreign key when on dependent.
        // if (outerStructuralTypeShaper.IsNullable || navigation.IsCollection || !navigation.ForeignKey.IsRequired || !navigation.IsOnDependent)
        // if (innerShaper.IsNullable || (navigation.IsOnDependent ? !navigation.ForeignKey.IsRequired : !navigation.ForeignKey.IsRequiredDependent))
        if (innerShaper.IsNullable)
        {
            select.AddLeftJoin(innerSelect, translatedJoinPredicate);
        }
        else
        {
            select.AddInnerJoin(innerSelect, translatedJoinPredicate);
        }
    }

    // TODO: Other navigation types
    [EntityFrameworkInternal]
    public SqlExpression CreateJoinPredicate(INavigation navigation, Expression outerShaper, Expression innerShaper)
    {
        var foreignKey = navigation.ForeignKey;

        // TODO: Figure out the makeNullable
        var (outerKey, innerKey) = navigation.IsOnDependent
            ? (outerShaper.CreateKeyValuesExpression(foreignKey.Properties, makeNullable: true),
                innerShaper.CreateKeyValuesExpression(foreignKey.PrincipalKey.Properties, makeNullable: true))
            : (outerShaper.CreateKeyValuesExpression(foreignKey.PrincipalKey.Properties, makeNullable: true),
                innerShaper.CreateKeyValuesExpression(foreignKey.Properties, makeNullable: true));

        var joinPredicate = Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey);

        // TODO: Look into using CreateJoinPredicate instead
        return _sqlTranslator.Translate(joinPredicate) ?? throw new InvalidOperationException();
    }

    #endregion Join / Include

    /// <summary>
    ///     Translates the given expression into equivalent SQL representation.
    /// </summary>
    /// <param name="expression">An expression to translate.</param>
    /// <param name="applyDefaultTypeMapping">
    ///     Whether to apply the default type mapping on the top-most element if it has none. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateExpression(Expression expression, bool applyDefaultTypeMapping = true)
        // TODO: Most callers of this should pass source instead (just below)
        => TranslateExpression(source: null, expression, applyDefaultTypeMapping);

    /// <summary>
    ///     Translates the given expression into equivalent SQL representation.
    /// </summary>
    /// <param name="source">A <see cref="ShapedQueryExpression" /> on which the expression is being applied.</param>
    /// <param name="expression">An expression to translate.</param>
    /// <param name="applyDefaultTypeMapping">
    ///     Whether to apply the default type mapping on the top-most element if it has none. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateExpression(ShapedQueryExpression? source, Expression expression, bool applyDefaultTypeMapping = true)
    {
        var translation = _sqlTranslator.Translate(expression, applyDefaultTypeMapping);

        if (source is null)
        {
            if (_translationContext.PendingJoins.Count > 0)
            {
                throw new UnreachableException("Pending joins require a source shaped query expression.");
            }
        }
        else
        {
            var select = (SelectExpression)source.QueryExpression;

            foreach (var (navigation, outerShaper, innerShaper) in _translationContext.PendingJoins)
            {
                AddJoin(select, navigation, outerShaper, innerShaper);
            }

            _translationContext.PendingJoins.Clear();
        }

        if (translation is null)
        {
            if (_sqlTranslator.TranslationErrorDetails != null)
            {
                AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
            }
        }

        return translation;
    }

    // TODO: Clean up, deduplicate with above
    [EntityFrameworkInternal]
    public Expression? TranslateProjection(ShapedQueryExpression? source, Expression expression, bool applyDefaultTypeMapping = true)
    {
        var translation = _sqlTranslator.TranslateProjection(expression, applyDefaultTypeMapping);

        if (source is null)
        {
            if (_translationContext.PendingJoins.Count > 0)
            {
                throw new UnreachableException("Pending joins require a source shaped query expression.");
            }
        }
        else
        {
            var select = (SelectExpression)source.QueryExpression;

            foreach (var (navigation, outerShaper, innerShaper) in _translationContext.PendingJoins)
            {
                AddJoin(select, navigation, outerShaper, innerShaper);
            }

            _translationContext.PendingJoins.Clear();
        }

        if (translation is null)
        {
            if (_sqlTranslator.TranslationErrorDetails != null)
            {
                AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
            }
        }

        return translation;
    }

    // TODO: Clean up, deduplicate with above
    [EntityFrameworkInternal]
    public Expression? TranslateProjection(SelectExpression selectExpression, Expression expression, bool applyDefaultTypeMapping = true)
    {
        var translation = _sqlTranslator.TranslateProjection(expression, applyDefaultTypeMapping);

        foreach (var (navigation, outerShaper, innerShaper) in _translationContext.PendingJoins)
        {
            AddJoin(selectExpression, navigation, outerShaper, innerShaper);
        }

        _translationContext.PendingJoins.Clear();

        return translation;
    }

    private Expression? TranslateProjection(Expression expression, bool applyDefaultTypeMapping = true)
        => TranslateProjection(source: null, expression, applyDefaultTypeMapping);

    /// <summary>
    ///     Translates the given lambda expression for the <see cref="ShapedQueryExpression" /> source into equivalent SQL representation.
    /// </summary>
    /// <param name="shapedQueryExpression">A <see cref="ShapedQueryExpression" /> on which the lambda expression is being applied.</param>
    /// <param name="lambdaExpression">A <see cref="LambdaExpression" /> to translate into SQL.</param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given lambda expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateLambdaExpression(
        ShapedQueryExpression shapedQueryExpression,
        LambdaExpression lambdaExpression)
        => TranslateExpression(shapedQueryExpression, RemapLambdaBody(shapedQueryExpression, lambdaExpression));

    /// <summary>
    ///     Determines whether the given <see cref="SelectExpression" /> is ordered, typically because orderings have been added to it.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> to check for ordering.</param>
    /// <returns>Whether <paramref name="selectExpression" /> is ordered.</returns>
    protected virtual bool IsOrdered(SelectExpression selectExpression)
        => selectExpression.Orderings.Count > 0;

    /// <summary>
    ///     Determines whether the given <see cref="SelectExpression" /> is naturally ordered, meaning that any ordering has been added
    ///     automatically by EF to preserve e.g. the natural ordering of a JSON array, and not because the original LINQ query contained
    ///     an explicit ordering.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> to check for ordering.</param>
    /// <returns>Whether <paramref name="selectExpression" /> is ordered.</returns>
    protected virtual bool IsNaturallyOrdered(SelectExpression selectExpression)
        => false;

    [DebuggerStepThrough]
    private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
    {
        var lambdaBody = ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

        return ExpandSharedTypeEntities((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody);
    }

    // private Expression ExpandSharedTypeEntities(SelectExpression selectExpression, Expression lambdaBody)
    //     => _sharedTypeEntityExpandingExpressionVisitor.Expand(selectExpression, lambdaBody);
    private Expression ExpandSharedTypeEntities(SelectExpression selectExpression, Expression lambdaBody)
        => lambdaBody; // TODO: Remove entirely

    private sealed class IncludePruner : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
            => node switch
            {
                IncludeExpression { Navigation: ISkipNavigation or not INavigation } i => i,
                IncludeExpression i => Visit(i.EntityExpression),
                _ => base.VisitExtension(node)
            };
    }

    private ShapedQueryExpression TranslateTwoParameterSelector(ShapedQueryExpression source, LambdaExpression resultSelector)
    {
        var transparentIdentifierType = source.ShaperExpression.Type;
        var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

        Expression original1 = resultSelector.Parameters[0];
        var replacement1 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
        Expression original2 = resultSelector.Parameters[1];
        var replacement2 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
        var newResultSelector = Expression.Lambda(
            new ReplacingExpressionVisitor(
                    [original1, original2], [replacement1, replacement2])
                .Visit(resultSelector.Body),
            transparentIdentifierParameter);

        return TranslateSelect(source, newResultSelector);
    }

    private static Expression AccessField(
        Type transparentIdentifierType,
        Expression targetExpression,
        string fieldName)
        => Expression.Field(targetExpression, transparentIdentifierType.GetTypeInfo().GetDeclaredField(fieldName)!);

    private static Expression MatchShaperNullabilityForSetOperation(Expression shaper1, Expression shaper2, bool makeNullable)
    {
        switch (shaper1)
        {
            case StructuralTypeShaperExpression entityShaperExpression1
                when shaper2 is StructuralTypeShaperExpression entityShaperExpression2:
                return entityShaperExpression1.IsNullable != entityShaperExpression2.IsNullable
                    ? entityShaperExpression1.MakeNullable(makeNullable)
                    : entityShaperExpression1;

            case NewExpression newExpression1
                when shaper2 is NewExpression newExpression2:
                var newArguments = new Expression[newExpression1.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    newArguments[i] = MatchShaperNullabilityForSetOperation(
                        newExpression1.Arguments[i], newExpression2.Arguments[i], makeNullable);
                }

                return newExpression1.Update(newArguments);

            case MemberInitExpression memberInitExpression1
                when shaper2 is MemberInitExpression memberInitExpression2:
                var newExpression = (NewExpression)MatchShaperNullabilityForSetOperation(
                    memberInitExpression1.NewExpression, memberInitExpression2.NewExpression, makeNullable);

                var memberBindings = new MemberBinding[memberInitExpression1.Bindings.Count];
                for (var i = 0; i < memberBindings.Length; i++)
                {
                    var memberAssignment = memberInitExpression1.Bindings[i] as MemberAssignment;
                    Check.DebugAssert(memberAssignment != null, "Only member assignment bindings are supported");

                    memberBindings[i] = memberAssignment.Update(
                        MatchShaperNullabilityForSetOperation(
                            memberAssignment.Expression, ((MemberAssignment)memberInitExpression2.Bindings[i]).Expression,
                            makeNullable));
                }

                return memberInitExpression1.Update(newExpression, memberBindings);

            default:
                return shaper1;
        }
    }

    private ShapedQueryExpression? TranslateAggregateWithPredicate(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        MethodInfo predicateLessMethodInfo,
        bool liftOrderings)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (!selectExpression.IsDistinct)
        {
            selectExpression.ReplaceProjection(new List<Expression>());
        }

        selectExpression.PrepareForAggregate(liftOrderings);
        var selector = _sqlExpressionFactory.Fragment("*");
        var methodCall = Expression.Call(
            predicateLessMethodInfo.MakeGenericMethod(selector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(selector.Type), new EnumerableExpression(selector)));
        var translation = TranslateExpression(source, methodCall);
        if (translation == null)
        {
            return null;
        }

        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjection(projectionMapping);
        var resultType = predicateLessMethodInfo.ReturnType;

        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), resultType.MakeNullable()),
                resultType));
    }

    private ShapedQueryExpression? TranslateAggregateWithSelector(
        ShapedQueryExpression source,
        LambdaExpression? selectorLambda,
        Func<Type, MethodInfo> methodGenerator,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.PrepareForAggregate();

        Expression? selector = null;
        if (selectorLambda == null
            || selectorLambda.Body == selectorLambda.Parameters[0])
        {
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                selector = selectExpression.GetProjection(projectionBindingExpression);
            }
        }
        else
        {
            selector = RemapLambdaBody(source, selectorLambda);
        }

        if (selector == null
            || TranslateExpression(source, selector) is not { } translatedSelector)
        {
            return null;
        }

        var methodCall = Expression.Call(
            methodGenerator(translatedSelector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(translatedSelector.Type), new EnumerableExpression(translatedSelector)));
        var translation = _sqlTranslator.Translate(methodCall);
        if (translation == null)
        {
            return null;
        }

        selectExpression.ReplaceProjection(
            new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } });

        selectExpression.ClearOrdering();

        // Sum case. Projection is always non-null. We read nullable value.
        Expression shaper = new ProjectionBindingExpression(
            source.QueryExpression, new ProjectionMember(), translation.Type.MakeNullable());

        if (resultType != shaper.Type)
        {
            shaper = Expression.Convert(shaper, resultType);
        }

        return source.UpdateShaperExpression(shaper);
    }

    private bool TryGetProjection(ShapedQueryExpression shapedQueryExpression, [NotNullWhen(true)] out SqlExpression? projection)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression sqlExpression)
        {
            projection = sqlExpression;
            return true;
        }

        projection = null;
        return false;
    }

    private bool TryExtractBareInlineCollectionValues(ShapedQueryExpression shapedQuery, [NotNullWhen(true)] out SqlExpression[]? values)
        => TryExtractBareInlineCollectionValues(shapedQuery, out values, out _);

    private bool TryExtractBareInlineCollectionValues(
        ShapedQueryExpression shapedQuery,
        out SqlExpression[]? values,
        out SqlParameterExpression? valuesParameter)
    {
        if (TryGetProjection(shapedQuery, out var projection)
            && shapedQuery.QueryExpression is SelectExpression
            {
                Tables:
                [
                    ValuesExpression { ColumnNames: [ValuesOrderingColumnName, ValuesValueColumnName] } valuesExpression
                ],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                // Note that we assume ordering doesn't matter (Contains/Min/Max)
            }
            // Make sure that the source projects the column from the ValuesExpression directly, i.e. no projection out with some expression
            && projection is ColumnExpression { TableAlias: var tableAlias }
            && tableAlias == valuesExpression.Alias)
        {
            switch (valuesExpression)
            {
                case { RowValues: not null }:
                    values = new SqlExpression[valuesExpression.RowValues.Count];

                    for (var i = 0; i < values.Length; i++)
                    {
                        // Skip the first value (_ord) - this function assumes ordering doesn't matter
                        values[i] = valuesExpression.RowValues[i].Values[1];
                    }

                    valuesParameter = null;
                    return true;

                case { ValuesParameter: not null }:
                    valuesParameter = valuesExpression.ValuesParameter;
                    values = null;
                    return true;
            }
        }

        values = null;
        valuesParameter = null;
        return false;
    }

    private ShapedQueryExpression CreateShapedQueryExpressionForValuesExpression(
        ValuesExpression valuesExpression,
        string tableAlias,
        Type elementType,
        RelationalTypeMapping? inferredTypeMapping,
        bool encounteredNull)
    {
        // Note: we leave the element type mapping null, to allow it to get inferred based on queryable operators composed on top.
        var valueColumn = new ColumnExpression(
            ValuesValueColumnName,
            tableAlias,
            elementType.UnwrapNullableType(),
            typeMapping: inferredTypeMapping,
            nullable: encounteredNull);
        var orderingColumn = new ColumnExpression(
            ValuesOrderingColumnName,
            tableAlias,
            typeof(int),
            typeMapping: _typeMappingSource.FindMapping(typeof(int), RelationalDependencies.Model),
            nullable: false);

        var selectExpression = new SelectExpression(
            [valuesExpression],
            valueColumn,
            identifier: [(orderingColumn, orderingColumn.TypeMapping!.Comparer)],
            _sqlAliasManager);

        selectExpression.AppendOrdering(new OrderingExpression(orderingColumn, ascending: true));

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), encounteredNull ? elementType.MakeNullable() : elementType);

        if (elementType != shaperExpression.Type)
        {
            Check.DebugAssert(
                elementType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    private static RelationalStructuralTypeShaperExpression UnwrapIncludes(Expression shaper)
    {
        while (shaper is IncludeExpression includeExpression)
        {
            shaper = includeExpression.EntityExpression;
        }

        return (RelationalStructuralTypeShaperExpression)shaper;
    }

    private static IQueryable<TSource?> FakeDefaultIfEmpty<TSource>(IQueryable<TSource> source)
        => throw new UnreachableException();

    private static readonly Lazy<MethodInfo> _fakeDefaultIfEmptyMethodInfo = new(
        () => typeof(RelationalQueryableMethodTranslatingExpressionVisitor)
            .GetMethod(nameof(FakeDefaultIfEmpty), BindingFlags.NonPublic | BindingFlags.Static)!);

    /// <summary>
    ///     This visitor has been obsoleted; Extend RelationalTypeMappingPostprocessor instead, and invoke it from
    ///     <see cref="RelationalQueryTranslationPostprocessor.ProcessTypeMappings" />.
    /// </summary>
    [Obsolete(
        "Extend RelationalTypeMappingPostprocessor instead, and invoke it from RelationalQueryTranslationPostprocessor.ProcessTypeMappings().")]
    protected class RelationalInferredTypeMappingApplier;
}
