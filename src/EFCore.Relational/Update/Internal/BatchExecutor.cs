// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class BatchExecutor : IBatchExecutor
{
    private const string SavepointName = "__EFSavePoint";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public BatchExecutor(
        ICurrentDbContext currentContext,
        ISqlGenerationHelper sqlGenerationHelper,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger,
        IRelationalCommandDiagnosticsLogger commandLogger)
    {
        CurrentContext = currentContext;
        SqlGenerationHelper = sqlGenerationHelper;
        RawSqlCommandBuilder = rawSqlCommandBuilder;
        UpdateLogger = updateLogger;
        CommandLogger = commandLogger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICurrentDbContext CurrentContext { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ISqlGenerationHelper SqlGenerationHelper { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRawSqlCommandBuilder RawSqlCommandBuilder { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IRelationalCommandDiagnosticsLogger CommandLogger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int Execute(
        IEnumerable<(ModificationCommandBatch Batch, bool HasMore)> commandBatches,
        IRelationalConnection connection)
    {
        using var batchEnumerator = commandBatches.GetEnumerator();

        if (!batchEnumerator.MoveNext())
        {
            return 0;
        }

        var (batch, hasMoreBatches) = batchEnumerator.Current;

        var rowsAffected = 0;
        var transaction = connection.CurrentTransaction;
        var usingRawSqlForTransactions = true; // TODO: Opt-out
        var beganTransaction = false;
        var beganApiTransaction = false;
        var createdSavepoint = false;
        try
        {
            var transactionEnlistManager = connection as ITransactionEnlistmentManager;
            if (transaction == null
                && transactionEnlistManager?.EnlistedTransaction is null
                && transactionEnlistManager?.CurrentAmbientTransaction is null
                && CurrentContext.Context.Database.AutoTransactionsEnabled)
            {
                // Don't start a transaction if we have a single batch which doesn't require a transaction (single command), for perf.
                if (hasMoreBatches || batch.RequiresTransaction)
                {
                    // We default to starting (and committing) the transaction by prepending BEGIN to the first batch and appending COMMIT
                    // to the last: this saves database roundtrips compared to doing it via the ADO.NET API.
                    if (usingRawSqlForTransactions)
                    {
                        batch.AddPrependedSql(SqlGenerationHelper.StartTransactionStatement);
                    }
                    else
                    {
                        transaction = connection.BeginTransaction();
                    }

                    beganTransaction = true;
                }
                else
                {
                    // We don't need to start a transaction, but make sure auto-commit mode is enabled, so that an implicit transaction
                    // doesn't get started
                    if (SqlGenerationHelper.EnsureAutocommitStatement is { } ensureAutocommitStatement)
                    {
                        batch.AddPrependedSql(ensureAutocommitStatement);
                    }
                }
            }
            else
            {
                connection.Open();

                if (transaction?.SupportsSavepoints == true
                    && CurrentContext.Context.Database.AutoSavepointsEnabled)
                {
                    transaction.CreateSavepoint(SavepointName);
                    createdSavepoint = true;
                }
            }

            batch.AddSaveChangesHeader();

            do
            {
                (batch, hasMoreBatches) = batchEnumerator.Current;

                if (!hasMoreBatches && beganTransaction && usingRawSqlForTransactions)
                {
                    batch.AddAppendedSql(SqlGenerationHelper.CommitTransactionStatement);
                }

                batch.Execute(connection);

                rowsAffected += batch.ModificationCommands.Count;
            }
            while (batchEnumerator.MoveNext());

            if (beganApiTransaction)
            {
                transaction!.Commit();
            }
        }
        catch
        {
            if (connection.DbConnection.State == ConnectionState.Open)
            {
                if (beganTransaction)
                {
                    if (usingRawSqlForTransactions)
                    {
                        try
                        {
                            RawSqlCommandBuilder
                                .Build(SqlGenerationHelper.RollbackTransactionStatement)
                                .ExecuteNonQuery(new(connection, null, null, CurrentContext.Context, CommandLogger));
                        }
                        catch (Exception)
                        {
                            // TODO: LOG?
                        }
                    }
                    else
                    {
                        transaction!.Rollback();
                    }
                }
                else if (createdSavepoint)
                {
                    try
                    {
                        transaction!.RollbackToSavepoint(SavepointName);
                    }
                    catch (Exception e)
                    {
                        UpdateLogger.BatchExecutorFailedToRollbackToSavepoint(CurrentContext.GetType(), e);
                    }
                }
            }

            throw;
        }
        finally
        {
            if (beganTransaction && !usingRawSqlForTransactions)
            {
                transaction!.Dispose();
            }
            else
            {
                if (createdSavepoint)
                {
                    if (connection.DbConnection.State == ConnectionState.Open)
                    {
                        try
                        {
                            transaction!.ReleaseSavepoint(SavepointName);
                        }
                        catch (Exception e)
                        {
                            UpdateLogger.BatchExecutorFailedToReleaseSavepoint(CurrentContext.GetType(), e);
                        }
                    }
                }

                connection.Close();
            }
        }

        return rowsAffected;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<int> ExecuteAsync(
        IEnumerable<(ModificationCommandBatch Batch, bool HasMore)> commandBatches,
        IRelationalConnection connection,
        CancellationToken cancellationToken = default)
    {
        using var batchEnumerator = commandBatches.GetEnumerator();

        if (!batchEnumerator.MoveNext())
        {
            return 0;
        }

        var (batch, hasMoreBatches) = batchEnumerator.Current;

        var rowsAffected = 0;
        var transaction = connection.CurrentTransaction;
        var beganTransaction = false;
        var createdSavepoint = false;
        try
        {
            var transactionEnlistManager = connection as ITransactionEnlistmentManager;
            if (transaction == null
                && transactionEnlistManager?.EnlistedTransaction is null
                && transactionEnlistManager?.CurrentAmbientTransaction is null
                && CurrentContext.Context.Database.AutoTransactionsEnabled
                // Don't start a transaction if we have a single batch which doesn't require a transaction (single command), for perf.
                && (hasMoreBatches || batch.RequiresTransaction))
            {
                transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                beganTransaction = true;
            }
            else
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                if (transaction?.SupportsSavepoints == true
                    && CurrentContext.Context.Database.AutoSavepointsEnabled)
                {
                    await transaction.CreateSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                    createdSavepoint = true;
                }
            }

            do
            {
                batch = batchEnumerator.Current.Batch;
                await batch.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                rowsAffected += batch.ModificationCommands.Count;
            }
            while (batchEnumerator.MoveNext());

            if (beganTransaction)
            {
                await transaction!.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            if (createdSavepoint && connection.DbConnection.State == ConnectionState.Open)
            {
                try
                {
                    await transaction!.RollbackToSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    UpdateLogger.BatchExecutorFailedToRollbackToSavepoint(CurrentContext.GetType(), e);
                }
            }

            throw;
        }
        finally
        {
            if (beganTransaction)
            {
                await transaction!.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                if (createdSavepoint)
                {
                    if (connection.DbConnection.State == ConnectionState.Open)
                    {
                        try
                        {
                            await transaction!.ReleaseSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            UpdateLogger.BatchExecutorFailedToReleaseSavepoint(CurrentContext.GetType(), e);
                        }
                    }
                }

                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        return rowsAffected;
    }
}
