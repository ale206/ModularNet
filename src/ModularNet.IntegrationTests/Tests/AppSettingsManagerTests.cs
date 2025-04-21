using ModularNet.Business.Interfaces;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

public class AppSettingsManagerTests
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly ISecretsManager _secretsManager;

    public AppSettingsManagerTests(IAppSettingsManager appSettingsManager, ISecretsManager secretsManager)
    {
        _appSettingsManager = appSettingsManager;
        _secretsManager = secretsManager;
    }

    public async Task RunAllTests()
    {
        await GetAppSettings();
    }

    private async Task GetAppSettings()
    {
        var appSettings = await _appSettingsManager.GetAppSettings();

        Assert.NotNull(appSettings);
    }
}