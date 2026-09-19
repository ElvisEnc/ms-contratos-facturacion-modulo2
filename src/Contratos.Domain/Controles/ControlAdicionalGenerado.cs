using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Controles;

public sealed record ControlAdicionalGenerado(
    ControlId ControlId,
    PacienteId PacienteId,
    EvaluacionId EvaluacionId,
    Money Cobro) : DomainEvent;
