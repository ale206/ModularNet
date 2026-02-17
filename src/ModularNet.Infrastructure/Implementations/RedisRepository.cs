using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ModularNet.Infrastructure.Implementations;

public class RedisRepository : IRedisRepository
{
    private const string RedisPrefixForLocalEnvironment = "_local_";
    private readonly bool _redisCacheEnabled;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<RedisRepository> _logger;
    private readonly IRedisConnectionFactory _redisConnectionFactory;

    public RedisRepository(IRedisConnectionFactory redisConnectionFactory, ILogger<RedisRepository> logger,
        IHostingEnvironment hostingEnvironment, IConfiguration configuration)
    {
        _redisConnectionFactory = redisConnectionFactory;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        _redisCacheEnabled = appSettings.ModularNetConfig.RedisCacheEnabled;
    }

    public async Task SaveToRedisCache<T>(string itemKey, T value, int cacheExpirationSeconds)
    {
        _logger.LogDebug($"Start repository method {nameof(SaveToRedisCache)}");

        if (!_redisCacheEnabled || value == null)
            return;

        var connectionString = await _redisConnectionFactory.GetRedisConnectionString();
        var redisDb = await _redisConnectionFactory.GetRedisDatabase(connectionString);

        var redisKey = GetRedisKey(itemKey);

        RedisValue redisValue;

        // Check if T is a primitive type or string
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
        {
            redisValue = new RedisValue(value.ToString());
        }
        else
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            redisValue = new RedisValue(serializedValue);
        }

        var result = await redisDb.StringSetAsync(redisKey, redisValue, TimeSpan.FromSeconds(cacheExpirationSeconds));
    }

    public async Task<T?> GetFromRedisCache<T>(string itemKey)
    {
        _logger.LogDebug($"Start repository method {nameof(GetFromRedisCache)}");

        if (!_redisCacheEnabled) return default;

        var connectionString = await _redisConnectionFactory.GetRedisConnectionString();
        var redisDb = await _redisConnectionFactory.GetRedisDatabase(connectionString);

        var redisKey = GetRedisKey(itemKey);
        var stringFromRedis = await redisDb.StringGetAsync(redisKey);

        if (stringFromRedis.IsNullOrEmpty) return default;

        // Check if T is a primitive type or string
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            return (T)Convert.ChangeType(stringFromRedis, typeof(T));

        // If T is not a primitive type or string, proceed with deserialization
        var deserializedObject = JsonConvert.DeserializeObject<T>(stringFromRedis);

        return deserializedObject;
    }

    public async Task RemoveFromRedisCache(string itemKey)
    {
        if (!_redisCacheEnabled) return;

        var connectionString = await _redisConnectionFactory.GetRedisConnectionString();
        var redisDb = await _redisConnectionFactory.GetRedisDatabase(connectionString);

        var redisKey = GetRedisKey(itemKey);
        await redisDb.KeyDeleteAsync(redisKey);
    }

    private RedisKey GetRedisKey(string itemKey)
    {
        var itemKeyWithPrefix =
            _hostingEnvironment.IsDevelopment() ? $"{RedisPrefixForLocalEnvironment}{itemKey}" : itemKey;
        return new RedisKey(itemKeyWithPrefix);
    }
}