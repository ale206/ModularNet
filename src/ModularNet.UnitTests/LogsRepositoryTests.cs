using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class LogsRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<LogsRepository>> _loggerMock = new();

    [Fact]
    public async Task WriteLogToDbExecutesSqlAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        const string connectionString = "Server=localhost;Database=test;";
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnectionString()).ReturnsAsync(connectionString);
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnection(connectionString)).Returns(fakeDbConnection);

        var logsRepository = new LogsRepository(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        var modularNetLog = new ModularNetLog
        {
            Id = 100L,
            LogTimeStamp = DateTimeOffset.UtcNow,
            LogLevel = "Information",
            LogMessage = "Application startup",
            LogProperties = "{}"
        };

        // Act
        await logsRepository.WriteLogToDb(modularNetLog);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnectionString(), Times.Once);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(connectionString), Times.Once);
    }
}
