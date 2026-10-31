using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class UsersSettingsRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<UsersSettingsRepository>> _loggerMock = new();

    private UsersSettingsRepository CreateUsersSettingsRepository(FakeDbConnection fakeDbConnection)
    {
        _dbConnectionFactoryMock.Setup(x => x.GetConnection()).ReturnsAsync(fakeDbConnection);
        _dbConnectionFactoryMock.SetupGet(x => x.CurrentDbTransaction).Returns((System.Data.Common.DbTransaction?)null);
        return new UsersSettingsRepository(_dbConnectionFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserSettingsExecutesQueryAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var usersSettingsRepository = CreateUsersSettingsRepository(fakeDbConnection);
        var userId = Guid.NewGuid();

        // Act
        var result = await usersSettingsRepository.GetUserSettings(userId);

        // Assert
        Assert.NotNull(result);
        _dbConnectionFactoryMock.Verify(x => x.GetConnection(), Times.Once);
    }

    [Fact]
    public async Task CreateUserSettingExecutesInsertAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var usersSettingsRepository = CreateUsersSettingsRepository(fakeDbConnection);
        var userSetting = UserSettingsHelper.GetUserSetting();

        // Act
        await usersSettingsRepository.CreateUserSetting(userSetting);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetConnection(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserSettingExecutesUpdateAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var usersSettingsRepository = CreateUsersSettingsRepository(fakeDbConnection);
        var userSetting = UserSettingsHelper.GetUserSetting();

        // Act
        await usersSettingsRepository.UpdateUserSetting(userSetting);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetConnection(), Times.Once);
    }
}
