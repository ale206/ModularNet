using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using Moq;

namespace ModularNet.UnitTests;

public class InMemoryCacheRepositoryTests
{
    private readonly Mock<ILogger<InMemoryCacheRepository>> _loggerMock = new();

    private InMemoryCacheRepository CreateInMemoryCacheRepository(bool enabled = true)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "AppSettings:ModularNetConfig:MemoryCacheEnabled", enabled ? "true" : "false" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        return new InMemoryCacheRepository(_loggerMock.Object, configuration);
    }

    [Fact]
    public async Task SaveToInMemoryCacheAndGetFromInMemoryCacheReturnsSavedValue()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        const string key = "user-session-1";
        const string expectedValue = "session-token-abc";

        // Act
        await inMemoryCacheRepository.SaveToInMemoryCache(key, expectedValue, 60);
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Equal(expectedValue, cachedValue);
    }

    [Fact]
    public async Task SaveToInMemoryCacheOverwritesExistingValue()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        const string key = "config-key";

        // Act
        await inMemoryCacheRepository.SaveToInMemoryCache(key, "old-value", 60);
        await inMemoryCacheRepository.SaveToInMemoryCache(key, "new-value", 60);
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Equal("new-value", cachedValue);
    }

    [Fact]
    public async Task SaveToInMemoryCacheDoesNothingWhenValueIsNull()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        const string key = "null-key";

        // Act
        await inMemoryCacheRepository.SaveToInMemoryCache<string?>(key, null, 60);
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task SaveToInMemoryCacheDoesNothingWhenCacheDisabled()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: false);
        const string key = "disabled-key";

        // Act
        await inMemoryCacheRepository.SaveToInMemoryCache(key, "some-val", 60);
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task RemoveFromInMemoryCacheDeletesExistingItem()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        const string key = "to-delete";
        await inMemoryCacheRepository.SaveToInMemoryCache(key, "value", 60);

        // Act
        await inMemoryCacheRepository.RemoveFromInMemoryCache(key);
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task RemoveFromInMemoryCacheDoesNothingWhenCacheDisabled()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: false);

        // Act & Assert
        await inMemoryCacheRepository.RemoveFromInMemoryCache("any-key");
    }

    [Fact]
    public async Task GetFromInMemoryCacheReturnsNullWhenItemIsExpired()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        const string key = "expired-key";
        await inMemoryCacheRepository.SaveToInMemoryCache(key, "temp-value", -10);

        // Act
        var cachedValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>(key);

        // Assert
        Assert.Null(cachedValue);
    }

    [Fact]
    public async Task RemoveExpiredItemsFromInMemoryCacheRemovesOnlyExpiredItems()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: true);
        await inMemoryCacheRepository.SaveToInMemoryCache("expired-1", "val1", -5);
        await inMemoryCacheRepository.SaveToInMemoryCache("active-1", "val2", 300);

        // Act
        await inMemoryCacheRepository.RemoveExpiredItemsFromInMemoryCache();
        var expiredValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>("expired-1");
        var activeValue = await inMemoryCacheRepository.GetFromInMemoryCache<string>("active-1");

        // Assert
        Assert.Null(expiredValue);
        Assert.Equal("val2", activeValue);
    }

    [Fact]
    public async Task RemoveExpiredItemsFromInMemoryCacheDoesNothingWhenCacheDisabled()
    {
        // Arrange
        var inMemoryCacheRepository = CreateInMemoryCacheRepository(enabled: false);

        // Act & Assert
        await inMemoryCacheRepository.RemoveExpiredItemsFromInMemoryCache();
    }
}
