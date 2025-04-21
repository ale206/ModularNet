using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Domain.Requests;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Business.Implementations;

public class UsersManager : IUsersManager
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly IAuditsManager _auditsManager;
    private readonly IAuthenticationService _authenticationService;
    private readonly IEmailServiceManager _emailServiceManager;
    private readonly IEncryptManager _encryptManager;
    private readonly ILogger<UsersManager> _logger;
    private readonly IUsersRepository _usersRepository;

    public UsersManager(IUsersRepository usersRepository, ILogger<UsersManager> logger, IEncryptManager encryptManager,
        IEmailServiceManager emailServiceManager, IAuditsManager auditsManager,
        IAuthenticationService authenticationService, IAppSettingsManager appSettingsManager)
    {
        _usersRepository = usersRepository;
        _logger = logger;
        _encryptManager = encryptManager;
        _emailServiceManager = emailServiceManager;
        _auditsManager = auditsManager;
        _authenticationService = authenticationService;
        _appSettingsManager = appSettingsManager;

        _logger.LogDebug($"{nameof(UsersManager)} constructed");
    }

    public async Task<Guid> RegisterUserInModularNet(RegisterUserRequest registerUserRequest)
    {
        _logger.LogDebug($"Start method {nameof(RegisterUserInModularNet)}");

        // Check if the user is already registered. If its userOid is null, update it, because it was already registered with Azure AD
        // TODO: At this point this piece of code is not needed, because all users will be registered in Firebase
        var userByEmail = await _usersRepository.GetUserByEmail(registerUserRequest.Email);

        if (userByEmail != null && string.IsNullOrEmpty(userByEmail.UserOid))
        {
            // Update the userOid and return the user id
            await _usersRepository.UpdateUserOid(userByEmail.Id, registerUserRequest.UserOid);
            return userByEmail.Id;
        }

        var user = new User
        {
            FirstName = registerUserRequest.FirstName,
            LastName = registerUserRequest.LastName,
            Email = registerUserRequest.Email,
            UserOid = registerUserRequest.UserOid,
            TermsAndConditionsAcceptedOn = registerUserRequest.TermsAndConditionsAcceptedOn,
            IsEmailVerified = registerUserRequest.IsEmailVerified,
            IsEnabled = true, // Always set to true because the enabling is managed by Firebase at login time.
            AuthenticatedOn = registerUserRequest.AuthenticatedOn
        };

        // Register new user
        user.Id = Guid.NewGuid();

        // Generate an initialization vector to encrypt user's data (subscriptions)
        user.InitializationVector = await _encryptManager.GenerateInitializationVector();

        user.CreatedOn = user.AuthenticatedOn ?? DateTime.UtcNow;

        user.EmailVerificationCode = await _encryptManager.GenerateEmailVerificationCode(user.Email);

        try
        {
            await _usersRepository.RegisterUser(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error registering user {user.Email} in ModularNet. Error: {ex.Message}");
            throw;
        }

        // User could already have the email verified as he did the auth with Google
        if (!user.IsEmailVerified)
            await _emailServiceManager.PrepareEmailVerificationEmailAndSend(user.Email, user.EmailVerificationCode);

        return user.Id;
    }

    public async Task LoginUser(UserSignInRequest userSignInRequest)
    {
        _logger.LogDebug($"Start method {nameof(LoginUser)}");

        // Audit login
        var modularNetAudit = new ModularNetAudit { AuditType = AuditType.Login, UserId = userSignInRequest.Id };
        await _auditsManager.WriteAuditToDb(modularNetAudit);

        await _usersRepository.UpdateUserLoginDate(userSignInRequest.Id,
            userSignInRequest.AuthenticatedOn ?? DateTime.UtcNow);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        _logger.LogDebug($"Start method {nameof(GetUserByEmail)}");

        return await _usersRepository.GetUserByEmail(email);
    }

    public async Task<User?> GetUserById(Guid userId)
    {
        _logger.LogDebug($"Start method {nameof(GetUserById)}");

        return await _usersRepository.GetUserById(userId);
    }

    public async Task SetUserEmailAsVerified(Guid userId)
    {
        await _usersRepository.SetUserEmailAsVerified(userId);
    }

    public async Task SetTermsAndConditionsAsAccepted(Guid userId)
    {
        await _usersRepository.SetTermsAndConditionsAsAccepted(userId);
    }

    public async Task DeactivateAccount(Guid userId)
    {
        await _usersRepository.DeactivateAccount(userId);
    }

    public async Task EnableUser(Guid userId)
    {
        await _usersRepository.EnableUser(userId);
    }

    /// <summary>
    ///     If the registration is not done in the Front-end, the user can be registered in Firebase with this method
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<string?> RegisterUserInFirebase(string email, string password)
    {
        try
        {
            return await _authenticationService.RegisterAsync(email, password);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error registering user {email} in Firebase. Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     If the login is not done in the Front-end, the user can be logged in Firebase with this method
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
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
            _logger.LogError($"Error logging user {email} in Firebase. Error: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateUserOid(Guid userId, string? userOid)
    {
        await _usersRepository.UpdateUserOid(userId, userOid);
    }

    public async Task<Guid> GetUserIdByEmail(string emailFromToken)
    {
        return await _usersRepository.GetUserIdByEmail(emailFromToken);
    }
}