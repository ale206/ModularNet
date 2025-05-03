using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class LogsRepository : ILogsRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<LogsRepository> _logger;

    public LogsRepository(IDbConnectionFactory dbConnectionFactory, ILogger<LogsRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(LogsRepository)} constructed");
    }

    public async Task WriteLogToDb(ModularNetLog modularNetLog)
    {
        _logger.LogDebug($"Start repository method {nameof(WriteLogToDb)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"INSERT INTO `modular_net`.`modular_net_logs` 
                (`id`, `log_timestamp`, `log_level`, `log_message`, `log_exception`, `log_properties`)
                VALUES 
                (@id, @log_timestamp, @log_level, @log_message, @log_exception, @log_properties )";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = modularNetLog.Id,
            log_timestamp = modularNetLog.LogTimeStamp,
            log_level = modularNetLog.LogLevel,
            log_message = modularNetLog.LogMessage,
            log_exception = modularNetLog.LogException,
            log_properties = modularNetLog.LogProperties
        });
    }
}