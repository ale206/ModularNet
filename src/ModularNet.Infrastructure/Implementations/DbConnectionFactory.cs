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
    private DbConnection? _dbConnection;
    private DbTransaction? _dbTransaction;
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

    public DbTransaction? CurrentDbTransaction
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _dbTransaction;
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

        if (_dbConnection == null)
        {
            var connectionString = await GetDbConnectionString();
            _dbConnection = GetDbConnection(connectionString);
        }

        if (_dbConnection.State != ConnectionState.Open)
        {
            await _dbConnection.OpenAsync();
        }

        return _dbConnection;
    }

    public async Task BeginTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(BeginTransaction)}");

        if (_dbTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        var dbConnection = await GetConnection();
        _dbTransaction = await dbConnection.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(CommitTransaction)}");

        if (_dbTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _dbTransaction.CommitAsync();
        }
        finally
        {
            await _dbTransaction.DisposeAsync();
            _dbTransaction = null;
        }
    }

    public async Task RollbackTransaction()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logger.LogDebug($"Start repository method {nameof(RollbackTransaction)}");

        if (_dbTransaction == null)
        {
            return;
        }

        try
        {
            await _dbTransaction.RollbackAsync();
        }
        finally
        {
            await _dbTransaction.DisposeAsync();
            _dbTransaction = null;
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
            if (_dbTransaction != null)
            {
                try
                {
                    _dbTransaction.Rollback();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exception while rolling back transaction during Dispose");
                }
                _dbTransaction.Dispose();
                _dbTransaction = null;
            }

            if (_dbConnection != null)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;

        if (_dbTransaction != null)
        {
            try
            {
                await _dbTransaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while rolling back transaction during DisposeAsync");
            }
            await _dbTransaction.DisposeAsync();
            _dbTransaction = null;
        }

        if (_dbConnection != null)
        {
            await _dbConnection.DisposeAsync();
            _dbConnection = null;
        }

        _disposed = true;
    }
}
