using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class InMemoryCacheRepository : IInMemoryCacheRepository
{
    private readonly ILogger<InMemoryCacheRepository> _logger;
    private readonly List<InMemoryCacheItem> _memoryCacheItems = new();
    private readonly object _syncLock = new(); // Lock object

    public InMemoryCacheRepository(ILogger<InMemoryCacheRepository> logger)
    {
        _logger = logger;
    }

    public async Task SaveToInMemoryCache<T>(string item, T value, int cacheExpirationSeconds)
    {
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
        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(RemoveFromInMemoryCache)}");
            _memoryCacheItems.RemoveAll(x => x.ItemKey == item);
        }
    }

    public async Task RemoveExpiredItemsFromInMemoryCache()
    {
        lock (_syncLock)
        {
            _logger.LogDebug($"Start method {nameof(RemoveExpiredItemsFromInMemoryCache)}");
            _memoryCacheItems.RemoveAll(item => item.ExpirationDate <= DateTime.UtcNow);
        }
    }
}