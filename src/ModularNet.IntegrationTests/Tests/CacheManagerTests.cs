using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.IntegrationTests.Helpers;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

public class CacheManagerTests
{
    private readonly ICacheManager _cacheManager;

    public CacheManagerTests(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task RunAllTests()
    {
        await InMemoryCacheForStringsTest();
        await InMemoryMemoryCacheForPrimitivesTest();
        await InMemoryMemoryCacheForObjectsTest();
        await RemoveFromInMemoryCacheTest();

        await RedisCacheForStringsTest();
        await RedisCacheForPrimitivesTest();
        await RedisCacheForObjectsTest();
        await RemoveFromRedisCacheTest();
    }

    private async Task InMemoryCacheForStringsTest()
    {
        const string itemKey = "itemToCache";
        const string itemValue = "itemCached";
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.InMemory;

        await _cacheManager.SaveInCache<string>(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<string>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);
        Assert.Equal(itemValue, valueFromCache);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<string>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task InMemoryMemoryCacheForPrimitivesTest()
    {
        const string itemKey = "itemToCache";
        const int itemValue = 123;
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.InMemory;

        await _cacheManager.SaveInCache<int?>(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<int?>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);
        Assert.Equal(itemValue, valueFromCache);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<int?>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task InMemoryMemoryCacheForObjectsTest()
    {
        const string itemKey = "itemToCache";
        var itemValue = UserHelper.GetUser();
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.InMemory;

        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);

        var userFromCache = valueFromCache;

        Assert.Equal(itemValue.Email, userFromCache.Email);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task RemoveFromInMemoryCacheTest()
    {
        const string itemKey = "itemToCache";
        var itemValue = UserHelper.GetUser();
        const int timeToCacheInSeconds = 10;
        const CacheType cacheType = CacheType.InMemory;

        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        await _cacheManager.RemoveFromCache(itemKey, cacheType);

        var valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task RedisCacheForStringsTest()
    {
        const string itemKey = "itemToCache";
        const string itemValue = "itemCached";
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.Redis;

        await _cacheManager.SaveInCache<string>(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<string>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);
        Assert.Equal(itemValue, valueFromCache);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<string>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task RedisCacheForPrimitivesTest()
    {
        const string itemKey = "itemToCache";
        const int itemValue = 123;
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.Redis;

        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<int?>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);
        Assert.Equal(itemValue, valueFromCache);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<int?>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task RedisCacheForObjectsTest()
    {
        const string itemKey = "itemToCache";
        var itemValue = UserHelper.GetUser();
        const int timeToCacheInSeconds = 2;
        const CacheType cacheType = CacheType.Redis;

        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        var valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.NotNull(valueFromCache);

        var userFromCache = valueFromCache;

        Assert.Equal(itemValue.Email, userFromCache.Email);

        await Task.Delay(2000);
        valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }

    private async Task RemoveFromRedisCacheTest()
    {
        const string itemKey = "itemToCache";
        var itemValue = UserHelper.GetUser();
        const int timeToCacheInSeconds = 10;
        const CacheType cacheType = CacheType.Redis;

        await _cacheManager.SaveInCache(itemKey, itemValue, cacheType, timeToCacheInSeconds);

        await _cacheManager.RemoveFromCache(itemKey, cacheType);

        var valueFromCache = await _cacheManager.GetFromCache<User>(itemKey, cacheType);
        Assert.Null(valueFromCache);
    }
}