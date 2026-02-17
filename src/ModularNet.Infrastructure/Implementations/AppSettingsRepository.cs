using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppSettingsRepository> _logger;

    public AppSettingsRepository(IConfiguration configuration, ILogger<AppSettingsRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _logger.LogDebug($"{nameof(AppSettingsRepository)} constructed");
    }

    public AppSettings GetAppSettings()
    {
        _logger.LogDebug($"Start method {nameof(GetAppSettings)}");

        return _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
    }
}
