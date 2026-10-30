using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class AuditsRepository : IAuditsRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<AuditsRepository> _logger;

    public AuditsRepository(IDbConnectionFactory dbConnectionFactory, ILogger<AuditsRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(AuditsRepository)} constructed");
    }

    public async Task WriteAuditToDb(ModularNetAudit modularNetAudit)
    {
        _logger.LogDebug($"Start repository method {nameof(WriteAuditToDb)}");

        const string sql =
            @"INSERT INTO `modularNet_audits` 
                (`audit_timestamp`, `user_id`, `audit_type`, `meta`)
                VALUES 
                (@audit_timestamp, @user_id, @audit_type, @meta)";
        var connection = await _dbConnectionFactory.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            audit_timestamp = DateTime.UtcNow,
            user_id = modularNetAudit.UserId,
            audit_type = modularNetAudit.AuditType,
            meta = modularNetAudit.Meta
        }, transaction: _dbConnectionFactory.CurrentTransaction);
    }
}
