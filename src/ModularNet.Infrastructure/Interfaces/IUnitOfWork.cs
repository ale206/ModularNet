namespace ModularNet.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task BeginTransaction();
    Task Commit();
    Task Rollback();
}
