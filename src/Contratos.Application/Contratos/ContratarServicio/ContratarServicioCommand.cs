using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Contratos.ContratarServicio;

// Exactamente estos tres campos (RN-01b): nunca DuracionDias ni FechaFin.
// Para CATERING, FechaInicio se ignora en silencio (C4): la vigencia sale
// siempre de la réplica PacienteConocido.
public sealed record ContratarServicioCommand(
    Guid PacienteId,
    TipoServicio TipoServicio,
    DateOnly? FechaInicio) : IRequest<ContratarServicioResult>;

public sealed record ContratarServicioResult(Guid ContratoId, Guid FacturaId);
