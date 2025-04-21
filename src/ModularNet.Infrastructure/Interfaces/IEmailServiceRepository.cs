using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface IEmailServiceRepository
{
    Task SaveEmail(ModularNetEmail modularNetEmail, bool saveHtml = true);
    Task UpdateEmailWithMessageId(Guid emailId, string messageId);
    Task UpdateEmailStatus(Guid emailId, string sendStatusJson);
}