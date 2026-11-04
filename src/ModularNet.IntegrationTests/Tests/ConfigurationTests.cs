using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Domain.Entities;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class ConfigurationTests
{
    private readonly IConfiguration _configuration;

    public ConfigurationTests(ModularNetWebApplicationFactory factory)
    {
        _configuration = factory.Services.GetRequiredService<IConfiguration>();
    }

    [Fact]
    public void GetConfigurationFromAppSettingsReturnsPopulatedConfiguration()
    {
        // Arrange & Act
        var connectionString = _configuration.GetConnectionString("ModularNetConnectionString");
        var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();

        // Assert
        Assert.NotNull(connectionString);
        Assert.NotNull(appSettings);
        Assert.NotNull(appSettings.ModularNetConfig);
    }
}
