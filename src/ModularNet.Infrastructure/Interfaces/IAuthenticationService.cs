namespace ModularNet.Infrastructure.Interfaces;

public interface IAuthenticationService
{
    Task<string?> RegisterAsync(string email, string password);
    Task<string?> GetJwtTokenFromFirebaseAsync(string email, string password, string apiKey);
    Task<bool> VerifyAuthenticationInFirebase(string accessToken);
    Task LogoutAsync(string uid);
    Task<string> GeneratePasswordResetLinkAsync(string email);
    Task<IEnumerable<string>> GetProviders(string uid);
}