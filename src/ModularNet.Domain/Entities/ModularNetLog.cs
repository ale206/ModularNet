namespace ModularNet.Domain.Entities;

public class ModularNetLog
{
    public long Id { get; set; }
    public DateTimeOffset LogTimeStamp { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
    public string? LogException { get; set; }
    public string LogProperties { get; set; } = string.Empty;
}