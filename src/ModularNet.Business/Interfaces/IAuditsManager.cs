using ModularNet.Domain.Entities;

namespace ModularNet.Business.Interfaces;

public interface IAuditsManager
{
    Task WriteAuditToDb(ModularNetAudit modularNetAudit);
}