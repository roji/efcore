// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server extension methods for LINQ queries.
/// </summary>
public static class SqlServerQueryableExtensions
{
    /// <summary>
    ///     TODO
    /// </summary>
    [Experimental("FOO")]
    public static IQueryable<VectorSearchResult<T, TVector>> VectorSearch<T, TVector>(
        this DbSet<T> source,
        Expression<Func<T, TVector>> vectorPropertySelector,
        // TODO: Should be [NotParameterized]? Check if the function accepts parameters
        string distanceFunction = "cosine")
        where T : class
        where TVector : unmanaged
    {
        var queryableSource = (IQueryable)source;
        EntityQueryRootExpression root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<VectorSearchResult<T, TVector>>(
                Expression.Call(
                    VectorSearchMethodInfo.MakeGenericMethod(typeof(T), typeof(TVector)),
                    root,
                    Expression.Quote(vectorPropertySelector),
                    Expression.Constant(distanceFunction)))
            : throw new InvalidOperationException(
                "VectorSearch can only be used with an Entity Framework Core SQL Server query provider.");
    }

    [Experimental("FOO")]
    private static IQueryable<VectorSearchResult<T, TVector>> VectorSearchInternal<T, TVector>(
        this IQueryable<T> source,
        Expression<Func<T, TVector>> vectorPropertySelector,
        string distanceFunction = "cosine")
        where T : class
        where TVector : unmanaged
        => throw new UnreachableException();

    internal static readonly MethodInfo VectorSearchMethodInfo
        = typeof(SqlServerQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(VectorSearchInternal))!;
}

[Experimental("FOO")]
public class VectorSearchResult<T, TVector>
{
#pragma warning disable IDE0060 // Remove unused parameter
    // public VectorSearchResult(Expression<Func<T, TVector>> vectorPropertySelector, string distanceFunction)
#pragma warning restore IDE0060 // Remove unused parameter
    public VectorSearchResult()
    {
    }

    public T Value { get; set; } = default!;
    public float Distance { get; set; }
}
