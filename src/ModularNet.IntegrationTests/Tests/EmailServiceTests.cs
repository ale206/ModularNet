using Azure.Communication.Email;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

public class EmailServiceTests
{
    private readonly IEmailServiceManager _emailServiceManager;

    public EmailServiceTests(IEmailServiceManager emailServiceManager)
    {
        _emailServiceManager = emailServiceManager;
    }

    public async Task RunAllTests()
    {
        await SendEmail();
    }

    private async Task SendEmail()
    {
        var emailRecipients = new EmailRecipients(
            new List<EmailAddress>
            {
                new("yourEmail@gmail.com")
            });

        var newModularNetEmail = new NewModularNetEmail
        {
            //Html = "",
            Subject = "Test Email",
            PlainText = "Testing Email",
            EmailRecipients = emailRecipients
        };

        var messageId = await _emailServiceManager.SendEmail(newModularNetEmail);

        Assert.NotNull(messageId);
    }
}