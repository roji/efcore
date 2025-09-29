// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.Tasks;
using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BulkUpdatesAsserter(IBulkUpdatesFixtureBase queryFixture, Func<Expression, Expression> rewriteServerQueryExpression)
{
    private readonly Func<DbContext> _contextCreator = queryFixture.GetContextCreator();
    private readonly Action<DatabaseFacade, IDbContextTransaction> _useTransaction = queryFixture.GetUseTransaction();
    private readonly Func<DbContext, ISetSource> _setSourceCreator = queryFixture.GetSetSourceCreator();
    private readonly Func<Expression, Expression> _rewriteServerQueryExpression = rewriteServerQueryExpression;
    private readonly IReadOnlyDictionary<Type, object> _entitySorters = queryFixture.EntitySorters ?? new Dictionary<Type, object>();
    private readonly ISetSource _expectedData = queryFixture.GetExpectedData();

    public Task AssertDelete<TResult>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        int rowsAffectedCount)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            _contextCreator, _useTransaction,
            async context =>
            {
                var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                var result = async
                    ? await processedQuery.ExecuteDeleteAsync()
                    : processedQuery.ExecuteDelete();

                Assert.Equal(rowsAffectedCount, result);
            });

    public Task AssertUpdate<TResult, TEntity>(
        bool async,
        Func<ISetSource, IQueryable<TResult>> query,
        Expression<Func<TResult, TEntity>> entitySelector,
        Action<UpdateSettersBuilder<TResult>> setPropertyCalls,
        int rowsAffectedCount,
        Action<IReadOnlyList<TEntity>, IReadOnlyList<TEntity>>? asserter = null)
        where TResult : class
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            _contextCreator, _useTransaction,
            async context =>
            {
                var elementSorter = (Func<TEntity, object>)_entitySorters[typeof(TEntity)];

                var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));

                var before = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                var result = async
                    ? await processedQuery.ExecuteUpdateAsync(setPropertyCalls)
                    : processedQuery.ExecuteUpdate(setPropertyCalls);

                Assert.Equal(rowsAffectedCount, result);

                var after = processedQuery.AsNoTracking().Select(entitySelector).OrderBy(elementSorter).ToList();

                asserter?.Invoke(before, after);
            });

    public Task AssertUpdate(Expression<Func<ISetSource, int>> update, int rowsAffectedCount)
    {
        if (update is not LambdaExpression
            {
                Body: MethodCallExpression
                {
                    Method.Name: nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync),
                    Arguments: [var source, var settersExpression]
                }
            })
        {
            throw new ArgumentException("Only ExecuteUpdateAsync is supported.");
        }

        if (settersExpression is not LambdaExpression
            {
                Body: var settersExpressionBody,
                Parameters: [var setterBuilderParameter]
            })
        {
            throw new UnreachableException();
        }

        // Convert the expression containing the setters
            // Action<UpdateSettersBuilder<TSource>> foo =
            // Expression setPropertyCalls = Expression.Call(
            //     typeof(BulkUpdatesAsserter),
            //     nameof(Foo),
            //     [source.Type],
            //     settersExpression);
        var node = settersExpressionBody;

        while (node is MethodCallExpression { })
        {

        }

        if (node is not ParameterExpression p || p != )



        var executeUpdateMethod = ExecuteUpdateAsyncMethodInfo.MakeGenericMethod(source.Type);
        var rowsAffected = (int)executeUpdateMethod.Invoke(obj: null, [source, setPropertyCalls])!;
        // var settersBuilder = Action<UpdateSettersBuilder<TSource>> setPropertyCalls


        // TestHelpers.ExecuteWithStrategyInTransactionAsync(
        //             _contextCreator, _useTransaction,
        //             async context =>
        //             {
        //             });

        Action<UpdateSettersBuilder<TSource>> Foo<TSource>(Expression settersExpression)
        {

        }
    }

    public static readonly MethodInfo ExecuteUpdateAsyncMethodInfo
        = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.Name == nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync));

    private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
        => query.Provider.CreateQuery<T>(_rewriteServerQueryExpression(query.Expression));
}
