using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Contratos;

public sealed record ContratoCompletado(
    ContratoId ContratoId,
    PacienteId PacienteId,
    TipoServicio TipoServicio,
    DateOnly FechaFin) : DomainEvent;
