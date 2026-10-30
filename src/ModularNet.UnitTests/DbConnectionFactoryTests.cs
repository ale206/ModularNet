using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class DbConnectionFactoryTests
{
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IHostingEnvironment> _hostingEnvironmentMock = new();
    private readonly Mock<ILogger<DbConnectionFactory>> _loggerMock = new();
    private readonly Mock<ISecretsRepository> _secretsRepositoryMock = new();
    private readonly TestDbConnection _testDbConnection = new();

    public DbConnectionFactoryTests()
    {
        _hostingEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns("Development");

        var connectionStringsSectionMock = new Mock<IConfigurationSection>();
        connectionStringsSectionMock.Setup(x => x["ModularNetConnectionString"])
            .Returns("Server=localhost;Database=test;Uid=root;Pwd=test;");
        _configurationMock
            .Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(connectionStringsSectionMock.Object);
    }

    private TestableDbConnectionFactory CreateDbConnectionFactory()
    {
        return new TestableDbConnectionFactory(
            _configurationMock.Object,
            _hostingEnvironmentMock.Object,
            _secretsRepositoryMock.Object,
            _loggerMock.Object,
            _testDbConnection);
    }

    [Fact]
    public async Task GetConnectionOpensAndReturnsConnection()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();

        // Act
        var dbConnection = await dbConnectionFactory.GetConnection();

        // Assert
        Assert.NotNull(dbConnection);
        Assert.Equal(ConnectionState.Open, dbConnection.State);
        Assert.Equal(1, _testDbConnection.OpenAsyncCallCount);
    }

    [Fact]
    public async Task GetConnectionReusesExistingConnection()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();

        // Act
        var firstDbConnection = await dbConnectionFactory.GetConnection();
        var secondDbConnection = await dbConnectionFactory.GetConnection();

        // Assert
        Assert.Same(firstDbConnection, secondDbConnection);
        Assert.Equal(1, _testDbConnection.OpenAsyncCallCount);
    }

    [Fact]
    public async Task BeginTransactionStartsTransaction()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();

        // Act
        await dbConnectionFactory.BeginTransaction();

        // Assert
        Assert.NotNull(dbConnectionFactory.CurrentDbTransaction);
        Assert.Same(_testDbConnection.CurrentTestTransaction, dbConnectionFactory.CurrentDbTransaction);
    }

    [Fact]
    public async Task BeginTransactionThrowsWhenTransactionAlreadyActive()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        await dbConnectionFactory.BeginTransaction();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dbConnectionFactory.BeginTransaction());
    }

    [Fact]
    public async Task CommitTransactionCommitsAndClearsTransaction()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        await dbConnectionFactory.BeginTransaction();
        var dbTransaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await dbConnectionFactory.CommitTransaction();

        // Assert
        Assert.Null(dbConnectionFactory.CurrentDbTransaction);
        Assert.Equal(1, dbTransaction.CommitCallCount);
        Assert.True(dbTransaction.DisposeCallCount > 0);
    }

    [Fact]
    public async Task CommitTransactionThrowsWhenNoTransactionActive()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dbConnectionFactory.CommitTransaction());
    }

    [Fact]
    public async Task RollbackTransactionRollsBackAndClearsTransaction()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        await dbConnectionFactory.BeginTransaction();
        var dbTransaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await dbConnectionFactory.RollbackTransaction();

        // Assert
        Assert.Null(dbConnectionFactory.CurrentDbTransaction);
        Assert.Equal(1, dbTransaction.RollbackCallCount);
        Assert.True(dbTransaction.DisposeCallCount > 0);
    }

    [Fact]
    public async Task RollbackTransactionDoesNothingWhenNoTransactionActive()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();

        // Act
        await dbConnectionFactory.RollbackTransaction();

        // Assert
        Assert.Null(dbConnectionFactory.CurrentDbTransaction);
    }

    [Fact]
    public async Task DisposeRollsBackUncommittedTransaction()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        await dbConnectionFactory.BeginTransaction();
        var dbTransaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        dbConnectionFactory.Dispose();

        // Assert
        Assert.Equal(1, dbTransaction.RollbackCallCount);
        Assert.True(_testDbConnection.DisposeCallCount > 0);
    }

    [Fact]
    public async Task DisposeAsyncRollsBackUncommittedTransaction()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        await dbConnectionFactory.BeginTransaction();
        var dbTransaction = _testDbConnection.CurrentTestTransaction!;

        // Act
        await dbConnectionFactory.DisposeAsync();

        // Assert
        Assert.Equal(1, dbTransaction.RollbackCallCount);
        Assert.True(_testDbConnection.DisposeCallCount > 0);
    }

    [Fact]
    public async Task MethodsThrowObjectDisposedExceptionWhenDisposed()
    {
        // Arrange
        var dbConnectionFactory = CreateDbConnectionFactory();
        dbConnectionFactory.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => dbConnectionFactory.GetConnection());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => dbConnectionFactory.BeginTransaction());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => dbConnectionFactory.CommitTransaction());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => dbConnectionFactory.RollbackTransaction());
        Assert.Throws<ObjectDisposedException>(() => _ = dbConnectionFactory.CurrentDbTransaction);
    }

    private class TestableDbConnectionFactory : DbConnectionFactory
    {
        private readonly DbConnection _dbConnection;

        public TestableDbConnectionFactory(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ISecretsRepository secretsRepository,
            ILogger<DbConnectionFactory> logger,
            DbConnection dbConnection)
            : base(configuration, hostingEnvironment, secretsRepository, logger)
        {
            _dbConnection = dbConnection;
        }

        public override DbConnection GetDbConnection(string connectionString) => _dbConnection;
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

        [AllowNull]
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
        private readonly DbConnection _dbConnection;

        public int CommitCallCount { get; private set; }
        public int RollbackCallCount { get; private set; }
        public int DisposeCallCount { get; private set; }

        public TestDbTransaction(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection DbConnection => _dbConnection;

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
