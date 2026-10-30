using System.Data;

namespace ModularNet.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<IDbConnection> GetConnection();
    IDbTransaction? Transaction { get; }
    Task BeginTransaction();
    Task Commit();
    Task Rollback();
}
