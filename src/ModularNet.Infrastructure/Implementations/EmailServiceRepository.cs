using Dapper;
using Microsoft.Extensions.Logging;
using ModularNet.Domain.Entities;
using ModularNet.Infrastructure.Interfaces;

namespace ModularNet.Infrastructure.Implementations;

public class EmailServiceRepository : IEmailServiceRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<EmailServiceRepository> _logger;

    public EmailServiceRepository(IDbConnectionFactory dbConnectionFactory, ILogger<EmailServiceRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;

        _logger.LogDebug($"{nameof(EmailServiceRepository)} constructed");
    }

    public async Task SaveEmail(ModularNetEmail modularNetEmail, bool saveHtml = true)
    {
        _logger.LogDebug($"Start repository method {nameof(SaveEmail)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"INSERT INTO `email` 
            (`id`, `subject`, `plain_text`, `html`, `sender_email`, `email_recipients`, `email_importance`, `is_enabled`, `created_on`)
            VALUES 
            (@id, @subject, @plain_text, @html, @sender_email, @email_recipients, @email_importance, @is_enabled, @created_on)
            ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = modularNetEmail.Id,
            subject = modularNetEmail.Subject,
            plain_text = modularNetEmail.PlainText,
            html = saveHtml ? modularNetEmail.Html : null,
            sender_email = modularNetEmail.SenderEmail,
            email_recipients = modularNetEmail.EmailRecipientsJson,
            email_importance = modularNetEmail.ImportanceValue,

            is_enabled = 1,
            created_on = DateTime.UtcNow
        });
    }

    public async Task UpdateEmailWithMessageId(Guid emailId, string messageId)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateEmailWithMessageId)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `email` 
                SET `message_id` = @message_id, modified_on = @modified_on
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = emailId,
            message_id = messageId,
            modified_on = DateTime.UtcNow
        });
    }

    public async Task UpdateEmailStatus(Guid emailId, string sendStatusJson)
    {
        _logger.LogDebug($"Start repository method {nameof(UpdateEmailStatus)}");

        var connectionString = await _dbConnectionFactory.GetDbConnectionString();

        const string sql =
            @"UPDATE `email` 
                SET `send_status` = @send_status, modified_on = @modified_on
                WHERE id = @id
                ";
        await using var connection = _dbConnectionFactory.GetDbConnection(connectionString);
        await connection.ExecuteAsync(sql, new
        {
            id = emailId,
            send_status = sendStatusJson,
            modified_on = DateTime.UtcNow
        });
    }
}