using System.Web;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace ModularNet.Business.Implementations;

public class EmailServiceManager : IEmailServiceManager
{
    private readonly IAppSettingsManager _appSettingsManager;
    private readonly IEmailServiceRepository _emailServiceRepository;
    private readonly ILogger<EmailServiceManager> _logger;

    public EmailServiceManager(IAppSettingsManager appSettingsManager, IEmailServiceRepository emailServiceRepository,
        ILogger<EmailServiceManager> logger)
    {
        _appSettingsManager = appSettingsManager;
        _emailServiceRepository = emailServiceRepository;
        _logger = logger;

        _logger.LogDebug($"{nameof(EmailServiceManager)} constructed");
    }

    public async Task<EmailClient> GetEmailClient()
    {
        _logger.LogDebug($"Start method {nameof(GetEmailClient)}");

        var appSettings = await _appSettingsManager.GetAppSettings();

        var emailClient = new EmailClient(appSettings.AzureEmailService.ConnectionString);

        return emailClient;
    }

    public async Task<string?> SendEmail(NewModularNetEmail newModularNetEmail)
    {
        _logger.LogDebug($"Start method {nameof(SendEmail)}");

        var appSettings = await _appSettingsManager.GetAppSettings();

        if (!appSettings.ModularNetConfig.IsEmailServiceEnabled) return null;

        var emailClient = await GetEmailClient();

        //Map to ModularNetEmail
        var emailId = Guid.NewGuid();

        var senderEmail = appSettings.AzureEmailService.SenderEmail;

        var modularNetEmail = new ModularNetEmail
        {
            Subject = newModularNetEmail.Subject,
            Html = newModularNetEmail.Html,
            Id = emailId,
            EmailRecipients = newModularNetEmail.EmailRecipients,
            PlainText = newModularNetEmail.PlainText,
            EmailRecipientsJson = JsonConvert.SerializeObject(newModularNetEmail.EmailRecipients),
            SenderEmail = senderEmail
        };

        await _emailServiceRepository.SaveEmail(modularNetEmail, false);

        var emailContent = new EmailContent(modularNetEmail.Subject)
        {
            PlainText = modularNetEmail.PlainText,
            Html = modularNetEmail.Html
        };

        var emailMessage = new EmailMessage(modularNetEmail.SenderEmail, modularNetEmail.EmailRecipients, emailContent);

        string? messageId = null;
        try
        {
            var sendEmailResult = await emailClient.SendAsync(WaitUntil.Started, emailMessage);

            messageId = sendEmailResult.Id;

            await _emailServiceRepository.UpdateEmailWithMessageId(emailId, messageId);

            if (!string.IsNullOrEmpty(messageId))
            {
                _logger.LogDebug("Email sent, MessageId = {MessageId}", messageId);
            }
            else
            {
                _logger.LogError("Failed to send email");
                return messageId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email. MessageId: {MessageId}", messageId);
            throw;
        }

        return messageId;
    }

    public async Task PrepareEmailVerificationEmailAndSend(string userEmail, string emailVerificationCode)
    {
        _logger.LogDebug($"Start method {nameof(PrepareEmailVerificationEmailAndSend)}");

        // Specify the file path to your HTML file
        const string htmlFilePath = "EmailTemplates/ConfirmYourEmail.html";

        // Read the HTML content from the file
        var html = File.ReadAllText(htmlFilePath);

        var appSettings = await _appSettingsManager.GetAppSettings();
        var encodedEmailVerificationCode = HttpUtility.UrlEncode(emailVerificationCode);
        var link = $"{appSettings.ModularNetConfig.FrontEndBaseUrl}/verify-email?code={encodedEmailVerificationCode}";

        // Replace {{link}} with the specified link
        html = html.Replace("{{link}}", $"<a href=\"{link}\">Click here to verify your email</a>");

        var emailRecipients = new EmailRecipients(
            new List<EmailAddress>
            {
                new(userEmail)
            });

        //TODO: Get these messages from DB
        var subject = "Confirm your email in ModularNet";

        var newModularNetEmail = new NewModularNetEmail
        {
            Html = html,
            EmailRecipients = emailRecipients,
            Subject = subject
        };

        // Do not wait
        SendEmail(newModularNetEmail);
    }

    public async Task PreparePasswordResetEmailAndSend(string email, string passwordResetLink)
    {
        _logger.LogDebug($"Start method {nameof(PreparePasswordResetEmailAndSend)}");

        // Specify the file path to your HTML file
        const string htmlFilePath = "EmailTemplates/ResetPassword.html";

        // Read the HTML content from the file
        var html = File.ReadAllText(htmlFilePath);

        var link = passwordResetLink;

        // Replace {{link}} with the specified link
        html = html.Replace("{{link}}", $"<a href=\"{link}\">Click here to reset your password</a>");

        var emailRecipients = new EmailRecipients(
            new List<EmailAddress>
            {
                new(email)
            });

        //TODO: Get these messages from DB
        var subject = "Reset your password in ModularNet";

        var newModularNetEmail = new NewModularNetEmail
        {
            Html = html,
            EmailRecipients = emailRecipients,
            Subject = subject
        };

        // Do not wait
        SendEmail(newModularNetEmail);
    }
}