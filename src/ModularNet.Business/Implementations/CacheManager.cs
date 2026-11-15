using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class CacheManager : ICacheManager
{
    private readonly ICacheRepository _inMemoryCacheRepository;
    private readonly ILogger<CacheManager> _logger;
    private readonly ICacheRepository _redisRepository;

    public CacheManager(ILogger<CacheManager> logger, [FromKeyedServices(CacheType.InMemory)] ICacheRepository inMemoryCacheRepository,
        [FromKeyedServices(CacheType.Redis)] ICacheRepository redisRepository)
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
                await _inMemoryCacheRepository.Save(item, value, cacheExpirationSeconds);
                break;
            case CacheType.Redis:
                await _redisRepository.Save(item, value, cacheExpirationSeconds);
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
                return await _inMemoryCacheRepository.Get<T>(itemKey);
            case CacheType.Redis:
                return await _redisRepository.Get<T>(itemKey);
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
                await _inMemoryCacheRepository.Remove(item);
                break;
            case CacheType.Redis:
                await _redisRepository.Remove(item);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

}
