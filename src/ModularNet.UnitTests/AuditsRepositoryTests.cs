using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class AuditsRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<AuditsRepository>> _loggerMock = new();

    [Fact]
    public async Task WriteAuditToDbExecutesSqlAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        _dbConnectionFactoryMock.Setup(x => x.GetConnection()).ReturnsAsync(fakeDbConnection);
        _dbConnectionFactoryMock.SetupGet(x => x.CurrentDbTransaction).Returns((System.Data.Common.DbTransaction?)null);

        var auditsRepository = new AuditsRepository(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        var modularNetAudit = new ModularNetAudit
        {
            Id = 1L,
            UserId = Guid.NewGuid(),
            AuditType = AuditType.Login,
            AuditTimeStamp = DateTimeOffset.UtcNow,
            Meta = "Audit test metadata"
        };

        // Act
        await auditsRepository.WriteAuditToDb(modularNetAudit);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetConnection(), Times.Once);
    }
}
