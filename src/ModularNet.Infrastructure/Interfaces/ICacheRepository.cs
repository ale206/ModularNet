using ModularNet.Domain.Enums;

namespace ModularNet.Infrastructure.Interfaces;

public interface ICacheRepository
{
    Task SaveInCache<T>(string item, T value, CacheType cacheType, int cacheExpirationSeconds = 30);
    Task<T?> GetFromCache<T>(string itemKey, CacheType cacheType);
    Task RemoveFromCache(string item, CacheType cacheType);
    Task RemoveExpiredItems(CacheType cacheType);
}