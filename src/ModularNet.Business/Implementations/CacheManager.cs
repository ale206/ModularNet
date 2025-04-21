using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class CacheManager : ICacheManager
{
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<CacheManager> _logger;

    public CacheManager(ILogger<CacheManager> logger, ICacheRepository cacheRepository)
    {
        _logger = logger;
        _cacheRepository = cacheRepository;

        _logger.LogDebug($"{nameof(CacheManager)} constructed");
    }

    public async Task SaveInCache<T>(string item, T value, CacheType cacheType, int cacheExpirationSeconds = 30)
    {
        _logger.LogDebug($"Start method {nameof(SaveInCache)}");

        await _cacheRepository.SaveInCache(item, value, cacheType, cacheExpirationSeconds);
    }

    public async Task<T?> GetFromCache<T>(string itemKey, CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(GetFromCache)}");

        return await _cacheRepository.GetFromCache<T>(itemKey, cacheType);
    }

    public async Task RemoveFromCache(string item, CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(RemoveFromCache)}");

        await _cacheRepository.RemoveFromCache(item, cacheType);
    }

    private async Task RemoveExpiredItems(CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(RemoveExpiredItems)}");

        await _cacheRepository.RemoveExpiredItems(cacheType);
    }
}