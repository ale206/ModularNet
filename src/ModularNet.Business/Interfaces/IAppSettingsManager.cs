using ModularNet.Domain.Entities;

namespace ModularNet.Business.Interfaces;

public interface IAppSettingsManager
{
    Task<AppSettings> GetAppSettings();
}