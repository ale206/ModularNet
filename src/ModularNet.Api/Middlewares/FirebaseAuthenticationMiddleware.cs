using System.Security.Claims;
using FirebaseAdmin.Auth;

namespace ModularNet.Api.Middlewares;

public class FirebaseAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var extractedToken))
        {
            var auth = FirebaseAuth.DefaultInstance;
            try
            {
                // Remove the "Bearer " prefix
                var tokenString = extractedToken.ToString().Substring("Bearer ".Length);
                var token = await auth.VerifyIdTokenAsync(tokenString);

                // Example: Extract claims from the token
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, token.Uid)
                    // Add other necessary claims
                };

                // Ensure to specify an authentication type
                var identity = new ClaimsIdentity(claims, "FirebaseAuth");
                var principal = new ClaimsPrincipal(identity);

                context.User = principal; // Set the authenticated user

                context.Items["User"] = token;
            }
            catch (Exception ex)
            {
                // The token is invalid
                context.Response.StatusCode = 401; // Unauthorized
                return;
            }
        }

        await _next(context);
    }
}