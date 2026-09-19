using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Facturas;

public sealed record FacturaEmitida(
    FacturaId FacturaId,
    PacienteId PacienteId,
    OrigenFactura Origen,
    Guid ReferenciaId,
    Money Total,
    DateOnly Fecha) : DomainEvent;
