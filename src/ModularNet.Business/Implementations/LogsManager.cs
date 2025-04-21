using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class LogsManager : ILogsManager
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogsRepository _logsRepository;

    public LogsManager(ILogsRepository logsRepository, ICacheManager cacheManager)
    {
        _logsRepository = logsRepository;
        _cacheManager = cacheManager;
    }

    public async Task WriteLogToDb(ModularNetLog modularNetLog)
    {
        // We save logs in cache for 5 minutes, to avoid writing the same log continuously.
        // The same error will be written to the db once every 5 minute
        const int cacheExpirationSecondsForLogs = 300;

        var cachedLog = await _cacheManager.GetFromCache<string>(modularNetLog.LogMessage, CacheType.InMemory);

        if (cachedLog == null)
        {
            await _cacheManager.SaveInCache(modularNetLog.LogMessage, string.Empty, CacheType.InMemory,
                cacheExpirationSecondsForLogs);
            await _logsRepository.WriteLogToDb(modularNetLog);
        }
    }
}