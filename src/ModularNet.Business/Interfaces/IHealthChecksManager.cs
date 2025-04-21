namespace ModularNet.Business.Interfaces;

public interface IHealthChecksManager
{
    Task CheckDbConnection();
    Task<string> GetLocalIp();
}