using ModularNet.Domain.Entities;

namespace ModularNet.UnitTests.Helpers;

public static class AppSettingsHelper
{
    public static AppSettings GetAppSettings()
    {
        // Return a valid AppSettings object
        return new AppSettings
        {
            // Mock all the properties
            ModularNetConfig = new ModularNetConfig
            {
                FrontEndBaseUrl = "https://modularNet.com",
                IsEmailServiceEnabled = true,
                EncryptionSalt = "encryptionSalt",
                MemoryCacheEnabled = true,
                RedisCacheEnabled = true
            },
            AzureEmailService = new AzureEmailService
            {
                ConnectionString = "connectionString",
                SenderEmail = "senderEmail"
            },
            LogsConfig = new LogsConfig
            {
                MinimumLevel = "Debug"
            },
            KeyVault = new KeyVault
            {
                VaultUri = "vaultUri"
            }
        };
    }
}