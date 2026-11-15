namespace ModularNet.Infrastructure.Interfaces;

/// <summary>A common contract for selecting a cache backend through keyed dependency injection.</summary>
public interface ICacheRepository
{
    Task Save<T>(string key, T value, int expirationSeconds);
    Task<T?> Get<T>(string key);
    Task Remove(string key);
}
