using ModularNet.Business.Implementations;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Enums;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class AuditsManagerTests
{
    private readonly Mock<IAuditsRepository> _auditsRepositoryMock = new();

    [Fact]
    public async Task WriteAuditToDbDelegatesToAuditsRepository()
    {
        // Arrange
        var auditsManager = new AuditsManager(_auditsRepositoryMock.Object);
        var modularNetAudit = new ModularNetAudit
        {
            Id = 1L,
            UserId = Guid.NewGuid(),
            AuditType = AuditType.Login,
            AuditTimeStamp = DateTimeOffset.UtcNow,
            Meta = "login audit"
        };
        _auditsRepositoryMock.Setup(x => x.WriteAuditToDb(modularNetAudit)).Returns(Task.CompletedTask);

        // Act
        await auditsManager.WriteAuditToDb(modularNetAudit);

        // Assert
        _auditsRepositoryMock.Verify(x => x.WriteAuditToDb(modularNetAudit), Times.Once);
    }
}
