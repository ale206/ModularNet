using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class HealthChecksRepository : IHealthChecksRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<HealthChecksRepository> _logger;

    public HealthChecksRepository(IDbConnectionFactory dbConnectionFactory, ILogger<HealthChecksRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(HealthChecksRepository)} constructed");
    }

    public async Task CheckDbConnection()
    {
        _logger.LogDebug($"Start repository method {nameof(CheckDbConnection)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"SELECT id 
            FROM `user`
            LIMIT 1";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.QueryFirstOrDefaultAsync<User>(sql);
    }
}