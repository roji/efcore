// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072", Justification = "TODO")]
public class DbSetFinder : IDbSetFinder
{
    private readonly ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>> _cache = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<DbSetProperty> FindSets(Type contextType)
        => _cache.GetOrAdd(contextType, FindSetsNonCached);

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2067",
        Justification =
            "If any DbSet properties aren't referenced in the application, they get trimmed and we won't find them below. "
            + "That's OK in this context (but may be problematic elsewhere as they're absent from the model)")]
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2072",
        Justification =
            "Any DbSet<T> properties we find below are guaranteed to have DynamicallyAccessedMemberTypes.All via the annotation on DbSet")]
    private static DbSetProperty[] FindSetsNonCached(Type contextType)
    {
        var factory = new ClrPropertySetterFactory();

        return contextType.GetRuntimeProperties()
            .Where(
                p => !p.IsStatic()
                    && !p.GetIndexParameters().Any()
                    && p.DeclaringType != typeof(DbContext)
                    && p.PropertyType.GetTypeInfo().IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .OrderBy(p => p.Name)
            .Select(
                p => new DbSetProperty(
                    p.Name,
                    p.PropertyType.GenericTypeArguments.Single(),
                    p.SetMethod == null ? null : factory.Create(p)))
            .ToArray();
    }
}
