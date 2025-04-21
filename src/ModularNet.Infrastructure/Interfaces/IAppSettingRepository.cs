using ModularNet.Domain.Entities;

namespace ModularNet.Infrastructure.Interfaces;

public interface IAppSettingsRepository
{
    Task<AppSettings> GetAppSettings();
}