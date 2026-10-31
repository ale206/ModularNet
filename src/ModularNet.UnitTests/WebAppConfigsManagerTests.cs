using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class WebAppConfigsManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();
    private readonly Mock<ILogger<WebAppConfigsManager>> _loggerMock = new();

    private WebAppConfigsManager CreateWebAppConfigsManager()
    {
        return new WebAppConfigsManager(_loggerMock.Object, _appSettingsManagerMock.Object);
    }

    [Fact]
    public async Task GetWebAppConfigsReturnsWebAppConfigsFromAppSettings()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.WebAppConfigs = new WebAppConfigs
        {
            FirebaseApiKey = "custom-key",
            FirebaseProjectId = "custom-project",
            FirebaseAuthDomain = "custom.firebaseapp.com"
        };
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var webAppConfigsManager = CreateWebAppConfigsManager();

        // Act
        var webAppConfigs = await webAppConfigsManager.GetWebAppConfigs();

        // Assert
        Assert.NotNull(webAppConfigs);
        Assert.Equal("custom-key", webAppConfigs.FirebaseApiKey);
        Assert.Equal("custom-project", webAppConfigs.FirebaseProjectId);
        Assert.Equal("custom.firebaseapp.com", webAppConfigs.FirebaseAuthDomain);
    }
}
