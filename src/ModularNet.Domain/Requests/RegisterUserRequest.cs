namespace ModularNet.Domain.Requests;

public class RegisterUserRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? UserOid { get; set; }
    public DateTime TermsAndConditionsAcceptedOn { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime AuthenticatedOn { get; set; }
}