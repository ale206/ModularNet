using Newtonsoft.Json;

namespace ModularNet.Domain.Entities;

[JsonObject("AppSettings")]
public class AppSettings
{
    public KeyVault KeyVault { get; set; } = new();
    public LogsConfig LogsConfig { get; set; } = new();
    public ModularNetConfig ModularNetConfig { get; set; } = new();
    public AzureEmailService AzureEmailService { get; set; } = new();
    public WebAppConfigs WebAppConfigs { get; set; } = new();
    public FirebaseConfig FirebaseConfig { get; set; }
}

public class FirebaseConfig
{
    public string TokenUri { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ValidIssuer { get; set; } = string.Empty;
}

public class WebAppConfigs
{
    public string FirebaseApiKey { get; set; } = string.Empty;
    public string FirebaseAuthDomain { get; set; } = string.Empty;
    public string FirebaseProjectId { get; set; } = string.Empty;
    public string FirebaseStorageBucket { get; set; } = string.Empty;
    public string FirebaseMessagingSenderId { get; set; } = string.Empty;
    public string FirebaseAppId { get; set; } = string.Empty;
    public string FirebaseMeasurementId { get; set; } = string.Empty;
}

public class LogsConfig
{
    public string MinimumLevel { get; set; } = string.Empty;
}

public class AzureEmailService
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
}

public class ModularNetConfig
{
    public string FrontEndBaseUrl { get; set; } = string.Empty;
    public bool IsEmailServiceEnabled { get; set; }
    public string EncryptionSalt { get; set; } = string.Empty;
    public string InitializationVector { get; set; }
    public bool MemoryCacheEnabled { get; set; }
    public bool RedisCacheEnabled { get; set; }
}

public class KeyVault
{
    public string VaultUri { get; set; } = string.Empty;
}