using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class AuditsRepository : IAuditsRepository
{
    private readonly ILogger<AuditsRepository> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuditsRepository(IUnitOfWork unitOfWork, ILogger<AuditsRepository> logger)
    {
        _unitOfWork = unitOfWork;
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
        var connection = await _unitOfWork.GetConnection();
        await connection.ExecuteAsync(sql, new
        {
            audit_timestamp = DateTime.UtcNow,
            user_id = modularNetAudit.UserId,
            audit_type = modularNetAudit.AuditType,
            meta = modularNetAudit.Meta
        }, transaction: _unitOfWork.Transaction);
    }
}
