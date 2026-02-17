using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Interfaces;
using StackExchange.Redis;

namespace ModularNet.Infrastructure.Implementations;

public class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly Lazy<Task<ConnectionMultiplexer>> _lazyConnection;
    private readonly ILogger<RedisConnectionFactory> _logger;
    private readonly ISecretsRepository _secretsRepository;

    public RedisConnectionFactory(IConfiguration configuration, IHostingEnvironment hostingEnvironment,
        ISecretsRepository secretsRepository, ILogger<RedisConnectionFactory> logger)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        _secretsRepository = secretsRepository;
        _logger = logger;

        _lazyConnection = new Lazy<Task<ConnectionMultiplexer>>(InitializeConnectionAsync);

        _logger.LogDebug($"{nameof(RedisConnectionFactory)} constructed");
    }

    public async Task<IDatabase> GetRedisDatabase(string connectionString)
    {
        _logger.LogDebug($"Start repository method {nameof(GetRedisDatabase)}");

        var redisConnection = await _lazyConnection.Value;

        // Get a reference to the Redis database
        return redisConnection.GetDatabase();
    }

    public async Task<string> GetRedisConnectionString()
    {
        _logger.LogDebug($"Start repository method {nameof(GetRedisConnectionString)}");

        if (_hostingEnvironment.IsDevelopment())
            return _configuration.GetConnectionString("RedisConnectionString") ??
                   throw new Exception("Error getting Redis Connection String");

        //TODO: Create If for each environment when ready
        //else if(_hostingEnvironment.EnvironmentName == "STG")

        return await _secretsRepository.GetSecret("redis-connection-string") ??
               throw new Exception("Error Getting Redis Connection String from Secrets");
    }

    private async Task<ConnectionMultiplexer> InitializeConnectionAsync()
    {
        _logger.LogDebug("Initializing Redis connection");
        return await ConnectionMultiplexer.ConnectAsync(await GetRedisConnectionString());
    }
}
