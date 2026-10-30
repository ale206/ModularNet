using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Interfaces;
using MySqlConnector;

namespace ModularNet.Infrastructure.Implementations;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<DbConnectionFactory> _logger;
    private readonly ISecretsRepository _secretsRepository;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public DbConnectionFactory(IConfiguration configuration, IHostingEnvironment hostingEnvironment,
        ISecretsRepository secretsRepository, ILogger<DbConnectionFactory> logger)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        _secretsRepository = secretsRepository;
        _logger = logger;

        _logger.LogDebug($"{nameof(DbConnectionFactory)} constructed");
    }

    public DbTransaction? CurrentTransaction
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _transaction;
        }
    }

    public virtual DbConnection GetDbConnection(string connectionString)
    {
        _logger.LogDebug($"Start repository method {nameof(GetDbConnection)}");

        // This is to return proper DateTime from MySql instead of 01-01-0001
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return new MySqlConnection(connectionString);
    }

    public async Task<string> GetDbConnectionString()
    {
        _logger.LogDebug($"Start repository method {nameof(GetDbConnectionString)}");

        if (_hostingEnvironment.IsDevelopment())
            return _configuration.GetConnectionString("ModularNetConnectionString") ??
                   throw new Exception("Error getting Db Connection String");

        //TODO: Create If for each environment when ready
        //else if(_hostingEnvironment.EnvironmentName == "STG") 

        return await _secretsRepository.GetSecret("connection-string") ??
               throw new Exception("Error Getting Db Connection String from Secrets");
    }

    public async Task<DbConnection> GetConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(GetConnection)}");

        if (_connection == null)
        {
            var connectionString = await GetDbConnectionString();
            _connection = GetDbConnection(connectionString);
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
        _logger.LogDebug($"Start repository method {nameof(BeginTransaction)}");

        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        var connection = await GetConnection();
        _transaction = await connection.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(CommitTransaction)}");

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

    public async Task RollbackTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(RollbackTransaction)}");

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
