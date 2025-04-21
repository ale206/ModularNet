using FirebaseAdmin.Auth;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly IJwtProvider _jwtProvider;

    public AuthenticationService(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    public async Task<string?> RegisterAsync(string email, string password)
    {
        var userRecordArgs = new UserRecordArgs
        {
            Email = email,
            Password = password
        };

        var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

        return userRecord.Uid;
    }

    // Login
    public async Task<string?> GetJwtTokenFromFirebaseAsync(string email, string password, string apiKey)
    {
        return await _jwtProvider.GetJwtTokenFromFirebase(email, password, apiKey);
    }

    public async Task<bool> VerifyAuthenticationInFirebase(string accessToken)
    {
        var token = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);

        if (token != null)
            return true;

        return false;
    }

    public async Task LogoutAsync(string uid)
    {
        await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(uid);
    }

    public async Task<string> GeneratePasswordResetLinkAsync(string email)
    {
        return await FirebaseAuth.DefaultInstance.GeneratePasswordResetLinkAsync(email);
    }

    public async Task<IEnumerable<string>> GetProviders(string uid)
    {
        var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
        var providers = userRecord.ProviderData.Select(p => p.ProviderId);
        return providers;
    }
}