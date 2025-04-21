using ModularNet.Domain.Entities;

namespace ModularNet.Business.Interfaces;

public interface IWebAppConfigsManager
{
    Task<WebAppConfigs> GetWebAppConfigs();
}