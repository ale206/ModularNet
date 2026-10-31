using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class AppSettingsManagerTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IAppSettingsManager _appSettingsManager;

    public AppSettingsManagerTests(ModularNetWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _appSettingsManager = _scope.ServiceProvider.GetRequiredService<IAppSettingsManager>();
    }

    [Fact]
    public async Task GetAppSettingsReturnsPopulatedAppSettings()
    {
        // Arrange & Act
        var appSettings = await _appSettingsManager.GetAppSettings();

        // Assert
        Assert.NotNull(appSettings);
        Assert.NotNull(appSettings.ModularNetConfig);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
