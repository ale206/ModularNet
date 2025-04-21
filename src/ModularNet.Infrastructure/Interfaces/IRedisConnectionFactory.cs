using StackExchange.Redis;

namespace ModularNet.Infrastructure.Interfaces;

public interface IRedisConnectionFactory
{
    Task<IDatabase> GetRedisDatabase(string connectionString);
    Task<string> GetRedisConnectionString();
}