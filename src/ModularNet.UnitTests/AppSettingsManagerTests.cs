using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class AppSettingsManagerTests
{
    private readonly Mock<IAppSettingsRepository> _appSettingsRepositoryMock = new();
    private readonly Mock<IHostingEnvironment> _hostingEnvironmentMock = new();
    private readonly Mock<ILogger<AppSettingsManager>> _loggerMock = new();
    private readonly Mock<ISecretsManager> _secretsManagerMock = new();

    private AppSettingsManager CreateAppSettingsManager()
    {
        return new AppSettingsManager(
            _appSettingsRepositoryMock.Object,
            _hostingEnvironmentMock.Object,
            _secretsManagerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAppSettingsReturnsConfigurationInDevelopmentEnvironment()
    {
        // Arrange
        var expectedAppSettings = AppSettingsHelper.GetAppSettings();
        _hostingEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns("Development");
        _appSettingsRepositoryMock.Setup(x => x.GetAppSettings()).Returns(expectedAppSettings);
        var appSettingsManager = CreateAppSettingsManager();

        // Act
        var appSettings = await appSettingsManager.GetAppSettings();

        // Assert
        Assert.Same(expectedAppSettings, appSettings);
        _secretsManagerMock.Verify(x => x.GetSecretAndCacheIt(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAppSettingsOverridesSecretsInNonDevelopmentEnvironment()
    {
        // Arrange
        var initialAppSettings = AppSettingsHelper.GetAppSettings();
        _hostingEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns("Production");
        _appSettingsRepositoryMock.Setup(x => x.GetAppSettings()).Returns(initialAppSettings);
        _secretsManagerMock.Setup(x => x.GetSecretAndCacheIt("int-modularNetConfig-encryptionSalt"))
            .ReturnsAsync("prod-salt");
        _secretsManagerMock.Setup(x => x.GetSecretAndCacheIt("int-modularNetConfig-initializationVector"))
            .ReturnsAsync("prod-iv");
        _secretsManagerMock.Setup(x => x.GetSecretAndCacheIt("int-azureMailService-connectionString"))
            .ReturnsAsync("prod-conn-string");
        _secretsManagerMock.Setup(x => x.GetSecretAndCacheIt("int-azureMailService-senderEmail"))
            .ReturnsAsync("prod-sender@example.com");

        var appSettingsManager = CreateAppSettingsManager();

        // Act
        var appSettings = await appSettingsManager.GetAppSettings();

        // Assert
        Assert.Equal("prod-salt", appSettings.ModularNetConfig.EncryptionSalt);
        Assert.Equal("prod-iv", appSettings.ModularNetConfig.InitializationVector);
        Assert.Equal("prod-conn-string", appSettings.AzureEmailService.ConnectionString);
        Assert.Equal("prod-sender@example.com", appSettings.AzureEmailService.SenderEmail);
    }
}
