using Azure.Communication.Email;

namespace ModularNet.Domain.Entities;

public class ModularNetEmail : BaseEntity
{
    public string Subject { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public EmailRecipients EmailRecipients { get; set; } = new(new List<EmailAddress>());
    public string? EmailRecipientsJson { get; set; }
    public string? ImportanceValue { get; set; }
}