namespace ModularNet.Domain.Requests;

public class SignUpUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool TermsAccepted { get; set; } = false;
}