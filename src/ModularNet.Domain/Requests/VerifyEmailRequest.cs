namespace ModularNet.Domain.Requests;

public class VerifyEmailRequest
{
    public string EmailVerificationCode { get; set; } = string.Empty;
}