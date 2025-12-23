// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of an entity instance for a relational provider in
///         <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalStructuralTypeShaperExpression : StructuralTypeShaperExpression
{
    private IncludeTreeNode? _includeTree;

    public Dictionary<INavigationBase, Expression> BoundNavigations { get; private set; } = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalStructuralTypeShaperExpression" /> class.
    /// </summary>
    /// <param name="structuralType">The entity type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the entity.</param>
    /// <param name="nullable">A bool value indicating whether this entity instance can be null.</param>
    public RelationalStructuralTypeShaperExpression(ITypeBase structuralType, Expression valueBufferExpression, bool nullable)
        : base(structuralType, valueBufferExpression, nullable)
    {
        if (structuralType is IEntityType entityType)
        {
            _includeTree = new IncludeTreeNode(entityType, setLoaded: true);
        }
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalStructuralTypeShaperExpression" /> class.
    /// </summary>
    /// <param name="type">The entity type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the entity.</param>
    /// <param name="nullable">Whether this entity instance can be null.</param>
    /// <param name="materializationCondition">
    ///     An expression of <see cref="Func{T,TResult}" /> to determine which entity type to
    ///     materialize.
    /// </param>
    /// <param name="clrType">CLR type for this expression as returned from <see cref="Type"/>.</param>
    protected RelationalStructuralTypeShaperExpression(
        ITypeBase type,
        Expression valueBufferExpression,
        bool nullable,
        LambdaExpression? materializationCondition,
        Type clrType)
        : base(type, valueBufferExpression, nullable, materializationCondition, clrType)
    {
        if (type is IEntityType entityType)
        {
            _includeTree = new IncludeTreeNode(entityType, setLoaded: true);
        }
    }

    /// <inheritdoc />
    protected override LambdaExpression GenerateMaterializationCondition(ITypeBase type, bool nullable)
    {
        if (type is IComplexType)
        {
            return base.GenerateMaterializationCondition(type, nullable);
        }

        var entityType = (IEntityType)type;
        LambdaExpression baseCondition;
        // Generate discriminator condition
        var containsDiscriminatorProperty = entityType.FindDiscriminatorProperty() != null;
        if (!containsDiscriminatorProperty
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // TPT/TPC
            var valueBufferParameter = Parameter(typeof(ValueBuffer));
            var discriminatorValueVariable = Variable(typeof(string), "discriminator");
            var expressions = new List<Expression>
            {
                Assign(
                    discriminatorValueVariable,
                    valueBufferParameter.CreateValueBufferReadValueExpression(typeof(string), 0, null))
            };

            var derivedConcreteEntityTypes = entityType.GetDerivedTypes().Where(dt => !dt.IsAbstract()).ToArray();
            var switchCases = new SwitchCase[derivedConcreteEntityTypes.Length];
            for (var i = 0; i < derivedConcreteEntityTypes.Length; i++)
            {
                var discriminatorValue = Constant(derivedConcreteEntityTypes[i].ShortName(), typeof(string));
                switchCases[i] = SwitchCase(Constant(derivedConcreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
            }

            var defaultBlock = entityType.IsAbstract()
                ? CreateUnableToDiscriminateExceptionExpression(entityType, discriminatorValueVariable)
                : Constant(entityType, typeof(IEntityType));

            expressions.Add(Switch(discriminatorValueVariable, defaultBlock, switchCases));
            baseCondition = Lambda(Block([discriminatorValueVariable], expressions), valueBufferParameter);
        }
        else
        {
            baseCondition = base.GenerateMaterializationCondition(entityType, nullable);
        }

        if (containsDiscriminatorProperty
            || entityType.FindPrimaryKey() == null
            || entityType.GetRootType() != entityType
            || entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
            || entityType.IsMappedToJson())
        {
            return baseCondition;
        }

        var table = entityType.GetViewOrTableMappings().SingleOrDefault(e => e.IsSplitEntityTypePrincipal ?? true)?.Table
            ?? entityType.GetDefaultMappings().Single().Table;
        if (table.IsOptional(entityType))
        {
            // Optional dependent
            var body = baseCondition.Body;
            var valueBufferParameter = baseCondition.Parameters[0];
            Expression? condition = null;
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                condition = requiredNonPkProperties
                    .Select(p => NotEqual(
                        valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                        Constant(null)))
                    .Aggregate(AndAlso);
            }

            var allNonPrincipalSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table);
            // We don't need condition for nullable property if there exist at least one required property which is non shared.
            if (allNonPrincipalSharedNonPkProperties.Count != 0
                && allNonPrincipalSharedNonPkProperties.All(p => p.IsNullable))
            {
                var atLeastOneNonNullValueInNullablePropertyCondition = allNonPrincipalSharedNonPkProperties
                    .Select(p => NotEqual(
                        valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                        Constant(null)))
                    .Aggregate(OrElse);

                condition = condition == null
                    ? atLeastOneNonNullValueInNullablePropertyCondition
                    : AndAlso(condition, atLeastOneNonNullValueInNullablePropertyCondition);
            }

            if (condition != null)
            {
                body = Condition(condition, body, Default(typeof(IEntityType)));
            }

            return Lambda(body, valueBufferParameter);
        }

        return baseCondition;
    }

    /// <inheritdoc />
    public override StructuralTypeShaperExpression WithType(ITypeBase type)
        => type != StructuralType
            ? new RelationalStructuralTypeShaperExpression(type, ValueBufferExpression, IsNullable, materializationCondition: null, type.ClrType)
            : this;

    /// <inheritdoc />
    public override StructuralTypeShaperExpression MakeNullable(bool nullable = true)
    {
        if (IsNullable == nullable)
        {
            return this;
        }

        var newValueBufferExpression = ValueBufferExpression;
        if (StructuralType is IComplexType
            && ValueBufferExpression is StructuralTypeProjectionExpression structuralTypeProjectionExpression)
        {
            // for complex types we also need to go inside and mark all properties there as nullable
            // so that when they get extracted during apply projection, they have correct nullabilities
            // for entity types (containing complex types) we are ok - if the pushdown already happened we iterate over all complex shaper
            // and call MakeNullable, so we get here. If pushdown hasn't happened yet (so the complex cache is not populated yet)
            // it's enough to mark shaper itself as nullable - when we eventually create shapers for the complex types
            // in GenerateComplexPropertyShaperExpression, we generate the columns as nullable if the shaper itself is nullable
            // see issue #33547 for more details
            newValueBufferExpression = structuralTypeProjectionExpression.MakeNullable();
        }

        // Marking nullable requires re-computation of Discriminator condition
        return new RelationalStructuralTypeShaperExpression(StructuralType, newValueBufferExpression, true, materializationCondition: null, Type);
    }

    /// <inheritdoc />
    public override RelationalStructuralTypeShaperExpression Update(Expression valueBufferExpression)
        => valueBufferExpression != ValueBufferExpression
            ? new RelationalStructuralTypeShaperExpression(StructuralType, valueBufferExpression, IsNullable, MaterializationCondition, Type)
            {
                // TODO: Make sure this makes sense
                BoundNavigations = BoundNavigations,
                _includeTree = _includeTree,
                LastIncludeTreeNode = LastIncludeTreeNode
            }
            : this;

    /// <inheritdoc />
    public override RelationalStructuralTypeShaperExpression MakeClrTypeNullable()
        => Type != Type.MakeNullable()
            ? new RelationalStructuralTypeShaperExpression(StructuralType, ValueBufferExpression, IsNullable, MaterializationCondition, Type.MakeNullable())
            : this;

    /// <inheritdoc />
    public override RelationalStructuralTypeShaperExpression MakeClrTypeNonNullable()
        => Type != Type.UnwrapNullableType()
            ? new RelationalStructuralTypeShaperExpression(StructuralType, ValueBufferExpression, IsNullable, MaterializationCondition, Type.UnwrapNullableType())
            : this;

    public IncludeTreeNode IncludeTree
        => StructuralType is IEntityType
            ? _includeTree!
            : throw new InvalidOperationException("IncludeTree is only available for entity types.");

    public IncludeTreeNode? LastIncludeTreeNode;
}

// TODO: Should this be immutable...
// TODO: INavigationBase
public class IncludeTreeNode(IEntityType entityType, bool setLoaded)
    : Dictionary<INavigation, IncludeTreeNode>
{
    // TODO: Is this really needed?
    public IEntityType EntityType { get; } = entityType;
    // private readonly RelationalStructuralTypeShaperExpression? _shaper = shaper;
    public bool SetLoaded { get; private set; } = setLoaded;

    // TODO: INavigationBase
    public IncludeTreeNode AddNavigation(INavigation navigation, bool setLoaded)
    {
        if (TryGetValue(navigation, out var existingValue))
        {
            if (setLoaded && !existingValue.SetLoaded)
            {
                existingValue.SetLoaded = true;
            }

            return existingValue;
        }

        // IncludeTreeNode? nodeToAdd = null;

        // nodeToAdd = navigation switch
        // {
        //     INavigation concreteNavigation when _reference.ForeignKeyExpansionMap.TryGetValue(
        //         (concreteNavigation.ForeignKey, concreteNavigation.IsOnDependent), out var expansion) => UnwrapEntityReference(
        //         expansion)!.IncludePaths,
        //     ISkipNavigation skipNavigation when _reference.ForeignKeyExpansionMap.TryGetValue(
        //             (skipNavigation.ForeignKey, skipNavigation.IsOnDependent), out var firstExpansion)
        //         // Value known to be non-null
        //         && UnwrapEntityReference(firstExpansion)!.ForeignKeyExpansionMap.TryGetValue(
        //             (skipNavigation.Inverse.ForeignKey, !skipNavigation.Inverse.IsOnDependent),
        //             out var secondExpansion) => UnwrapEntityReference(secondExpansion)!.IncludePaths,
        //     _ => nodeToAdd
        // };

        // nodeToAdd ??= new IncludeTreeNode(navigation.TargetEntityType, null, setLoaded);

        // this[navigation] = nodeToAdd;

        // return this[navigation];

        return this[navigation] = new IncludeTreeNode(navigation.TargetEntityType, setLoaded);
    }

    public void Merge(IncludeTreeNode includeTreeNode)
    {
        // EntityReference is intentionally ignored
        // FilterExpression = includeTreeNode.FilterExpression;
        foreach (var (navigationBase, value) in includeTreeNode)
        {
            AddNavigation(navigationBase, value.SetLoaded).Merge(value);
        }
    }

    public override string ToString() => $"";
}
