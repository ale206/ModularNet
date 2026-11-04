using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class UserNameManagerTests
{
    [Fact]
    public async Task GetUserFirstNameByIdReturnsCachedName()
    {
        var userId = Guid.NewGuid();
        var usersManagerMock = new Mock<IUsersManager>();
        var cacheManagerMock = new Mock<ICacheManager>();
        cacheManagerMock.Setup(manager => manager.GetFromCache<string>($"user:{userId}:first-name", CacheType.InMemory))
            .ReturnsAsync("Ada");
        var userNameManager = new UserNameManager(usersManagerMock.Object, cacheManagerMock.Object,
            Mock.Of<ILogger<UserNameManager>>());

        Assert.Equal("Ada", await userNameManager.GetUserFirstNameById(userId));
        usersManagerMock.Verify(manager => manager.GetUserById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetUserFirstNameByIdLoadsAndCachesNameOnMiss()
    {
        var userId = Guid.NewGuid();
        var user = UsersHelper.GetUser();
        user.FirstName = "Ada";
        var usersManagerMock = new Mock<IUsersManager>();
        usersManagerMock.Setup(manager => manager.GetUserById(userId)).ReturnsAsync(user);
        var cacheManagerMock = new Mock<ICacheManager>();
        var userNameManager = new UserNameManager(usersManagerMock.Object, cacheManagerMock.Object,
            Mock.Of<ILogger<UserNameManager>>());

        Assert.Equal("Ada", await userNameManager.GetUserFirstNameById(userId));
        cacheManagerMock.Verify(manager => manager.SaveInCache($"user:{userId}:first-name", "Ada", CacheType.InMemory, 300), Times.Once);
    }

    [Fact]
    public async Task GetUserFirstNameByIdReturnsNullWhenUserMissing()
    {
        var usersManagerMock = new Mock<IUsersManager>();
        var cacheManagerMock = new Mock<ICacheManager>();
        var userNameManager = new UserNameManager(usersManagerMock.Object, cacheManagerMock.Object,
            Mock.Of<ILogger<UserNameManager>>());

        Assert.Null(await userNameManager.GetUserFirstNameById(Guid.NewGuid()));
        cacheManagerMock.Verify(manager => manager.SaveInCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CacheType>(), It.IsAny<int>()), Times.Never);
    }
}
