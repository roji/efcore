// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents an entity in the projection of <see cref="SelectExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class EntityProjectionExpression : Expression
{
    private readonly IReadOnlyDictionary<IProperty, ColumnExpression> _propertyExpressionMap;
    private readonly IReadOnlyDictionary<IComplexProperty, EntityShaperExpression> _complexPropertyExpressionMap;
    private readonly Dictionary<INavigation, EntityShaperExpression> _ownedNavigationMap;

    /// <summary>
    ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
    /// </summary>
    /// <param name="typeBase">An entity type to shape.</param>
    /// <param name="propertyExpressionMap">A dictionary of column expressions corresponding to properties of the entity type.</param>
    /// <param name="complexPropertyExpressionMap">A dictionary of entity shapers corresponding to complex properties of the entity type.</param>
    /// <param name="discriminatorExpression">A <see cref="SqlExpression" /> to generate discriminator for each concrete entity type in hierarchy.</param>
    public EntityProjectionExpression(
        ITypeBase typeBase,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        IReadOnlyDictionary<IComplexProperty, EntityShaperExpression> complexPropertyExpressionMap,
        SqlExpression? discriminatorExpression = null)
        : this(
            typeBase,
            propertyExpressionMap,
            complexPropertyExpressionMap,
            new Dictionary<INavigation, EntityShaperExpression>(),
            discriminatorExpression)
    {
    }

    private EntityProjectionExpression(
        ITypeBase typeBase,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        IReadOnlyDictionary<IComplexProperty, EntityShaperExpression> complexPropertyExpressionMap,
        Dictionary<INavigation, EntityShaperExpression> ownedNavigationMap,
        SqlExpression? discriminatorExpression = null)
    {
        TypeBase = typeBase;
        _propertyExpressionMap = propertyExpressionMap;
        _complexPropertyExpressionMap = complexPropertyExpressionMap;
        _ownedNavigationMap = ownedNavigationMap;
        DiscriminatorExpression = discriminatorExpression;
    }

    /// <summary>
    ///     The base type being projected out (entity or complex type)
    /// </summary>
    public virtual ITypeBase TypeBase { get; }

    /// <summary>
    ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
    /// </summary>
    public virtual SqlExpression? DiscriminatorExpression { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => TypeBase.ClrType;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            var newExpression = (ColumnExpression)visitor.Visit(columnExpression);
            changed |= newExpression != columnExpression;

            propertyExpressionMap[property] = newExpression;
        }

        var complexPropertyExpressionMap = new Dictionary<IComplexProperty, EntityShaperExpression>();
        foreach (var (complexProperty, entityShaperExpression) in _complexPropertyExpressionMap)
        {
            var newExpression = (EntityShaperExpression)visitor.Visit(entityShaperExpression);
            changed |= newExpression != entityShaperExpression;
            complexPropertyExpressionMap[complexProperty] = newExpression;
        }

        var discriminatorExpression = (SqlExpression?)visitor.Visit(DiscriminatorExpression);
        changed |= discriminatorExpression != DiscriminatorExpression;

        var ownedNavigationMap = new Dictionary<INavigation, EntityShaperExpression>();
        foreach (var (navigation, entityShaperExpression) in _ownedNavigationMap)
        {
            var newExpression = (EntityShaperExpression)visitor.Visit(entityShaperExpression);
            changed |= newExpression != entityShaperExpression;
            ownedNavigationMap[navigation] = newExpression;
        }

        return changed
            ? new EntityProjectionExpression(
                TypeBase, propertyExpressionMap, complexPropertyExpressionMap, ownedNavigationMap, discriminatorExpression)
            : this;
    }

    /// <summary>
    ///     Makes entity instance in projection nullable.
    /// </summary>
    /// <returns>A new entity projection expression which can project nullable entity.</returns>
    public virtual EntityProjectionExpression MakeNullable()
    {
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            propertyExpressionMap[property] = columnExpression.MakeNullable();
        }

        // TODO: Process the complex property map as well once we implement JSON support

        var discriminatorExpression = DiscriminatorExpression;
        if (discriminatorExpression is ColumnExpression ce)
        {
            // if discriminator is column then we need to make it nullable
            discriminatorExpression = ce.MakeNullable();
        }

        var ownedNavigationMap = new Dictionary<INavigation, EntityShaperExpression>();
        foreach (var (navigation, shaper) in _ownedNavigationMap)
        {
            if (shaper.EntityType.IsMappedToJson())
            {
                // even if shaper is nullable, we need to make sure key property map contains nullable keys,
                // if json entity itself is optional, the shaper would be null, but the PK of the owner entity would be non-nullable
                // initially
                var jsonQueryExpression = (JsonQueryExpression)shaper.ValueBufferExpression;
                var newJsonQueryExpression = jsonQueryExpression.MakeNullable();
                var newShaper = shaper.Update(newJsonQueryExpression).MakeNullable();
                ownedNavigationMap[navigation] = newShaper;
            }
        }

        return new EntityProjectionExpression(
            TypeBase,
            propertyExpressionMap,
            _complexPropertyExpressionMap,
            ownedNavigationMap,
            discriminatorExpression);
    }

    /// <summary>
    ///     Updates the entity type being projected out to one of the derived type.
    /// </summary>
    /// <param name="derivedType">A derived entity type which should be projected.</param>
    /// <returns>A new entity projection expression which has the derived type being projected.</returns>
    public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
    {
        if (TypeBase is not IEntityType entityType)
        {
            throw new InvalidOperationException(); // TODO: Message
        }

        if (!derivedType.GetAllBaseTypes().Contains(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), entityType.DisplayName()));
        }

        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            if (derivedType.IsAssignableFrom(property.DeclaringType)
                || property.DeclaringType.IsAssignableFrom(derivedType))
            {
                propertyExpressionMap[property] = columnExpression;
            }
        }

        var complexPropertyExpressionMap = new Dictionary<IComplexProperty, EntityShaperExpression>();
        foreach (var (complexProperty, entityShaperExpression) in _complexPropertyExpressionMap)
        {
            if (derivedType.IsAssignableFrom(complexProperty.DeclaringType)
                || complexProperty.DeclaringType.IsAssignableFrom(derivedType))
            {
                complexPropertyExpressionMap[complexProperty] = entityShaperExpression;
            }
        }

        var ownedNavigationMap = new Dictionary<INavigation, EntityShaperExpression>();
        foreach (var (navigation, entityShaperExpression) in _ownedNavigationMap)
        {
            if (derivedType.IsAssignableFrom(navigation.DeclaringEntityType)
                || navigation.DeclaringEntityType.IsAssignableFrom(derivedType))
            {
                ownedNavigationMap[navigation] = entityShaperExpression;
            }
        }

        var discriminatorExpression = DiscriminatorExpression;
        if (discriminatorExpression is CaseExpression caseExpression)
        {
            var entityTypesToSelect =
                derivedType.GetConcreteDerivedTypesInclusive().Select(e => (string)e.GetDiscriminatorValue()!).ToList();
            var whenClauses = caseExpression.WhenClauses
                .Where(wc => entityTypesToSelect.Contains((string)((SqlConstantExpression)wc.Result).Value!))
                .ToList();

            discriminatorExpression = caseExpression.Update(operand: null, whenClauses, elseResult: null);
        }

        return new EntityProjectionExpression(
            derivedType, propertyExpressionMap, complexPropertyExpressionMap, ownedNavigationMap, discriminatorExpression);
    }

    /// <summary>
    ///     Binds a property with this entity projection to get the SQL representation.
    /// </summary>
    /// <param name="property">A property to bind.</param>
    /// <returns>A column which is a SQL representation of the property.</returns>
    public virtual ColumnExpression BindProperty(IProperty property)
    {
        if (!TypeBase.IsAssignableFrom(property.DeclaringType)
            && !property.DeclaringType.IsAssignableFrom(TypeBase))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, TypeBase.DisplayName()));
        }

        return _propertyExpressionMap[property];
    }

    /// <summary>
    ///     Binds a complex property with this entity projection to get the shaper for the target complex type.
    /// </summary>
    // /// <param name="property">A property to bind.</param>
    // /// <returns>A column which is a SQL representation of the property.</returns>
    public virtual EntityShaperExpression BindComplexProperty(IComplexProperty complexProperty)
    {
        if (!TypeBase.IsAssignableFrom(complexProperty.DeclaringType)
            && !complexProperty.DeclaringType.IsAssignableFrom(TypeBase))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("complexProperty", complexProperty.Name, TypeBase.DisplayName()));
        }

        throw new NotImplementedException();
        // return _propertyExpressionMap[complexProperty];
    }

    /// <summary>
    ///     Adds a navigation binding for this entity projection when the target entity type of the navigation is owned or weak.
    /// </summary>
    /// <param name="navigation">A navigation to add binding for.</param>
    /// <param name="entityShaper">An entity shaper expression for the target type.</param>
    public virtual void AddNavigationBinding(INavigation navigation, EntityShaperExpression entityShaper)
    {
        if (TypeBase is not IEntityType entityType)
        {
            throw new InvalidOperationException(); // TODO: Message
        }

        if (!entityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, entityType.DisplayName()));
        }

        _ownedNavigationMap[navigation] = entityShaper;
    }

    /// <summary>
    ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
    ///     previously added using <see cref="AddNavigationBinding(INavigation, EntityShaperExpression)" /> method.
    /// </summary>
    /// <param name="navigation">A navigation to bind.</param>
    /// <returns>An entity shaper expression for the target entity type of the navigation.</returns>
    public virtual EntityShaperExpression? BindNavigation(INavigation navigation)
    {
        if (TypeBase is not IEntityType entityType)
        {
            throw new InvalidOperationException(); // TODO: Message
        }

        if (!entityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, entityType.DisplayName()));
        }

        return _ownedNavigationMap.TryGetValue(navigation, out var expression)
            ? expression
            : null;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"EntityProjectionExpression: {TypeBase.ShortName()}";
}
