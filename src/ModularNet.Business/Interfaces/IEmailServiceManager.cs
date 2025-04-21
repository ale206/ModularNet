using Azure.Communication.Email;
using ModularNet.Domain.Entities;

namespace ModularNet.Business.Interfaces;

public interface IEmailServiceManager
{
    Task<EmailClient> GetEmailClient();

    Task<string?> SendEmail(NewModularNetEmail newModularNetEmail);

    Task PrepareEmailVerificationEmailAndSend(string userEmail, string emailVerificationCode);
    Task PreparePasswordResetEmailAndSend(string email, string passwordResetLink);
}