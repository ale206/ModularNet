using System.Data.Common;

namespace ModularNet.Infrastructure.Interfaces;

public interface IDbConnectionFactory : IDisposable, IAsyncDisposable
{
    DbConnection GetDbConnection(string connectionString);
    Task<string> GetDbConnectionString();
    Task<DbConnection> GetConnection();
    DbTransaction? CurrentDbTransaction { get; }
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
}
