namespace ModularNet.Business.Interfaces;

public interface IEmailVerifierManager
{
    Task<bool> VerifyEmail(string emailVerificationCode);
}