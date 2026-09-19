using Contratos.Application.Errors;
using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Application.Ports;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.EventHandlers;

// Política de §8.2: traductora pura, no muta estado. Reacciona al evento de
// dominio ServicioContratado (Contrato.Contratar) y compone el evento de
// integración de SISTEMA.md §9.2.
public sealed class PublicarServicioContratadoHandler(
    IPacienteConocidoStore pacienteConocidoStore,
    IIntegrationEventPublisher integrationEventPublisher) : INotificationHandler<ServicioContratado>
{
    public async Task Handle(ServicioContratado notification, CancellationToken cancellationToken)
    {
        // C16: pacienteNombre y nroIdentificacion salen de la réplica local,
        // nunca de una consulta a BC1. Si falta, es un fallo interno.
        var replica = await pacienteConocidoStore.GetByIdAsync(notification.PacienteId)
            ?? throw new DomainException(ContratosErrors.PacienteSinReplica());

        var evento = new ServicioContratadoIntegrationEvent(
            notification.ContratoId.Value,
            notification.PacienteId.Value,
            replica.Nombre,
            replica.NroIdentificacion,
            notification.TipoServicio,
            notification.TipoServicio == TipoServicio.CATERING ? notification.DuracionDias : null,
            notification.FechaInicio);

        await integrationEventPublisher.PublishAsync(evento, cancellationToken);
    }
}
