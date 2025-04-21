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
    private readonly Mock<ICacheRepository> _cacheRepositoryMock = new();
    private readonly Mock<ILogger<CacheManager>> _loggerMock = new();

    public CacheManagerTests()
    {
        _cacheManager = new CacheManager(_loggerMock.Object, _cacheRepositoryMock.Object);
    }

    [Fact]
    public async Task SaveInCache_ShouldCallRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var value = "testValue";
        var cacheType = CacheType.InMemory;
        var expirationSeconds = 30;

        // Act
        await _cacheManager.SaveInCache(itemKey, value, cacheType, expirationSeconds);

        // Assert
        _cacheRepositoryMock.Verify(
            x => x.SaveInCache(itemKey, value, cacheType, expirationSeconds),
            Times.Once);
    }

    [Fact]
    public async Task GetFromCache_ShouldCallRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var expectedValue = "testValue";
        var cacheType = CacheType.InMemory;

        _cacheRepositoryMock
            .Setup(x => x.GetFromCache<string>(itemKey, cacheType))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _cacheManager.GetFromCache<string>(itemKey, cacheType);

        // Assert
        Assert.Equal(expectedValue, result);
        _cacheRepositoryMock.Verify(
            x => x.GetFromCache<string>(itemKey, cacheType),
            Times.Once);
    }

    [Fact]
    public async Task GetFromCache_WhenItemDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var itemKey = "nonExistentKey";
        var cacheType = CacheType.InMemory;

        _cacheRepositoryMock
            .Setup(x => x.GetFromCache<string>(itemKey, cacheType))
            .ReturnsAsync((string)null);

        // Act
        var result = await _cacheManager.GetFromCache<string>(itemKey, cacheType);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveFromCache_ShouldCallRepository()
    {
        // Arrange
        var itemKey = "testKey";
        var cacheType = CacheType.InMemory;

        // Act
        await _cacheManager.RemoveFromCache(itemKey, cacheType);

        // Assert
        _cacheRepositoryMock.Verify(
            x => x.RemoveFromCache(itemKey, cacheType),
            Times.Once);
    }
}