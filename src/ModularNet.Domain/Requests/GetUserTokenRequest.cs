namespace ModularNet.Domain.Requests;

public class GetUserTokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string OneAccessCode { get; set; } = string.Empty;
}