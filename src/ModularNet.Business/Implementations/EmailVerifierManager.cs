using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;

namespace ModularNet.Business.Implementations;

public class EmailVerifierManager : IEmailVerifierManager
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly IEncryptManager _encryptManager;
    private readonly ILogger<EmailVerifierManager> _logger;
    private readonly IUsersManager _usersManager;

    public EmailVerifierManager(IEncryptManager encryptManager, ILogger<EmailVerifierManager> logger,
        IAppSettingsManager appSettingsManager,
        IUsersManager usersManager)
    {
        _encryptManager = encryptManager;
        _logger = logger;
        _appSettingsManager = appSettingsManager;
        _usersManager = usersManager;

        _logger.LogDebug($"{nameof(EmailVerifierManager)} constructed");
    }

    public async Task<bool> VerifyEmail(string emailVerificationCode)
    {
        var codeInByteArray = Convert.FromBase64String(emailVerificationCode);

        var appSettings = await _appSettingsManager.GetAppSettings();
        var encryptionKey = appSettings.ModularNetConfig.EncryptionSalt;
        var initializationVector = appSettings.ModularNetConfig.InitializationVector;

        var verificationCodeDecrypted =
            await _encryptManager.Decrypt(codeInByteArray, encryptionKey, initializationVector);

        var parts = verificationCodeDecrypted.Split('|');

        string email;
        DateTime codeExpirationDate;
        if (parts.Length == 2)
        {
            email = parts[0];
            codeExpirationDate = Convert.ToDateTime(parts[1]);
        }
        else
        {
            throw new Exception("Wrong verification code");
        }

        var user = await _usersManager.GetUserByEmail(email);

        if (user == null) throw new Exception("User not found");

        if (DateTime.UtcNow > codeExpirationDate)
        {
            _logger.LogWarning($"Code expired on verification email. UserId: {user.Id}");
            throw new Exception("Code Expired");
        }

        if (user.EmailVerificationCode == emailVerificationCode)
        {
            await _usersManager.SetUserEmailAsVerified(user.Id);

            return true;
        }

        throw new Exception("Something went wrong verifying the email");
    }
}