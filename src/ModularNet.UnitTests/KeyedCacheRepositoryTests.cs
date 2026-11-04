using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.Infrastructure.Implementations;
using Moq;
using Xunit;

namespace ModularNet.UnitTests;

public class KeyedCacheRepositoryTests
{
    [Fact]
    public async Task KeyedRepositoriesDispatchToTheirOwnBackends()
    {
        var inMemoryCacheRepositoryMock = new Mock<IInMemoryCacheRepository>();
        var redisRepositoryMock = new Mock<IRedisRepository>();
        inMemoryCacheRepositoryMock.Setup(repository => repository.GetFromInMemoryCache<string>("key"))
            .ReturnsAsync("memory");
        redisRepositoryMock.Setup(repository => repository.GetFromRedisCache<string>("key"))
            .ReturnsAsync("redis");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddInfrastructureServices(new ConfigurationBuilder().Build());
        serviceCollection.AddSingleton(inMemoryCacheRepositoryMock.Object);
        serviceCollection.AddScoped(_ => redisRepositoryMock.Object);
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        using var serviceScope = serviceProvider.CreateScope();
        var inMemoryCacheRepository = serviceScope.ServiceProvider.GetRequiredKeyedService<ICacheRepository>(CacheType.InMemory);
        var redisCacheRepository = serviceScope.ServiceProvider.GetRequiredKeyedService<ICacheRepository>(CacheType.Redis);

        await inMemoryCacheRepository.Save("key", "value", 30);
        await redisCacheRepository.Save("key", "value", 30);
        Assert.Equal("memory", await inMemoryCacheRepository.Get<string>("key"));
        Assert.Equal("redis", await redisCacheRepository.Get<string>("key"));
        await inMemoryCacheRepository.Remove("key");
        await redisCacheRepository.Remove("key");

        inMemoryCacheRepositoryMock.Verify(repository => repository.SaveToInMemoryCache("key", "value", 30), Times.Once);
        redisRepositoryMock.Verify(repository => repository.SaveToRedisCache("key", "value", 30), Times.Once);
        inMemoryCacheRepositoryMock.Verify(repository => repository.RemoveFromInMemoryCache("key"), Times.Once);
        redisRepositoryMock.Verify(repository => repository.RemoveFromRedisCache("key"), Times.Once);
    }
    [Fact]
    public async Task DisabledRedisAdapterTreatsReadsAsMissesWithoutConnecting()
    {
        var redisConnectionFactoryMock = new Mock<IRedisConnectionFactory>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<RedisRepository>>();
        var hostingEnvironmentMock = new Mock<IHostingEnvironment>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AppSettings:ModularNetConfig:RedisCacheEnabled"] = "false"
        }).Build();
        var redisRepository = new RedisRepository(redisConnectionFactoryMock.Object, loggerMock.Object,
            hostingEnvironmentMock.Object, configuration);
        var redisCacheAdapter = new RedisCacheAdapter(redisRepository);

        await redisCacheAdapter.Save("key", "value", 30);
        Assert.Null(await redisCacheAdapter.Get<string>("key"));
        await redisCacheAdapter.Remove("key");
        redisConnectionFactoryMock.VerifyNoOtherCalls();
    }

}
