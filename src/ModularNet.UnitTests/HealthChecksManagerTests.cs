using Microsoft.Extensions.Logging;
using ModularNet.Business.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class HealthChecksManagerTests
{
    private readonly Mock<IHealthChecksRepository> _healthChecksRepositoryMock = new();
    private readonly Mock<ILogger<HealthChecksManager>> _loggerMock = new();

    private HealthChecksManager CreateHealthChecksManager()
    {
        return new HealthChecksManager(_healthChecksRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CheckDbConnectionDelegatesToRepository()
    {
        // Arrange
        var healthChecksManager = CreateHealthChecksManager();
        _healthChecksRepositoryMock.Setup(x => x.CheckDbConnection()).Returns(Task.CompletedTask);

        // Act
        await healthChecksManager.CheckDbConnection();

        // Assert
        _healthChecksRepositoryMock.Verify(x => x.CheckDbConnection(), Times.Once);
    }

    [Fact]
    public async Task GetLocalIpReturnsIpAddressOrThrows()
    {
        // Arrange
        var healthChecksManager = CreateHealthChecksManager();

        // Act & Assert
        try
        {
            var ipAddress = await healthChecksManager.GetLocalIp();
            Assert.False(string.IsNullOrWhiteSpace(ipAddress));
        }
        catch (Exception ex)
        {
            Assert.Contains("No network adapters with an IPv4 address", ex.Message);
        }
    }
}
