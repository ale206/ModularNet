using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;

namespace ModularNet.Business.Implementations;

/// <summary>
///     This is used to return Web App Configs to the Web App
/// </summary>
public class WebAppConfigsManager : IWebAppConfigsManager
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly ILogger<WebAppConfigsManager> _logger;

    public WebAppConfigsManager(ILogger<WebAppConfigsManager> logger, IAppSettingsManager appSettingsManager)
    {
        _logger = logger;
        _appSettingsManager = appSettingsManager;

        _logger.LogDebug($"{nameof(UsersManager)} constructed");
    }

    public async Task<WebAppConfigs> GetWebAppConfigs()
    {
        var appSettings = await _appSettingsManager.GetAppSettings();

        return appSettings.WebAppConfigs;
    }
}