using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class UnitOfWorkTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<UnitOfWork>> _loggerMock = new();

    private UnitOfWork CreateUnitOfWork()
    {
        return new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task BeginTransactionDelegatesToDbConnectionFactory()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        _dbConnectionFactoryMock.Setup(x => x.BeginTransaction()).Returns(Task.CompletedTask);

        // Act
        await unitOfWork.BeginTransaction();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.BeginTransaction(), Times.Once);
    }

    [Fact]
    public async Task CommitDelegatesToDbConnectionFactory()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        _dbConnectionFactoryMock.Setup(x => x.CommitTransaction()).Returns(Task.CompletedTask);

        // Act
        await unitOfWork.Commit();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.CommitTransaction(), Times.Once);
    }

    [Fact]
    public async Task RollbackDelegatesToDbConnectionFactory()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        _dbConnectionFactoryMock.Setup(x => x.RollbackTransaction()).Returns(Task.CompletedTask);

        // Act
        await unitOfWork.Rollback();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.RollbackTransaction(), Times.Once);
    }

    [Fact]
    public void DisposeDelegatesToDbConnectionFactory()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();

        // Act
        unitOfWork.Dispose();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsyncDelegatesToDbConnectionFactory()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();

        // Act
        await unitOfWork.DisposeAsync();

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
