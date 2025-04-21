using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ModularNet.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace ModularNet.Infrastructure.Implementations;

public class JwtProvider : IJwtProvider
{
    private readonly HttpClient _httpClient;

    public JwtProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetJwtTokenFromFirebase(string email, string password, string apiKey)
    {
        var request = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
            request);

        var authToken = JsonConvert.DeserializeObject<AuthToken>(await response.Content.ReadAsStringAsync());

        return authToken.IdToken;
    }

    public class AuthToken
    {
        // Kind, localId, email, displayName, idToken, registered, refreshToken, expiresIn
        [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

        [JsonPropertyName("localId")] public string LocalId { get; set; } = string.Empty;

        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;

        [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("idToken")] public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("registered")] public string Registered { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")] public string ExpiresIn { get; set; } = string.Empty;
    }
}