using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(IDbConnectionFactory dbConnectionFactory, ILogger<UnitOfWork> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(UnitOfWork)} constructed");
    }

    public async Task BeginTransaction()
    {
        _logger.LogDebug($"Start method {nameof(BeginTransaction)}");
        await _dbConnectionFactory.BeginTransaction();
    }

    public async Task Commit()
    {
        _logger.LogDebug($"Start method {nameof(Commit)}");
        await _dbConnectionFactory.CommitTransaction();
    }

    public async Task Rollback()
    {
        _logger.LogDebug($"Start method {nameof(Rollback)}");
        await _dbConnectionFactory.RollbackTransaction();
    }

    public void Dispose()
    {
        _dbConnectionFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbConnectionFactory.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
