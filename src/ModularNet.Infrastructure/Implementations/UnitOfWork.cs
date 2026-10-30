using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<UnitOfWork> _logger;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(IDbConnectionFactory dbConnectionFactory, ILogger<UnitOfWork> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(UnitOfWork)} constructed");
    }

    public IDbTransaction? Transaction
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _transaction;
        }
    }

    public async Task<IDbConnection> GetConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start method {nameof(GetConnection)}");

        if (_connection == null)
        {
            var connectionString = await _dbConnectionFactory.GetDbConnectionString();
            _connection = _dbConnectionFactory.GetDbConnection(connectionString);
        }

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        return _connection;
    }

    public async Task BeginTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start method {nameof(BeginTransaction)}");

        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        await GetConnection();
        _transaction = await _connection!.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start method {nameof(Commit)}");

        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task Rollback()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start method {nameof(Rollback)}");

        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.Rollback();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exception while rolling back transaction during Dispose");
                }
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        if (_transaction != null)
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while rolling back transaction during DisposeAsync");
            }
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _disposed = true;
    }
}
