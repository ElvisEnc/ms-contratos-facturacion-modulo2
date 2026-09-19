using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Contratos;

public sealed record ServicioContratado(
    ContratoId ContratoId,
    PacienteId PacienteId,
    TipoServicio TipoServicio,
    int DuracionDias,
    DateOnly FechaInicio) : DomainEvent;
