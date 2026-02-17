using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class InMemoryCacheRepository : IInMemoryCacheRepository
{
    private readonly bool _memoryCacheEnabled;
    private readonly ILogger<InMemoryCacheRepository> _logger;
    private readonly List<InMemoryCacheItem> _memoryCacheItems = new();
    private readonly object _syncLock = new(); // Lock object

    public InMemoryCacheRepository(ILogger<InMemoryCacheRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        _memoryCacheEnabled = appSettings.ModularNetConfig.MemoryCacheEnabled;
    }

    public async Task SaveToInMemoryCache<T>(string item, T value, int cacheExpirationSeconds)
    {
        if (!_memoryCacheEnabled) return;

        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(SaveToInMemoryCache)}");

            // Remove expired items before saving
            RemoveExpiredItemsFromInMemoryCache();

            if (value == null)
                return;

            // Remove the value, if exists, to overwrite it
            var valueFromMemory = GetFromInMemoryCache<T>(item);
            if (valueFromMemory != null)
                _memoryCacheItems.RemoveAll(x => x.ItemKey == item);

            _memoryCacheItems.Add(new InMemoryCacheItem
            {
                ExpirationDate = DateTime.UtcNow.AddSeconds(cacheExpirationSeconds),
                ItemKey = item,
                ItemValue = value
            });
        }
    }

    public async Task<T?> GetFromInMemoryCache<T>(string item)
    {
        if (!_memoryCacheEnabled) return default;

        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(GetFromInMemoryCache)}");

            // Remove expired items before retrieving
            RemoveExpiredItemsFromInMemoryCache();

            var cacheItem = _memoryCacheItems.FirstOrDefault(x => x.ItemKey == item);

            if (cacheItem == null || cacheItem.ItemValue == null) return default;

            return (T?)cacheItem.ItemValue;
        }
    }

    public async Task RemoveFromInMemoryCache(string item)
    {
        if (!_memoryCacheEnabled) return;

        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(RemoveFromInMemoryCache)}");
            _memoryCacheItems.RemoveAll(x => x.ItemKey == item);
        }
    }

    public async Task RemoveExpiredItemsFromInMemoryCache()
    {
        if (!_memoryCacheEnabled) return;

        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(RemoveExpiredItemsFromInMemoryCache)}");
            _memoryCacheItems.RemoveAll(item => item.ExpirationDate <= DateTime.UtcNow);
        }
    }
}