using Contratos.Domain.Shared;

namespace Contratos.Application.IntegrationEvents.Salientes;

// BC3 -> consumidor externo (SISTEMA.md §9.3). `monto` es el Total, nunca el
// subtotal; `moneda` viaja como string, excepción documentada al resto de
// enums del contrato.
public sealed record FacturaEmitidaIntegrationEvent(
    Guid FacturaId,
    Guid PacienteId,
    OrigenFactura Origen,
    Guid ReferenciaId,
    decimal Monto,
    string Moneda,
    DateOnly Fecha);
