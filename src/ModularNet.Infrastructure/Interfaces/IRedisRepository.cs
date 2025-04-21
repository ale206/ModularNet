namespace ModularNet.Infrastructure.Interfaces;

public interface IRedisRepository
{
    Task SaveToRedisCache<T>(string itemKey, T value, int cacheExpirationSeconds);
    Task<T?> GetFromRedisCache<T>(string itemKey);
    Task RemoveFromRedisCache(string itemKey);
}