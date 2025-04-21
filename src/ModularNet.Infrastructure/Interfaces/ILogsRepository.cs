using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface ILogsRepository
{
    Task WriteLogToDb(ModularNetLog modularNetLog);
}