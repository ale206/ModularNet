using ModularNet.Domain.Entities;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

public class ConfigurationTests
{
    private readonly IConfiguration _configuration;

    public ConfigurationTests(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void RunAllTests()
    {
        GetConfigurationFromAppSettings();
    }

    private void GetConfigurationFromAppSettings()
    {
        var connectionString = _configuration.GetConnectionString("ModularNetConnectionString");
        Assert.NotNull(connectionString);

        var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        Assert.NotNull(appSettings);
    }
}