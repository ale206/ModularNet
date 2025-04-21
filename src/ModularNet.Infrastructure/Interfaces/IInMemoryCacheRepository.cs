namespace ModularNet.Infrastructure.Interfaces;

public interface IInMemoryCacheRepository
{
    Task SaveToInMemoryCache<T>(string item, T value, int cacheExpirationSeconds);
    Task<T?> GetFromInMemoryCache<T>(string item);
    Task RemoveFromInMemoryCache(string item);
    Task RemoveExpiredItemsFromInMemoryCache();
}