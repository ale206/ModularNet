using ModularNet.Domain.Enums;

namespace ModularNet.Domain.Entities;

public class ModularNetAudit
{
    public long Id { get; set; }
    public DateTimeOffset AuditTimeStamp { get; set; }
    public Guid UserId { get; set; }
    public AuditType AuditType { get; set; }
    public string? Meta { get; set; }
}