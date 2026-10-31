using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class SecretsManagerTests
{
    private readonly Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<ILogger<SecretsManager>> _loggerMock = new();
    private readonly Mock<ISecretsRepository> _secretsRepositoryMock = new();

    private SecretsManager CreateSecretsManager()
    {
        return new SecretsManager(
            _secretsRepositoryMock.Object,
            _loggerMock.Object,
            _cacheManagerMock.Object);
    }

    [Fact]
    public async Task GetSecretAndCacheItReturnsSecretFromCacheWhenAvailable()
    {
        // Arrange
        var secretsManager = CreateSecretsManager();
        const string secretName = "ApiKey";
        const string cachedSecretValue = "cached-api-key-value";
        _cacheManagerMock.Setup(x => x.GetFromCache<string>(secretName, CacheType.InMemory))
            .ReturnsAsync(cachedSecretValue);

        // Act
        var secret = await secretsManager.GetSecretAndCacheIt(secretName);

        // Assert
        Assert.Equal(cachedSecretValue, secret);
        _secretsRepositoryMock.Verify(x => x.GetSecret(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSecretAndCacheItFetchesFromRepositoryAndCachesWhenCacheMisses()
    {
        // Arrange
        var secretsManager = CreateSecretsManager();
        const string secretName = "DatabasePassword";
        const string repositorySecretValue = "super-secret-password";
        _cacheManagerMock.Setup(x => x.GetFromCache<string>(secretName, CacheType.InMemory))
            .ReturnsAsync((string?)null);
        _secretsRepositoryMock.Setup(x => x.GetSecret(secretName))
            .ReturnsAsync(repositorySecretValue);

        // Act
        var secret = await secretsManager.GetSecretAndCacheIt(secretName);

        // Assert
        Assert.Equal(repositorySecretValue, secret);
        _secretsRepositoryMock.Verify(x => x.GetSecret(secretName), Times.Once);
        _cacheManagerMock.Verify(
            x => x.SaveInCache(secretName, repositorySecretValue, CacheType.InMemory, 604800),
            Times.Once);
    }

    [Fact]
    public async Task GetSecretAndCacheItHandlesNullRepositorySecretGracefully()
    {
        // Arrange
        var secretsManager = CreateSecretsManager();
        const string secretName = "MissingSecret";
        _cacheManagerMock.Setup(x => x.GetFromCache<string>(secretName, CacheType.InMemory))
            .ReturnsAsync((string?)null);
        _secretsRepositoryMock.Setup(x => x.GetSecret(secretName))
            .ReturnsAsync((string?)null);

        // Act
        var secret = await secretsManager.GetSecretAndCacheIt(secretName);

        // Assert
        Assert.Equal(string.Empty, secret);
        _cacheManagerMock.Verify(
            x => x.SaveInCache(secretName, string.Empty, CacheType.InMemory, 604800),
            Times.Once);
    }
}
