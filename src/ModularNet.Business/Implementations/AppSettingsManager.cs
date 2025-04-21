using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;

namespace ModularNet.Business.Implementations;

public class AppSettingsManager : IAppSettingsManager
{
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<AppSettingsManager> _logger;
    private readonly ISecretsManager _secretsManager;

    public AppSettingsManager(IConfiguration configuration, IHostingEnvironment hostingEnvironment,
        ISecretsManager secretsManager, ILogger<AppSettingsManager> logger)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        _secretsManager = secretsManager;
        _logger = logger;

        _logger.LogDebug($"{nameof(AppSettingsManager)} constructed");
    }

    /// <summary>
    ///     If not DEVELOPMENT, override the fields that have to be taken from Key Vault
    /// </summary>
    /// <returns></returns>
    public async Task<AppSettings> GetAppSettings()
    {
        _logger.LogDebug($"Start method {nameof(GetAppSettings)}");

        var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

        // If Development, return current app settings
        if (_hostingEnvironment.IsDevelopment())
            return appSettings;

        // If not DEVELOPMENT, override the fields that have to be taken from Key Vault

        // ModularNet Configs
        appSettings.ModularNetConfig.EncryptionSalt =
            await _secretsManager.GetSecretAndCacheIt("int-modularNetConfig-encryptionSalt") ?? string.Empty;
        appSettings.ModularNetConfig.InitializationVector =
            await _secretsManager.GetSecretAndCacheIt("int-modularNetConfig-initializationVector") ?? string.Empty;

        // Azure Email Service Configs
        appSettings.AzureEmailService.ConnectionString =
            await _secretsManager.GetSecretAndCacheIt("int-azureMailService-connectionString") ?? string.Empty;
        appSettings.AzureEmailService.SenderEmail =
            await _secretsManager.GetSecretAndCacheIt("int-azureMailService-senderEmail") ?? string.Empty;

        return appSettings;
    }
}