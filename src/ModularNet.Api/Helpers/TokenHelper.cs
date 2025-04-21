using FirebaseAdmin.Auth;

namespace ModularNet.Api.Helpers;

public class TokenHelper : ITokenHelper
{
    public async Task<string?> GetEmailFromToken(HttpContext context)
    {
        if (context.Items.TryGetValue("User", out var userToken) && userToken is FirebaseToken token)
        {
            var email = token.Claims.FirstOrDefault(c => c.Key == "email").Value?.ToString();
            return email;
        }

        return null;
    }
}