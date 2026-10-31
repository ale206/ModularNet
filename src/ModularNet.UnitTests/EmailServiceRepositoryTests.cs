using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class EmailServiceRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock = new();
    private readonly Mock<ILogger<EmailServiceRepository>> _loggerMock = new();
    private const string ConnectionString = "Server=localhost;Database=test;";

    private EmailServiceRepository CreateEmailServiceRepository(FakeDbConnection fakeDbConnection)
    {
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnectionString()).ReturnsAsync(ConnectionString);
        _dbConnectionFactoryMock.Setup(x => x.GetDbConnection(ConnectionString)).Returns(fakeDbConnection);
        return new EmailServiceRepository(_dbConnectionFactoryMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveEmailExecutesInsertAgainstConnection(bool saveHtml)
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var emailServiceRepository = CreateEmailServiceRepository(fakeDbConnection);
        var modularNetEmail = new ModularNetEmail
        {
            Id = Guid.NewGuid(),
            Subject = "Test Subject",
            PlainText = "Test plain text",
            Html = "<p>Test</p>",
            SenderEmail = "sender@example.com",
            EmailRecipientsJson = "[\"recipient@example.com\"]",
            ImportanceValue = "Normal"
        };

        // Act
        await emailServiceRepository.SaveEmail(modularNetEmail, saveHtml);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnectionString(), Times.Once);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(ConnectionString), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailWithMessageIdExecutesUpdateAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var emailServiceRepository = CreateEmailServiceRepository(fakeDbConnection);
        var emailId = Guid.NewGuid();
        const string messageId = "azure-message-id-123";

        // Act
        await emailServiceRepository.UpdateEmailWithMessageId(emailId, messageId);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnectionString(), Times.Once);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(ConnectionString), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailStatusExecutesUpdateAgainstConnection()
    {
        // Arrange
        var fakeDbConnection = new FakeDbConnection();
        var emailServiceRepository = CreateEmailServiceRepository(fakeDbConnection);
        var emailId = Guid.NewGuid();
        const string sendStatusJson = "{\"status\":\"Delivered\"}";

        // Act
        await emailServiceRepository.UpdateEmailStatus(emailId, sendStatusJson);

        // Assert
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnectionString(), Times.Once);
        _dbConnectionFactoryMock.Verify(x => x.GetDbConnection(ConnectionString), Times.Once);
    }
}
