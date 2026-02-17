using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

/// <summary>
///     Tests for <see cref="CacheManager" />
/// </summary>
public class CacheManagerTests
{
    private readonly ICacheManager _cacheManager;
    private readonly Mock<IInMemoryCacheRepository> _inMemoryCacheRepositoryMock = new();
    private readonly Mock<IRedisRepository> _redisRepositoryMock = new();
    private readonly Mock<ILogger<CacheManager>> _loggerMock = new();

    public CacheManagerTests()
    {
        _cacheManager = new CacheManager(_loggerMock.Object, _inMemoryCacheRepositoryMock.Object,
            _redisRepositoryMock.Object);
    }

    [Fact]
    public async Task SaveInCache_InMemory_ShouldCallInMemoryRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var value = "testValue";
        var expirationSeconds = 30;

        // Act
        await _cacheManager.SaveInCache(itemKey, value, CacheType.InMemory, expirationSeconds);

        // Assert
        _inMemoryCacheRepositoryMock.Verify(
            x => x.SaveToInMemoryCache(itemKey, value, expirationSeconds),
            Times.Once);
        _redisRepositoryMock.Verify(
            x => x.SaveToRedisCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveInCache_Redis_ShouldCallRedisRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var value = "testValue";
        var expirationSeconds = 30;

        // Act
        await _cacheManager.SaveInCache(itemKey, value, CacheType.Redis, expirationSeconds);

        // Assert
        _redisRepositoryMock.Verify(
            x => x.SaveToRedisCache(itemKey, value, expirationSeconds),
            Times.Once);
        _inMemoryCacheRepositoryMock.Verify(
            x => x.SaveToInMemoryCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetFromCache_InMemory_ShouldCallInMemoryRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var expectedValue = "testValue";

        _inMemoryCacheRepositoryMock
            .Setup(x => x.GetFromInMemoryCache<string>(itemKey))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _cacheManager.GetFromCache<string>(itemKey, CacheType.InMemory);

        // Assert
        Assert.Equal(expectedValue, result);
        _inMemoryCacheRepositoryMock.Verify(
            x => x.GetFromInMemoryCache<string>(itemKey),
            Times.Once);
    }

    [Fact]
    public async Task GetFromCache_WhenItemDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var itemKey = "nonExistentKey";

        _inMemoryCacheRepositoryMock
            .Setup(x => x.GetFromInMemoryCache<string>(itemKey))
            .ReturnsAsync((string)null);

        // Act
        var result = await _cacheManager.GetFromCache<string>(itemKey, CacheType.InMemory);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveFromCache_InMemory_ShouldCallInMemoryRepository()
    {
        // Arrange
        var itemKey = "testKey";

        // Act
        await _cacheManager.RemoveFromCache(itemKey, CacheType.InMemory);

        // Assert
        _inMemoryCacheRepositoryMock.Verify(
            x => x.RemoveFromInMemoryCache(itemKey),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFromCache_Redis_ShouldCallRedisRepository()
    {
        // Arrange
        var itemKey = "testKey";

        // Act
        await _cacheManager.RemoveFromCache(itemKey, CacheType.Redis);

        // Assert
        _redisRepositoryMock.Verify(
            x => x.RemoveFromRedisCache(itemKey),
            Times.Once);
    }
}
