// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     A context for a precompiled query that's being executed. This wraps the <see cref="DbContext" /> via which the query is being
///     executed, as well as a regular EF <see cref="QueryContext" />. It is flown through all intercepted LINQ operators, until the
///     terminating operator interceptor which actually executes the query. Note that it implements <see cref="IQueryable{T}" /> so that
///     it can be flown from one intercepted LINQ operator to another.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class PrecompiledQueryContext<T> : IQueryable<T>
{
    public PrecompiledQueryContext(DbContext dbContext)
        : this(dbContext, dbContext.GetService<IQueryContextFactory>().Create())
    {
    }

    private PrecompiledQueryContext(DbContext dbContext, QueryContext queryContext)
    {
        DbContext = dbContext;
        QueryContext = queryContext;
    }

    public DbContext DbContext { get; set; }
    public QueryContext QueryContext { get; }

    public PrecompiledQueryContext<T2> ChangeType<T2>()
        => new(DbContext, QueryContext);

    public IEnumerator<T> GetEnumerator()
        => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator()
        => throw new NotSupportedException();

    public Type ElementType => throw new NotSupportedException();
    public Expression Expression => throw new NotSupportedException();
    public IQueryProvider Provider => throw new NotSupportedException();
}
