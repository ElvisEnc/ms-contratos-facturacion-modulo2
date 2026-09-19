using System.Text.Json;
using System.Text.Json.Serialization;
using Contratos.Application.Ports;
using Microsoft.Extensions.Logging;

namespace Contratos.Infrastructure.Messaging;

// Única implementación hoy (§2.3, §10 del maestro): no hay bus real, se
// serializa a JSON y se escribe en el log. PropertyNamingPolicy.CamelCase +
// JsonStringEnumConverter porque el log documenta el payload real de
// SISTEMA.md §9, que viaja en camelCase con enums como string en mayúsculas.
public sealed class LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    // Los enums del dominio ya están declarados en mayúsculas (CATERING,
    // ACTIVO, CONTRATACION...); el converter por defecto los serializa tal
    // cual, sin aplicarles la política de nombres de las propiedades.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task PublishAsync<TEvent>(TEvent evento, CancellationToken ct = default) where TEvent : class
    {
        var payload = JsonSerializer.Serialize(evento, JsonOptions);
        logger.LogInformation("Evento de integración publicado {TipoEvento}: {Payload}", typeof(TEvent).Name, payload);
        return Task.CompletedTask;
    }
}
