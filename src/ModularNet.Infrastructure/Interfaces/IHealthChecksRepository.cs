namespace ModularNet.Infrastructure.Interfaces;

public interface IHealthChecksRepository
{
    Task CheckDbConnection();
}