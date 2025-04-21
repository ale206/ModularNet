using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class AppSettingsRepository
{
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<AppSettingsRepository> _logger;
    private readonly ISecretsRepository _secretsRepository;

    public AppSettingsRepository(IConfiguration configuration, IHostingEnvironment hostingEnvironment,
        ISecretsRepository secretsRepository, ILogger<AppSettingsRepository> logger)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        _secretsRepository = secretsRepository;
        _logger = logger;

        _logger.LogDebug($"{nameof(AppSettingsRepository)} constructed");
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
            await _secretsRepository.GetSecretAndCacheIt("int-modularNetConfig-encryptionSalt") ?? string.Empty;
        appSettings.ModularNetConfig.InitializationVector =
            await _secretsRepository.GetSecretAndCacheIt("int-modularNetConfig-initializationVector") ?? string.Empty;

        // Azure Email Service Configs
        appSettings.AzureEmailService.ConnectionString =
            await _secretsRepository.GetSecretAndCacheIt("int-azureMailService-connectionString") ?? string.Empty;
        appSettings.AzureEmailService.SenderEmail =
            await _secretsRepository.GetSecretAndCacheIt("int-azureMailService-senderEmail") ?? string.Empty;

        return appSettings;
    }
}