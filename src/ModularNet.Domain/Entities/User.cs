namespace ModularNet.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Username { get; set; }

    public DateTime? AuthenticatedOn { get; set; }
    public string? UserOid { get; set; }
    public bool IsEmailVerified { get; set; }

    public string InitializationVector { get; set; } = string.Empty;

    public DateTime? TermsAndConditionsAcceptedOn { get; set; }
    public string? EmailVerificationCode { get; set; }
}