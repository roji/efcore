// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbSetSource : IDbSetSource
{
    private readonly ConcurrentDictionary<(Type Type, string? Name), Func<DbContext, string?, object>> _cache = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Create(
        DbContext context,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
        => CreateCore(context, type, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Create(
        DbContext context,
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
        => CreateCore(context, type, name);

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2060", Justification =
        "The trimmer can't see that the MakeGenericMethod here is safe, since we always pass in a type with the right "
        + "DynamicallyAccessedMembers.")]
    private object CreateCore(
        DbContext context,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        string? name)
        => _cache.GetOrAdd(
            (type, name),
            static t => (Func<DbContext, string?, object>)
                typeof(DbSetSource).GetMethod(nameof(CreateSetFactory), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod(t.Type)
                    .Invoke(null, null)!)(context, name);

    [UsedImplicitly]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2091", Justification = "Linker can't see inside the lambda")]
    private static Func<DbContext, string?, object> CreateSetFactory<
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>()
        where TEntity : class
        => (c, name) => new InternalDbSet<TEntity>(c, name);
}
