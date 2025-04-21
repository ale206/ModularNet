using System.Data.Common;

namespace ModularNet.Infrastructure.Interfaces;

public interface IDbConnectionFactory
{
    DbConnection GetDbConnection(string connectionString);
    Task<string> GetDbConnectionString();
}