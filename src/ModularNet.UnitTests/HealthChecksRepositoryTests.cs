using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class HealthChecksRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<HealthChecksRepository>> _loggerMock = new();

    [Fact]
    public async Task CheckDbConnectionExecutesQueryAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        const string connectionString = "Server=localhost;Database=test;";
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnectionString()).ReturnsAsync(connectionString);
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnection(connectionString)).Returns(fakeDbConnection);

        var healthChecksRepository = new HealthChecksRepository(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act
        await healthChecksRepository.CheckDbConnection();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnectionString(), Times.Once);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(connectionString), Times.Once);
    }
}
