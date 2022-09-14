// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ManyToManyLoaderFactory
{
    private static readonly MethodInfo GenericCreate
        = typeof(ManyToManyLoaderFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateManyToMany))!;

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeGenericCreate(Type entityType, Type targetEntityType)
        => GenericCreate.MakeGenericMethod(entityType, targetEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICollectionLoader Create(ISkipNavigation skipNavigation)
        => (ICollectionLoader)MakeGenericCreate(
                skipNavigation.TargetEntityType.ClrType,
                skipNavigation.DeclaringEntityType.ClrType)
            .Invoke(null, new object[] { skipNavigation })!;

    [UsedImplicitly]
    private static ICollectionLoader CreateManyToMany<TEntity, TTargetEntity>(ISkipNavigation skipNavigation)
        where TEntity : class
        where TTargetEntity : class
        => new ManyToManyLoader<TEntity, TTargetEntity>(skipNavigation);
}
