namespace ModularNet.Domain.Requests;

public class SignInUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}