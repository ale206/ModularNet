using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class SecretsRepositoryTests
{
    private readonly Mock<IInMemoryCacheRepository> _inMemoryCacheRepositoryMock = new();
    private readonly Mock<ILogger<SecretsRepository>> _loggerMock = new();

    private SecretsRepository CreateSecretsRepository(string vaultUri = "https://test-vault.vault.azure.net/")
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AppSettings:KeyVault:VaultUri", vaultUri }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        return new SecretsRepository(configuration, _loggerMock.Object, _inMemoryCacheRepositoryMock.Object);
    }

    [Fact]
    public void ConstructorThrowsWhenAppSettingsMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            new SecretsRepository(configuration, _loggerMock.Object, _inMemoryCacheRepositoryMock.Object));
        Assert.Equal("Error getting AppSettings", exception.Message);
    }

    [Fact]
    public async Task GetSecretAndCacheItReturnsSecretFromCacheWhenAvailable()
    {
        // Arrange
        var secretsRepository = CreateSecretsRepository();
        const string secretName = "CachedKey";
        const string expectedValue = "cached-secret-val";
        _inMemoryCacheRepositoryMock.Setup(x => x.GetFromInMemoryCache<string>(secretName))
            .ReturnsAsync(expectedValue);

        // Act
        var secret = await secretsRepository.GetSecretAndCacheIt(secretName);

        // Assert
        Assert.Equal(expectedValue, secret);
        _inMemoryCacheRepositoryMock.Verify(
            x => x.SaveToInMemoryCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSecretThrowsWrappedExceptionWhenKeyVaultUnreachable()
    {
        // Arrange
        var secretsRepository = CreateSecretsRepository("https://invalid-vault-uri.vault.azure.net/");
        const string secretName = "NonExistentKey";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            secretsRepository.GetSecret(secretName));
        Assert.Equal("Error getting Secret Value", exception.Message);
    }
}
