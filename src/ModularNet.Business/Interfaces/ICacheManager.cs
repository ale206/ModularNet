using ModularNet.Domain.Enums;

namespace ModularNet.Business.Interfaces;

public interface ICacheManager
{
    Task SaveInCache<T>(string item, T value, CacheType cacheType, int cacheExpirationSeconds = 30);
    Task<T?> GetFromCache<T>(string itemKey, CacheType cacheType);
    Task RemoveFromCache(string item, CacheType cacheType);
}