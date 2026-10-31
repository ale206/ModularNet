using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.IntegrationTests.Helpers;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class CacheManagerTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly ICacheManager _cacheManager;

    public CacheManagerTests(ModularNetWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _cacheManager = _scope.ServiceProvider.GetRequiredService<ICacheManager>();
    }

    [Fact]
    public async Task InMemoryCacheForStringsSavesAndRetrievesValue()
    {
        // Arrange
        const string itemKey = "stringKey";
        const string itemValue = "stringValue";
        const CacheType cacheType = CacheType.InMemory;

        // Act
        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, 60);
        var cachedValue = await _cacheManager.GetFromCache<string>(itemKey, cacheType);

        // Assert
        Assert.NotNull(cachedValue);
        Assert.Equal(itemValue, cachedValue);
    }

    [Fact]
    public async Task InMemoryCacheForPrimitivesSavesAndRetrievesValue()
    {
        // Arrange
        const string itemKey = "primitiveKey";
        const int itemValue = 42;
        const CacheType cacheType = CacheType.InMemory;

        // Act
        await _cacheManager.SaveInCache<int?>(itemKey, itemValue, cacheType, 60);
        var cachedValue = await _cacheManager.GetFromCache<int?>(itemKey, cacheType);

        // Assert
        Assert.NotNull(cachedValue);
        Assert.Equal(itemValue, cachedValue);
    }

    [Fact]
    public async Task InMemoryCacheForObjectsSavesAndRetrievesValue()
    {
        // Arrange
        const string itemKey = "objectKey";
        var user = UserHelper.GetUser();
        const CacheType cacheType = CacheType.InMemory;

        // Act
        await _cacheManager.SaveInCache(itemKey, user, cacheType, 60);
        var cachedUser = await _cacheManager.GetFromCache<User>(itemKey, cacheType);

        // Assert
        Assert.NotNull(cachedUser);
        Assert.Equal(user.Email, cachedUser.Email);
    }

    [Fact]
    public async Task RemoveFromInMemoryCacheDeletesCachedItem()
    {
        // Arrange
        const string itemKey = "itemToRemove";
        var user = UserHelper.GetUser();
        const CacheType cacheType = CacheType.InMemory;
        await _cacheManager.SaveInCache(itemKey, user, cacheType, 60);

        // Act
        await _cacheManager.RemoveFromCache(itemKey, cacheType);
        var cachedUser = await _cacheManager.GetFromCache<User>(itemKey, cacheType);

        // Assert
        Assert.Null(cachedUser);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
