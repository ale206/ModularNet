using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class EmailServiceManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();
    private readonly Mock<IEmailServiceRepository> _emailServiceRepositoryMock = new();
    private readonly Mock<ILogger<EmailServiceManager>> _loggerMock = new();

    private EmailServiceManager CreateEmailServiceManager()
    {
        return new EmailServiceManager(
            _appSettingsManagerMock.Object,
            _emailServiceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailReturnsNullWhenEmailServiceIsDisabled()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.IsEmailServiceEnabled = false;
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var emailServiceManager = CreateEmailServiceManager();
        var newModularNetEmail = new NewModularNetEmail
        {
            Subject = "Test Subject",
            PlainText = "Hello World"
        };

        // Act
        var result = await emailServiceManager.SendEmail(newModularNetEmail);

        // Assert
        Assert.Null(result);
        _emailServiceRepositoryMock.Verify(x => x.SaveEmail(It.IsAny<ModularNetEmail>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetEmailClientCreatesClientWithConfiguredConnectionString()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.AzureEmailService.ConnectionString =
            "endpoint=https://testmail.communication.azure.com/;accesskey=dGVzdA==";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var emailServiceManager = CreateEmailServiceManager();

        // Act
        var emailClient = await emailServiceManager.GetEmailClient();

        // Assert
        Assert.NotNull(emailClient);
    }

    [Fact]
    public async Task PrepareEmailVerificationEmailAndSendExecutesWhenEmailServiceDisabled()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.IsEmailServiceEnabled = false;
        appSettings.ModularNetConfig.FrontEndBaseUrl = "https://app.modularnet.com";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var emailServiceManager = CreateEmailServiceManager();

        // Act
        await emailServiceManager.PrepareEmailVerificationEmailAndSend("user@example.com", "code123");

        // Assert
        _appSettingsManagerMock.Verify(x => x.GetAppSettings(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task PreparePasswordResetEmailAndSendExecutesWhenEmailServiceDisabled()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.IsEmailServiceEnabled = false;
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var emailServiceManager = CreateEmailServiceManager();

        // Act
        await emailServiceManager.PreparePasswordResetEmailAndSend("user@example.com", "https://app.modularnet.com/reset");

        // Assert
        _appSettingsManagerMock.Verify(x => x.GetAppSettings(), Times.AtLeastOnce);
    }
}
