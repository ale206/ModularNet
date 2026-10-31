using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using Moq;

namespace ModularNet.UnitTests;

public class AppSettingsRepositoryTests
{
    private readonly Mock<ILogger<AppSettingsRepository>> _loggerMock = new();

    [Fact]
    public void GetAppSettingsReturnsPopulatedAppSettingsFromConfiguration()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AppSettings:ModularNetConfig:FrontEndBaseUrl", "https://modularnet.com" },
            { "AppSettings:ModularNetConfig:EncryptionSalt", "salt-value" },
            { "AppSettings:WebAppConfigs:FirebaseApiKey", "firebase-api-key-123" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var appSettingsRepository = new AppSettingsRepository(configuration, _loggerMock.Object);

        // Act
        var appSettings = appSettingsRepository.GetAppSettings();

        // Assert
        Assert.NotNull(appSettings);
        Assert.Equal("https://modularnet.com", appSettings.ModularNetConfig.FrontEndBaseUrl);
        Assert.Equal("salt-value", appSettings.ModularNetConfig.EncryptionSalt);
        Assert.Equal("firebase-api-key-123", appSettings.WebAppConfigs.FirebaseApiKey);
    }

    [Fact]
    public void GetAppSettingsReturnsDefaultAppSettingsWhenSectionMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var appSettingsRepository = new AppSettingsRepository(configuration, _loggerMock.Object);

        // Act
        var appSettings = appSettingsRepository.GetAppSettings();

        // Assert
        Assert.NotNull(appSettings);
    }
}
