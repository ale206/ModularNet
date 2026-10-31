using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class EmailVerifierManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();
    private readonly Mock<IEncryptManager> _encryptManagerMock = new();
    private readonly Mock<ILogger<EmailVerifierManager>> _loggerMock = new();
    private readonly Mock<IUsersManager> _usersManagerMock = new();

    private EmailVerifierManager CreateEmailVerifierManager()
    {
        return new EmailVerifierManager(
            _encryptManagerMock.Object,
            _loggerMock.Object,
            _appSettingsManagerMock.Object,
            _usersManagerMock.Object);
    }

    [Fact]
    public async Task VerifyEmailReturnsTrueWhenVerificationCodeIsValid()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        const string email = "user@example.com";
        var expirationDate = DateTime.UtcNow.AddDays(1).ToString("o");
        var decryptedPayload = $"{email}|{expirationDate}";
        var rawBytes = new byte[] { 1, 2, 3 };
        var base64Code = Convert.ToBase64String(rawBytes);

        _encryptManagerMock
            .Setup(x => x.Decrypt(It.IsAny<byte[]>(), appSettings.ModularNetConfig.EncryptionSalt, appSettings.ModularNetConfig.InitializationVector))
            .ReturnsAsync(decryptedPayload);

        var user = UsersHelper.GetUser();
        user.Email = email;
        user.EmailVerificationCode = base64Code;
        _usersManagerMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);
        _usersManagerMock.Setup(x => x.SetUserEmailAsVerified(user.Id)).Returns(Task.CompletedTask);

        var emailVerifierManager = CreateEmailVerifierManager();

        // Act
        var result = await emailVerifierManager.VerifyEmail(base64Code);

        // Assert
        Assert.True(result);
        _usersManagerMock.Verify(x => x.SetUserEmailAsVerified(user.Id), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailThrowsWhenDecryptedPayloadHasInvalidFormat()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        var rawBytes = new byte[] { 1, 2, 3 };
        var base64Code = Convert.ToBase64String(rawBytes);

        _encryptManagerMock
            .Setup(x => x.Decrypt(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("invalid_payload_without_pipe");

        var emailVerifierManager = CreateEmailVerifierManager();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            emailVerifierManager.VerifyEmail(base64Code));
        Assert.Equal("Wrong verification code", exception.Message);
    }

    [Fact]
    public async Task VerifyEmailThrowsWhenUserNotFound()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        const string email = "nonexistent@example.com";
        var expirationDate = DateTime.UtcNow.AddDays(1).ToString("o");
        var decryptedPayload = $"{email}|{expirationDate}";
        var rawBytes = new byte[] { 1, 2, 3 };
        var base64Code = Convert.ToBase64String(rawBytes);

        _encryptManagerMock
            .Setup(x => x.Decrypt(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(decryptedPayload);

        _usersManagerMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((User?)null);

        var emailVerifierManager = CreateEmailVerifierManager();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            emailVerifierManager.VerifyEmail(base64Code));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task VerifyEmailThrowsWhenCodeIsExpired()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        const string email = "expired@example.com";
        var expirationDate = DateTime.UtcNow.AddDays(-2).ToString("o");
        var decryptedPayload = $"{email}|{expirationDate}";
        var rawBytes = new byte[] { 1, 2, 3 };
        var base64Code = Convert.ToBase64String(rawBytes);

        _encryptManagerMock
            .Setup(x => x.Decrypt(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(decryptedPayload);

        var user = UsersHelper.GetUser();
        user.Email = email;
        _usersManagerMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

        var emailVerifierManager = CreateEmailVerifierManager();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            emailVerifierManager.VerifyEmail(base64Code));
        Assert.Equal("Code Expired", exception.Message);
    }

    [Fact]
    public async Task VerifyEmailThrowsWhenCodeDoesNotMatchUserVerificationCode()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        const string email = "user@example.com";
        var expirationDate = DateTime.UtcNow.AddDays(1).ToString("o");
        var decryptedPayload = $"{email}|{expirationDate}";
        var rawBytes = new byte[] { 1, 2, 3 };
        var base64Code = Convert.ToBase64String(rawBytes);

        _encryptManagerMock
            .Setup(x => x.Decrypt(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(decryptedPayload);

        var user = UsersHelper.GetUser();
        user.Email = email;
        user.EmailVerificationCode = "different-verification-code";
        _usersManagerMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

        var emailVerifierManager = CreateEmailVerifierManager();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            emailVerifierManager.VerifyEmail(base64Code));
        Assert.Equal("Something went wrong verifying the email", exception.Message);
    }
}
