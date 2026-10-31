using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Domain.Requests;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class UsersSettingsManagerTests
{
    private readonly Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUsersSettingsRepository> _usersSettingsRepositoryMock = new();

    private UsersSettingsManager CreateUsersSettingsManager()
    {
        return new UsersSettingsManager(
            _usersSettingsRepositoryMock.Object,
            _cacheManagerMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdReturnsFromCacheWhenPresent()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var userId = Guid.NewGuid();
        var cachedSettings = new List<UserSetting> { UserSettingsHelper.GetUserSetting() };
        _cacheManagerMock
            .Setup(x => x.GetFromCache<List<UserSetting>>($"{userId}-settings", CacheType.InMemory))
            .ReturnsAsync(cachedSettings);

        // Act
        var userSettings = await usersSettingsManager.GetUserSettingsByUserId(userId);

        // Assert
        Assert.Equal(cachedSettings, userSettings);
        _usersSettingsRepositoryMock.Verify(x => x.GetUserSettings(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdFetchesFromRepositoryAndCachesWhenCacheEmpty()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var userId = Guid.NewGuid();
        var settingDto = UserSettingsHelper.GetUserSettingDto();
        settingDto.UserId = userId;
        _cacheManagerMock
            .Setup(x => x.GetFromCache<List<UserSetting>>($"{userId}-settings", CacheType.InMemory))
            .ReturnsAsync((List<UserSetting>?)null);
        _usersSettingsRepositoryMock
            .Setup(x => x.GetUserSettings(userId))
            .ReturnsAsync(new List<UserSettingDto> { settingDto });

        // Act
        var userSettings = await usersSettingsManager.GetUserSettingsByUserId(userId);

        // Assert
        Assert.Single(userSettings);
        Assert.Equal(settingDto.SettingName, userSettings[0].SettingName);
        _cacheManagerMock.Verify(
            x => x.SaveInCache($"{userId}-settings", It.IsAny<List<UserSetting>>(), CacheType.InMemory, 2592000),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateUserSettingsCommitsTransactionOnSuccess()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var request = UserSettingsHelper.GetCreateOrUpdateUserSettingsRequest();
        var userId = request.UserSettings[0].UserId;

        _usersSettingsRepositoryMock
            .Setup(x => x.GetUserSettings(userId))
            .ReturnsAsync(new List<UserSettingDto>());
        _usersSettingsRepositoryMock
            .Setup(x => x.CreateUserSetting(It.IsAny<UserSetting>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await usersSettingsManager.CreateOrUpdateUserSettings(request);

        // Assert
        Assert.Single(result);
        _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        _usersSettingsRepositoryMock.Verify(x => x.CreateUserSetting(It.IsAny<UserSetting>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateUserSettingsRollsBackTransactionWhenExceptionOccurs()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var request = UserSettingsHelper.GetCreateOrUpdateUserSettingsRequest();
        var userId = request.UserSettings[0].UserId;

        _usersSettingsRepositoryMock
            .Setup(x => x.GetUserSettings(userId))
            .ReturnsAsync(new List<UserSettingDto>());
        _usersSettingsRepositoryMock
            .Setup(x => x.CreateUserSetting(It.IsAny<UserSetting>()))
            .ThrowsAsync(new InvalidOperationException("Insert failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            usersSettingsManager.CreateOrUpdateUserSettings(request));

        _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateUserSettingsThrowsWhenEmpty()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var request = new CreateOrUpdateUserSettingsRequest { UserSettings = new List<UserSetting>() };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => usersSettingsManager.CreateOrUpdateUserSettings(request));
        _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateUserSettingsUpdatesExistingSettingWhenAlreadyPresent()
    {
        // Arrange
        var usersSettingsManager = CreateUsersSettingsManager();
        var request = UserSettingsHelper.GetCreateOrUpdateUserSettingsRequest();
        var userId = request.UserSettings[0].UserId;
        var existingDto = new UserSettingDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SettingName = request.UserSettings[0].SettingName,
            SettingValue = "OldValue",
            IsEnabled = false
        };

        _usersSettingsRepositoryMock
            .Setup(x => x.GetUserSettings(userId))
            .ReturnsAsync(new List<UserSettingDto> { existingDto });
        _usersSettingsRepositoryMock
            .Setup(x => x.UpdateUserSetting(It.IsAny<UserSetting>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await usersSettingsManager.CreateOrUpdateUserSettings(request);

        // Assert
        Assert.Single(result);
        Assert.Equal(existingDto.Id, result[0].Id);
        Assert.Equal(request.UserSettings[0].SettingValue, result[0].SettingValue);
        _usersSettingsRepositoryMock.Verify(x => x.UpdateUserSetting(It.IsAny<UserSetting>()), Times.Once);
        _usersSettingsRepositoryMock.Verify(x => x.CreateUserSetting(It.IsAny<UserSetting>()), Times.Never);
    }
}
