using Contratos.Application.Ports;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly List<object> _publicados = [];

    public IReadOnlyList<object> Publicados => _publicados;

    public Task PublishAsync<TEvent>(TEvent evento, CancellationToken ct = default) where TEvent : class
    {
        _publicados.Add(evento);
        return Task.CompletedTask;
    }
}
