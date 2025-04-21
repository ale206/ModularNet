using ModularNet.Domain.Entities;

namespace ModularNet.Business.Interfaces;

public interface ILogsManager
{
    Task WriteLogToDb(ModularNetLog modularNetLog);
}