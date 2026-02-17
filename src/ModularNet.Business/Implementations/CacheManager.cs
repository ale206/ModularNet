using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class CacheManager : ICacheManager
{
    private readonly IInMemoryCacheRepository _inMemoryCacheRepository;
    private readonly ILogger<CacheManager> _logger;
    private readonly IRedisRepository _redisRepository;

    public CacheManager(ILogger<CacheManager> logger, IInMemoryCacheRepository inMemoryCacheRepository,
        IRedisRepository redisRepository)
    {
        _logger = logger;
        _inMemoryCacheRepository = inMemoryCacheRepository;
        _redisRepository = redisRepository;

        _logger.LogDebug($"{nameof(CacheManager)} constructed");
    }

    public async Task SaveInCache<T>(string item, T value, CacheType cacheType, int cacheExpirationSeconds = 30)
    {
        _logger.LogDebug($"Start method {nameof(SaveInCache)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                await _inMemoryCacheRepository.SaveToInMemoryCache(item, value, cacheExpirationSeconds);
                break;
            case CacheType.Redis:
                await _redisRepository.SaveToRedisCache(item, value, cacheExpirationSeconds);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public async Task<T?> GetFromCache<T>(string itemKey, CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(GetFromCache)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                return await _inMemoryCacheRepository.GetFromInMemoryCache<T>(itemKey);
            case CacheType.Redis:
                return await _redisRepository.GetFromRedisCache<T>(itemKey);
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public async Task RemoveFromCache(string item, CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(RemoveFromCache)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                await _inMemoryCacheRepository.RemoveFromInMemoryCache(item);
                break;
            case CacheType.Redis:
                await _redisRepository.RemoveFromRedisCache(item);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    private async Task RemoveExpiredItems(CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(RemoveExpiredItems)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                await _inMemoryCacheRepository.RemoveExpiredItemsFromInMemoryCache();
                break;
            case CacheType.Redis:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }
}
