using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class AuditsManager : IAuditsManager
{
    private readonly IAuditsRepository _auditsRepository;

    public AuditsManager(IAuditsRepository auditsRepository)
    {
        _auditsRepository = auditsRepository;
    }

    public async Task WriteAuditToDb(ModularNetAudit modularNetAudit)
    {
        await _auditsRepository.WriteAuditToDb(modularNetAudit);
    }
}