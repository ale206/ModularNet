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
}
