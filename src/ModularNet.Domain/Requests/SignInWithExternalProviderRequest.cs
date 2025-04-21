namespace ModularNet.Domain.Requests;

public class SignInWithExternalProviderRequest
{
    /// <summary>
    ///     Access token to be used in the Authorization header to call API
    /// </summary>
    public string IdToken { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    /// <summary>
    ///     User Id in Firebase
    /// </summary>
    public string Uid { get; set; } = string.Empty;

    public string? RefreshToken { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }
    public string ProviderId { get; set; } = string.Empty;
}