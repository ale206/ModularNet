using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class SecretsManager : ISecretsManager
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<SecretsManager> _logger;
    private readonly ISecretsRepository _secretsRepository;

    public SecretsManager(ISecretsRepository secretsRepository, ILogger<SecretsManager> logger,
        ICacheManager cacheManager)
    {
        _secretsRepository = secretsRepository;
        _logger = logger;
        _cacheManager = cacheManager;

        _logger.LogDebug($"{nameof(SecretsManager)} constructed");
    }

    public async Task<string?> GetSecretAndCacheIt(string secretName)
    {
        _logger.LogDebug($"Start method {nameof(GetSecretAndCacheIt)}");

        var secretFromCache = await _cacheManager.GetFromCache<string>(secretName, CacheType.InMemory);

        if (secretFromCache == null)
        {
            var secretValue = await _secretsRepository.GetSecret(secretName) ?? string.Empty;

            var cacheExpirationInSeconds = 604800; // One week
            await _cacheManager.SaveInCache(secretName, secretValue, CacheType.InMemory, cacheExpirationInSeconds);

            return secretValue;
        }

        if (secretFromCache == null) throw new Exception("Secret not retrieved correctly from cache");

        return secretFromCache;
    }

    // Uncomment this method if you want to set a secret in the repository
    // public async Task SetSecret(string secretName, string secretValue)
    // {
    //     _logger.LogDebug($"Start method {nameof(SetSecret)}");
    //
    //     await _secretsRepository.SetSecret(secretName, secretValue);
    // }
}