namespace ModularNet.Domain.Requests;

public class VerifyAuthenticationRequest
{
    public string AccessToken { get; set; } = string.Empty;
}