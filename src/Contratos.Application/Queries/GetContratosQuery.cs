using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Queries;

public sealed record GetContratosQuery(DateOnly Desde, DateOnly Hasta, EstadoContrato? Estado)
    : IRequest<IReadOnlyCollection<ContratoResumenDto>>;

public sealed record ContratoResumenDto(
    Guid ContratoId,
    Guid PacienteId,
    TipoServicio TipoServicio,
    DateOnly FechaContratacion,
    DateOnly VigenciaFechaInicio,
    DateOnly VigenciaFechaFin,
    EstadoContrato Estado);
