namespace ModularNet.Infrastructure.Interfaces;

public interface IJwtProvider
{
    Task<string?> GetJwtTokenFromFirebase(string email, string password, string apiKey);
}