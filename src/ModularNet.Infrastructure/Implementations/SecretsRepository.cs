using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class SecretsRepository : ISecretsRepository
{
    private readonly AppSettings _appSettings;
    private readonly IInMemoryCacheRepository _inMemoryCacheRepository;
    private readonly ILogger<SecretsRepository> _logger;

    public SecretsRepository(IConfiguration configuration, ILogger<SecretsRepository> logger,
        IInMemoryCacheRepository inMemoryCacheRepository)
    {
        _logger = logger;
        _inMemoryCacheRepository = inMemoryCacheRepository;
        //TODO: Refactor to have this similar to what has been done in Business
        _appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ??
                       throw new Exception("Error getting AppSettings");

        _logger.LogDebug($"{nameof(SecretsRepository)} constructed");
    }

    public async Task<string?> GetSecret(string secretName)
    {
        _logger.LogDebug($"Start repository method {nameof(GetSecret)}");

        var secretClient =
            new SecretClient(new Uri(_appSettings.KeyVault.VaultUri), new DefaultAzureCredential());

        KeyVaultSecret keyValueSecret;

        try
        {
            keyValueSecret = await secretClient.GetSecretAsync(secretName);
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting Secret Value", ex.InnerException);
        }

        return keyValueSecret.Value;
    }

    public async Task<string?> GetSecretAndCacheIt(string secretName)
    {
        _logger.LogDebug($"Start method {nameof(GetSecretAndCacheIt)}");

        var secretFromCache = await _inMemoryCacheRepository.GetFromInMemoryCache<string>(secretName);

        if (secretFromCache == null)
        {
            var secretValue = await GetSecret(secretName) ?? string.Empty;

            var cacheExpirationInSeconds = 604800; // One week
            await _inMemoryCacheRepository.SaveToInMemoryCache(secretName, secretValue, cacheExpirationInSeconds);

            return secretValue;
        }

        if (secretFromCache == null) throw new Exception("Secret not retrieved correctly from cache");

        return secretFromCache;
    }

    // Uncomment this method if you want to set a secret in the repository
    // public async Task SetSecret(string secretName, string secretValue)
    // {
    //     _logger.LogDebug($"Start repository method {nameof(SetSecret)}");
    //
    //     // var secretClient =
    //     //     new SecretClient(new Uri(_appSettings.KeyVault.VaultUri), new DefaultAzureCredential());
    //     //
    //     // await secretClient.SetSecretAsync(secretName, secretValue);
    // }
}