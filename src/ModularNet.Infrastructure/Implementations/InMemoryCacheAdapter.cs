using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class InMemoryCacheAdapter : ICacheRepository
{
    private readonly IInMemoryCacheRepository _inMemoryCacheRepository;

    public InMemoryCacheAdapter(IInMemoryCacheRepository inMemoryCacheRepository)
    {
        _inMemoryCacheRepository = inMemoryCacheRepository;
    }

    public Task Save<T>(string key, T value, int expirationSeconds) =>
        _inMemoryCacheRepository.SaveToInMemoryCache(key, value, expirationSeconds);

    public Task<T?> Get<T>(string key) => _inMemoryCacheRepository.GetFromInMemoryCache<T>(key);

    public Task Remove(string key) => _inMemoryCacheRepository.RemoveFromInMemoryCache(key);
}
