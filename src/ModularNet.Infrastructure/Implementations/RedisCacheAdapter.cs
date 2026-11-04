using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class RedisCacheAdapter : ICacheRepository
{
    private readonly IRedisRepository _redisRepository;

    public RedisCacheAdapter(IRedisRepository redisRepository)
    {
        _redisRepository = redisRepository;
    }

    public Task Save<T>(string key, T value, int expirationSeconds) =>
        _redisRepository.SaveToRedisCache(key, value, expirationSeconds);

    public Task<T?> Get<T>(string key) => _redisRepository.GetFromRedisCache<T>(key);

    public Task Remove(string key) => _redisRepository.RemoveFromRedisCache(key);
}
