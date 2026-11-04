using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;

namespace ModularNet.Business.Implementations;

public class UserNameManager : IUserNameManager
{
    // Change this key to CacheType.Redis to select Redis for this manager.
    private const CacheType UserNameCacheType = CacheType.InMemory;
    private const int CacheExpirationSeconds = 300;
    private readonly IUsersManager _usersManager;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<UserNameManager> _logger;

    public UserNameManager(IUsersManager usersManager, ICacheManager cacheManager, ILogger<UserNameManager> logger)
    {
        _usersManager = usersManager;
        _cacheManager = cacheManager;
        _logger = logger;
    }

    public async Task<string?> GetUserFirstNameById(Guid userId)
    {
        _logger.LogDebug("Start method {Method}", nameof(GetUserFirstNameById));
        var cacheKey = $"user:{userId}:first-name";
        var firstName = await _cacheManager.GetFromCache<string>(cacheKey, UserNameCacheType);
        if (firstName != null) return firstName;

        var user = await _usersManager.GetUserById(userId);
        if (user == null) return null;

        await _cacheManager.SaveInCache(cacheKey, user.FirstName, UserNameCacheType, CacheExpirationSeconds);
        return user.FirstName;
    }
}
