using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class CacheRepository : ICacheRepository
{
    private readonly AppSettings _appSettings;
    private readonly IInMemoryCacheRepository _inMemoryCacheRepository;
    private readonly ILogger<CacheRepository> _logger;
    private readonly IRedisRepository _redisRepository;

    public CacheRepository(ILogger<CacheRepository> logger, IRedisRepository redisRepository,
        IInMemoryCacheRepository inMemoryCacheRepository,
        IConfiguration configuration)
    {
        _logger = logger;
        _redisRepository = redisRepository;
        _inMemoryCacheRepository = inMemoryCacheRepository;

        _appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

        _logger.LogDebug($"{nameof(CacheRepository)} constructed");
    }

    public async Task SaveInCache<T>(string item, T value, CacheType cacheType, int cacheExpirationSeconds = 30)
    {
        _logger.LogDebug($"Start method {nameof(SaveInCache)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                if (_appSettings.ModularNetConfig.MemoryCacheEnabled)
                    await _inMemoryCacheRepository.SaveToInMemoryCache(item, value, cacheExpirationSeconds);
                break;
            case CacheType.Redis:
                if (_appSettings.ModularNetConfig.RedisCacheEnabled)
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
                return _appSettings.ModularNetConfig.MemoryCacheEnabled
                    ? await _inMemoryCacheRepository.GetFromInMemoryCache<T>(itemKey)
                    : default;
                break;
            case CacheType.Redis:
                return _appSettings.ModularNetConfig.RedisCacheEnabled
                    ? await _redisRepository.GetFromRedisCache<T>(itemKey)
                    : default;
                break;
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
                if (_appSettings.ModularNetConfig.MemoryCacheEnabled)
                    await _inMemoryCacheRepository.RemoveFromInMemoryCache(item);
                break;
            case CacheType.Redis:
                if (_appSettings.ModularNetConfig.RedisCacheEnabled)
                    await _redisRepository.RemoveFromRedisCache(item);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }

    public async Task RemoveExpiredItems(CacheType cacheType)
    {
        _logger.LogDebug($"Start method {nameof(RemoveExpiredItems)}");

        switch (cacheType)
        {
            case CacheType.InMemory:
                if (_appSettings.ModularNetConfig.MemoryCacheEnabled)
                    await _inMemoryCacheRepository.RemoveExpiredItemsFromInMemoryCache();
                break;
            case CacheType.Redis:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
        }
    }
}