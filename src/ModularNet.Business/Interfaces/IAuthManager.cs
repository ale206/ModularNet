namespace ModularNet.Business.Interfaces;

public interface IAuthManager
{
    Task<string?> RegisterUserInFirebase(string email, string password);
    Task<string?> LoginUserInFirebase(string email, string password);
    Task<bool> VerifyAuthenticationInFirebase(string accessToken);
    Task LogoutFromFirebase(string uId);
    Task ResetPasswordInFirebase(string email);
    Task<IEnumerable<string>> GetProviders(string uid);
    Task ClearCache(string userId);
    Task<string> GetUserToken(string email);
}