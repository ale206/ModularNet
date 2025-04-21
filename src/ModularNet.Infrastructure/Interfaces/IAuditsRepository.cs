using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface IAuditsRepository
{
    Task WriteAuditToDb(ModularNetAudit modularNetAudit);
}