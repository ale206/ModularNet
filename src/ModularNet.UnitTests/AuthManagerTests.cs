using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class AuthManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();
    private readonly Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<IEmailServiceManager> _emailServiceManagerMock = new();
    private readonly Mock<ILogger<AuthManager>> _loggerMock = new();
    private readonly Mock<IUsersManager> _usersManagerMock = new();

    private AuthManager CreateAuthManager()
    {
        return new AuthManager(
            _loggerMock.Object,
            _authenticationServiceMock.Object,
            _emailServiceManagerMock.Object,
            _cacheManagerMock.Object,
            _usersManagerMock.Object,
            _appSettingsManagerMock.Object);
    }

    [Fact]
    public async Task RegisterUserInFirebaseReturnsUidWhenSuccessful()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        const string expectedUid = "auth-uid-123";
        _authenticationServiceMock.Setup(x => x.RegisterAsync(email, password))
            .ReturnsAsync(expectedUid);

        // Act
        var uid = await authManager.RegisterUserInFirebase(email, password);

        // Assert
        Assert.Equal(expectedUid, uid);
        _authenticationServiceMock.Verify(x => x.RegisterAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task RegisterUserInFirebaseThrowsWhenExceptionOccurs()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        _authenticationServiceMock.Setup(x => x.RegisterAsync(email, password))
            .ThrowsAsync(new InvalidOperationException("Registration failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authManager.RegisterUserInFirebase(email, password));
    }

    [Fact]
    public async Task LoginUserInFirebaseReturnsTokenWhenSuccessful()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        const string expectedToken = "jwt-token-value";
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        _authenticationServiceMock.Setup(x => x.GetJwtTokenFromFirebaseAsync(email, password, appSettings.WebAppConfigs.FirebaseApiKey))
            .ReturnsAsync(expectedToken);

        // Act
        var token = await authManager.LoginUserInFirebase(email, password);

        // Assert
        Assert.Equal(expectedToken, token);
        _authenticationServiceMock.Verify(
            x => x.GetJwtTokenFromFirebaseAsync(email, password, appSettings.WebAppConfigs.FirebaseApiKey),
            Times.Once);
    }

    [Fact]
    public async Task LoginUserInFirebaseThrowsWhenExceptionOccurs()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings())
            .ThrowsAsync(new InvalidOperationException("Config error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            authManager.LoginUserInFirebase(email, password));
    }

    [Fact]
    public async Task VerifyAuthenticationInFirebaseDelegatesToAuthenticationService()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string token = "access-token";
        _authenticationServiceMock.Setup(x => x.VerifyAuthenticationInFirebase(token))
            .ReturnsAsync(true);

        // Act
        var isValid = await authManager.VerifyAuthenticationInFirebase(token);

        // Assert
        Assert.True(isValid);
        _authenticationServiceMock.Verify(x => x.VerifyAuthenticationInFirebase(token), Times.Once);
    }

    [Fact]
    public async Task LogoutFromFirebaseDelegatesToAuthenticationService()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string uid = "user-uid-456";
        _authenticationServiceMock.Setup(x => x.LogoutAsync(uid)).Returns(Task.CompletedTask);

        // Act
        await authManager.LogoutFromFirebase(uid);

        // Assert
        _authenticationServiceMock.Verify(x => x.LogoutAsync(uid), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordInFirebaseGeneratesLinkAndSendsEmail()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "reset@example.com";
        const string resetLink = "https://modularnet.com/reset-link";
        _authenticationServiceMock.Setup(x => x.GeneratePasswordResetLinkAsync(email))
            .ReturnsAsync(resetLink);
        _emailServiceManagerMock.Setup(x => x.PreparePasswordResetEmailAndSend(email, resetLink))
            .Returns(Task.CompletedTask);

        // Act
        await authManager.ResetPasswordInFirebase(email);

        // Assert
        _authenticationServiceMock.Verify(x => x.GeneratePasswordResetLinkAsync(email), Times.Once);
        _emailServiceManagerMock.Verify(x => x.PreparePasswordResetEmailAndSend(email, resetLink), Times.Once);
    }

    [Fact]
    public async Task GetProvidersDelegatesToAuthenticationService()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string uid = "user-uid-789";
        var expectedProviders = new List<string> { "password", "google.com" };
        _authenticationServiceMock.Setup(x => x.GetProviders(uid)).ReturnsAsync(expectedProviders);

        // Act
        var providers = await authManager.GetProviders(uid);

        // Assert
        Assert.Equal(expectedProviders, providers);
        _authenticationServiceMock.Verify(x => x.GetProviders(uid), Times.Once);
    }

    [Fact]
    public async Task ClearCacheRemovesPasswordFromCache()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string userId = "12345";
        const string expectedCacheKey = "12345-psw";
        _cacheManagerMock.Setup(x => x.RemoveFromCache(expectedCacheKey, CacheType.InMemory))
            .Returns(Task.CompletedTask);

        // Act
        await authManager.ClearCache(userId);

        // Assert
        _cacheManagerMock.Verify(x => x.RemoveFromCache(expectedCacheKey, CacheType.InMemory), Times.Once);
    }

    [Fact]
    public async Task GetUserTokenThrowsWhenUserNotFound()
    {
        // Arrange
        var authManager = CreateAuthManager();
        const string email = "unknown@example.com";
        _usersManagerMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => authManager.GetUserToken(email));
        Assert.Equal("User not found", exception.Message);
    }
}
