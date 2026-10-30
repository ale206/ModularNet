using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class UnitOfWorkTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<UnitOfWork>> _loggerMock = new();
    private readonly TestDbConnection _testDbConnection = new();

    public UnitOfWorkTests()
    {
        _dbConnectionFactoryMock
            .Setup(x => x.GetDbConnectionString())
            .ReturnsAsync("Server=localhost;Database=test;Uid=root;Pwd=test;");
        _dbConnectionFactoryMock
            .Setup(x => x.GetDbConnection(It.IsAny<string>()))
            .Returns(_testDbConnection);
    }

    [Fact]
    public async Task GetConnectionOpensAndReturnsConnection()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act
        var connection = await unitOfWork.GetConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal(1, _testDbConnection.OpenAsyncCallCount);
    }

    [Fact]
    public async Task GetConnectionReusesExistingConnection()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act
        var firstConnection = await unitOfWork.GetConnection();
        var secondConnection = await unitOfWork.GetConnection();

        // Assert
        Assert.Same(firstConnection, secondConnection);
        Assert.Equal(1, _testDbConnection.OpenAsyncCallCount);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BeginTransactionStartsTransaction()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act
        await unitOfWork.BeginTransaction();

        // Assert
        Assert.NotNull(unitOfWork.Transaction);
        Assert.Same(_testDbConnection.CurrentTestTransaction, unitOfWork.Transaction);
    }

    [Fact]
    public async Task BeginTransactionThrowsWhenTransactionAlreadyActive()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        await unitOfWork.BeginTransaction();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.BeginTransaction());
    }

    [Fact]
    public async Task CommitCommitsAndClearsTransaction()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        await unitOfWork.BeginTransaction();
        var transaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await unitOfWork.Commit();

        // Assert
        Assert.Null(unitOfWork.Transaction);
        Assert.Equal(1, transaction.CommitCallCount);
        Assert.True(transaction.DisposeCallCount > 0);
    }

    [Fact]
    public async Task CommitThrowsWhenNoTransactionActive()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.Commit());
    }

    [Fact]
    public async Task RollbackRollsBackAndClearsTransaction()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        await unitOfWork.BeginTransaction();
        var transaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await unitOfWork.Rollback();

        // Assert
        Assert.Null(unitOfWork.Transaction);
        Assert.Equal(1, transaction.RollbackCallCount);
        Assert.True(transaction.DisposeCallCount > 0);
    }

    [Fact]
    public async Task RollbackDoesNothingWhenNoTransactionActive()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);

        // Act
        await unitOfWork.Rollback();

        // Assert
        Assert.Null(unitOfWork.Transaction);
    }

    [Fact]
    public async Task DisposeRollsBackUncommittedTransaction()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        await unitOfWork.BeginTransaction();
        var transaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        unitOfWork.Dispose();

        // Assert
        Assert.Equal(1, transaction.RollbackCallCount);
        Assert.True(_testDbConnection.DisposeCallCount > 0);
    }

    [Fact]
    public async Task DisposeAsyncRollsBackUncommittedTransaction()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        await unitOfWork.BeginTransaction();
        var transaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await unitOfWork.DisposeAsync();

        // Assert
        Assert.Equal(1, transaction.RollbackCallCount);
        Assert.True(_testDbConnection.DisposeCallCount > 0);
    }

    [Fact]
    public async Task MethodsThrowObjectDisposedExceptionWhenDisposed()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbConnectionFactoryMock.Object, _loggerMock.Object);
        unitOfWork.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.GetConnection());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.BeginTransaction());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.Commit());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.Rollback());
        Assert.Throws<ObjectDisposedException>(() => _ = unitOfWork.Transaction);
    }

    private class TestDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;

        public int OpenAsyncCallCount { get; private set; }
        public int DisposeCallCount { get; private set; }
        public TestDbTransaction? CurrentTestTransaction { get; private set; }

        public override ConnectionState State => _state;
        public override string Database => "test_db";
        public override string DataSource => "localhost";
        public override string ServerVersion => "8.0";
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { _state = ConnectionState.Closed; }
        public override void Open() { _state = ConnectionState.Open; }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            OpenAsyncCallCount++;
            _state = ConnectionState.Open;
            return Task.CompletedTask;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            CurrentTestTransaction = new TestDbTransaction(this);
            return CurrentTestTransaction;
        }

        protected override ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            CurrentTestTransaction = new TestDbTransaction(this);
            return ValueTask.FromResult<DbTransaction>(CurrentTestTransaction);
        }

        protected override DbCommand CreateDbCommand() => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            DisposeCallCount++;
            base.Dispose(disposing);
        }
    }

    private class TestDbTransaction : DbTransaction
    {
        private readonly DbConnection _connection;

        public int CommitCallCount { get; private set; }
        public int RollbackCallCount { get; private set; }
        public int DisposeCallCount { get; private set; }

        public TestDbTransaction(DbConnection connection)
        {
            _connection = connection;
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection DbConnection => _connection;

        public override void Commit() => CommitCallCount++;

        public override Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCallCount++;
            return Task.CompletedTask;
        }

        public override void Rollback() => RollbackCallCount++;

        public override Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            RollbackCallCount++;
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            DisposeCallCount++;
            base.Dispose(disposing);
        }
    }
}
