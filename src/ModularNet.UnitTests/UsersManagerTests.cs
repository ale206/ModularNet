using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class UsersManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();
    private readonly Mock<IAuditsManager> _auditsManagerMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();
    private readonly Mock<IEmailServiceManager> _emailServiceManagerMock = new();
    private readonly Mock<IEncryptManager> _encryptManagerMock = new();
    private readonly Mock<ILogger<UsersManager>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();

    private UsersManager CreateUsersManager()
    {
        return new UsersManager(
            _usersRepositoryMock.Object,
            _loggerMock.Object,
            _encryptManagerMock.Object,
            _emailServiceManagerMock.Object,
            _auditsManagerMock.Object,
            _authenticationServiceMock.Object,
            _appSettingsManagerMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task RegisterUserTest()
    {
        // Arrange
        _encryptManagerMock.Setup(x => x.GenerateInitializationVector()).ReturnsAsync("initialization-vector");
        var usersManager = CreateUsersManager();
        _usersRepositoryMock.Setup(x => x.RegisterUser(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        await usersManager.RegisterUserInModularNet(new RegisterUserRequest());

        // Assert
        _usersRepositoryMock.Verify(x => x.RegisterUser(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginTests()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        _usersRepositoryMock.Setup(x => x.UpdateUserLoginDate(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        // Act
        await usersManager.LoginUser(new UserSignInRequest());

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        _auditsManagerMock.Verify(x => x.WriteAuditToDb(It.IsAny<ModularNetAudit>()), Times.Once);
        _usersRepositoryMock.Verify(x => x.UpdateUserLoginDate(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
    }

    [Fact]
    public async Task LoginUserRollsBackTransactionWhenExceptionThrown()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        _usersRepositoryMock.Setup(x => x.UpdateUserLoginDate(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => usersManager.LoginUser(new UserSignInRequest()));
        _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task GetUserByEmailTest()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var mockedUser = UsersHelper.GetUser();
        _usersRepositoryMock.Setup(x => x.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(mockedUser);

        // Act
        var user = await usersManager.GetUserByEmail("email@email.com");

        // Assert
        _usersRepositoryMock.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Once);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetUserByIdTest()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var mockedUser = UsersHelper.GetUser();
        _usersRepositoryMock.Setup(x => x.GetUserById(It.IsAny<Guid>())).ReturnsAsync(mockedUser);

        // Act
        var user = await usersManager.GetUserById(Guid.NewGuid());

        // Assert
        _usersRepositoryMock.Verify(x => x.GetUserById(It.IsAny<Guid>()), Times.Once);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task SetUserEmailAsVerifiedCallsRepository()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var userId = Guid.NewGuid();
        _usersRepositoryMock.Setup(x => x.SetUserEmailAsVerified(userId)).Returns(Task.CompletedTask);

        // Act
        await usersManager.SetUserEmailAsVerified(userId);

        // Assert
        _usersRepositoryMock.Verify(x => x.SetUserEmailAsVerified(userId), Times.Once);
    }

    [Fact]
    public async Task SetTermsAndConditionsAsAcceptedCallsRepository()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var userId = Guid.NewGuid();
        _usersRepositoryMock.Setup(x => x.SetTermsAndConditionsAsAccepted(userId)).Returns(Task.CompletedTask);

        // Act
        await usersManager.SetTermsAndConditionsAsAccepted(userId);

        // Assert
        _usersRepositoryMock.Verify(x => x.SetTermsAndConditionsAsAccepted(userId), Times.Once);
    }

    [Fact]
    public async Task DeactivateAccountCallsRepository()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var userId = Guid.NewGuid();
        _usersRepositoryMock.Setup(x => x.DeactivateAccount(userId)).Returns(Task.CompletedTask);

        // Act
        await usersManager.DeactivateAccount(userId);

        // Assert
        _usersRepositoryMock.Verify(x => x.DeactivateAccount(userId), Times.Once);
    }

    [Fact]
    public async Task EnableUserCallsRepository()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var userId = Guid.NewGuid();
        _usersRepositoryMock.Setup(x => x.EnableUser(userId)).Returns(Task.CompletedTask);

        // Act
        await usersManager.EnableUser(userId);

        // Assert
        _usersRepositoryMock.Verify(x => x.EnableUser(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateUserOidCallsRepository()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var userId = Guid.NewGuid();
        const string userOid = "firebase-oid-123";
        _usersRepositoryMock.Setup(x => x.UpdateUserOid(userId, userOid)).Returns(Task.CompletedTask);

        // Act
        await usersManager.UpdateUserOid(userId, userOid);

        // Assert
        _usersRepositoryMock.Verify(x => x.UpdateUserOid(userId, userOid), Times.Once);
    }

    [Fact]
    public async Task GetUserIdByEmailCallsRepositoryAndReturnsGuid()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        var expectedUserId = Guid.NewGuid();
        const string email = "test@example.com";
        _usersRepositoryMock.Setup(x => x.GetUserIdByEmail(email)).ReturnsAsync(expectedUserId);

        // Act
        var userId = await usersManager.GetUserIdByEmail(email);

        // Assert
        Assert.Equal(expectedUserId, userId);
        _usersRepositoryMock.Verify(x => x.GetUserIdByEmail(email), Times.Once);
    }

    [Fact]
    public async Task RegisterUserInFirebaseReturnsUidWhenSuccessful()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        const string expectedUid = "firebase-uid-abc";
        _authenticationServiceMock.Setup(x => x.RegisterAsync(email, password)).ReturnsAsync(expectedUid);

        // Act
        var uid = await usersManager.RegisterUserInFirebase(email, password);

        // Assert
        Assert.Equal(expectedUid, uid);
        _authenticationServiceMock.Verify(x => x.RegisterAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task RegisterUserInFirebaseLogsAndThrowsWhenExceptionOccurs()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        _authenticationServiceMock.Setup(x => x.RegisterAsync(email, password))
            .ThrowsAsync(new InvalidOperationException("Firebase error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            usersManager.RegisterUserInFirebase(email, password));
    }

    [Fact]
    public async Task LoginUserInFirebaseReturnsJwtTokenWhenSuccessful()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        const string expectedToken = "jwt-token-xyz";
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        _authenticationServiceMock
            .Setup(x => x.GetJwtTokenFromFirebaseAsync(email, password, appSettings.WebAppConfigs.FirebaseApiKey))
            .ReturnsAsync(expectedToken);

        // Act
        var token = await usersManager.LoginUserInFirebase(email, password);

        // Assert
        Assert.Equal(expectedToken, token);
        _authenticationServiceMock.Verify(
            x => x.GetJwtTokenFromFirebaseAsync(email, password, appSettings.WebAppConfigs.FirebaseApiKey),
            Times.Once);
    }

    [Fact]
    public async Task LoginUserInFirebaseLogsAndThrowsWhenExceptionOccurs()
    {
        // Arrange
        var usersManager = CreateUsersManager();
        const string email = "user@example.com";
        const string password = "Password123!";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings())
            .ThrowsAsync(new InvalidOperationException("Settings failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            usersManager.LoginUserInFirebase(email, password));
    }
}
