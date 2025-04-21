using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class AuthManager : IAuthManager
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICacheManager _cacheManager;
    private readonly IEmailServiceManager _emailServiceManager;
    private readonly ILogger<AuthManager> _logger;
    private readonly IUsersManager _usersManager;

    public AuthManager(ILogger<AuthManager> logger, IAuthenticationService authenticationService,
        IEmailServiceManager emailServiceManager, ICacheManager cacheManager, IUsersManager usersManager,
        IAppSettingsManager appSettingsManager)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _emailServiceManager = emailServiceManager;
        _cacheManager = cacheManager;
        _usersManager = usersManager;
        _appSettingsManager = appSettingsManager;

        _logger.LogDebug($"{nameof(AuthManager)} constructed");
    }

    public async Task<string?> RegisterUserInFirebase(string email, string password)
    {
        try
        {
            return await _authenticationService.RegisterAsync(email, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error registering user in Firebase. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> LoginUserInFirebase(string email, string password)
    {
        try
        {
            var appSettings = await _appSettingsManager.GetAppSettings();

            return await _authenticationService.GetJwtTokenFromFirebaseAsync(email, password,
                appSettings.WebAppConfigs.FirebaseApiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error Login user in Firebase. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> VerifyAuthenticationInFirebase(string accessToken)
    {
        return await _authenticationService.VerifyAuthenticationInFirebase(accessToken);
    }

    public async Task LogoutFromFirebase(string uId)
    {
        await _authenticationService.LogoutAsync(uId);
    }

    public async Task ResetPasswordInFirebase(string email)
    {
        var passwordResetLink = await _authenticationService.GeneratePasswordResetLinkAsync(email);

        await _emailServiceManager.PreparePasswordResetEmailAndSend(email, passwordResetLink);
    }

    public async Task<IEnumerable<string>> GetProviders(string uid)
    {
        return await _authenticationService.GetProviders(uid);
    }

    public async Task ClearCache(string userId)
    {
        var usrPswItem = $"{userId}-psw";
        await _cacheManager.RemoveFromCache(usrPswItem, CacheType.InMemory);
    }

    public async Task<string> GetUserToken(string email)
    {
        var user = await _usersManager.GetUserByEmail(email);

        if (user == null)
            throw new Exception("User not found");

        var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(user.UserOid);
        return token;
    }
}