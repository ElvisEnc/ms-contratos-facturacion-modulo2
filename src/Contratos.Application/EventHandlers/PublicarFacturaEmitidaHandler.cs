using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Application.Ports;
using Contratos.Domain.Facturas;
using MediatR;

namespace Contratos.Application.EventHandlers;

// Política de §8.2: traductora pura. Aplana Total (Money) en monto + moneda
// — publica el TOTAL, nunca el subtotal (SISTEMA.md §9.3).
public sealed class PublicarFacturaEmitidaHandler(
    IIntegrationEventPublisher integrationEventPublisher) : INotificationHandler<FacturaEmitida>
{
    public Task Handle(FacturaEmitida notification, CancellationToken cancellationToken)
    {
        var evento = new FacturaEmitidaIntegrationEvent(
            notification.FacturaId.Value,
            notification.PacienteId.Value,
            notification.Origen,
            notification.ReferenciaId,
            notification.Total.Monto,
            notification.Total.Moneda,
            notification.Fecha);

        return integrationEventPublisher.PublishAsync(evento, cancellationToken);
    }
}
