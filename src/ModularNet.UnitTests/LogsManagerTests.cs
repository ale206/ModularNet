using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class LogsManagerTests
{
    private readonly Mock<ICacheManager> _cacheManagerMock = new();
    private readonly Mock<ILogsRepository> _logsRepositoryMock = new();

    private LogsManager CreateLogsManager()
    {
        return new LogsManager(_logsRepositoryMock.Object, _cacheManagerMock.Object);
    }

    [Fact]
    public async Task WriteLogToDbSavesToCacheAndWritesToDbWhenNotCached()
    {
        // Arrange
        var logsManager = CreateLogsManager();
        var modularNetLog = new ModularNetLog
        {
            Id = 1L,
            LogMessage = "Unique error occurred",
            LogTimeStamp = DateTimeOffset.UtcNow,
            LogLevel = "Error",
            LogProperties = "{}"
        };
        _cacheManagerMock.Setup(x => x.GetFromCache<string>(modularNetLog.LogMessage, CacheType.InMemory))
            .ReturnsAsync((string?)null);
        _logsRepositoryMock.Setup(x => x.WriteLogToDb(modularNetLog)).Returns(Task.CompletedTask);

        // Act
        await logsManager.WriteLogToDb(modularNetLog);

        // Assert
        _cacheManagerMock.Verify(
            x => x.SaveInCache(modularNetLog.LogMessage, string.Empty, CacheType.InMemory, 300),
            Times.Once);
        _logsRepositoryMock.Verify(x => x.WriteLogToDb(modularNetLog), Times.Once);
    }

    [Fact]
    public async Task WriteLogToDbSkipsDbWriteWhenLogIsAlreadyCached()
    {
        // Arrange
        var logsManager = CreateLogsManager();
        var modularNetLog = new ModularNetLog
        {
            Id = 2L,
            LogMessage = "Duplicate error message",
            LogTimeStamp = DateTimeOffset.UtcNow,
            LogLevel = "Error",
            LogProperties = "{}"
        };
        _cacheManagerMock.Setup(x => x.GetFromCache<string>(modularNetLog.LogMessage, CacheType.InMemory))
            .ReturnsAsync("cached");

        // Act
        await logsManager.WriteLogToDb(modularNetLog);

        // Assert
        _logsRepositoryMock.Verify(x => x.WriteLogToDb(It.IsAny<ModularNetLog>()), Times.Never);
        _cacheManagerMock.Verify(
            x => x.SaveInCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CacheType>(), It.IsAny<int>()),
            Times.Never);
    }
}
