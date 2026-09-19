using Joseco.DDD.Core.Abstractions;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Commits { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commits++;
        return Task.CompletedTask;
    }
}
