namespace Contratos.Application.Ports;

// Única implementación hoy: LoggingIntegrationEventPublisher (Infrastructure),
// que serializa a JSON y escribe en el log (§2.3, §10 del maestro).
public interface IIntegrationEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent evento, CancellationToken ct = default) where TEvent : class;
}
