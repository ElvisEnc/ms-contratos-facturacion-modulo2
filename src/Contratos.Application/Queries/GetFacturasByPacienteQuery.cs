using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Queries;

// Devuelve las facturas de contratos y de controles juntas: para el paciente
// (HU-21) son todos sus gastos con la empresa. Origen y ReferenciaId las
// distinguen. Lista vacía y 200 si no hay ninguna, nunca 404.
public sealed record GetFacturasByPacienteQuery(Guid PacienteId) : IRequest<IReadOnlyCollection<FacturaDto>>;

public sealed record FacturaDto(
    Guid FacturaId,
    OrigenFactura Origen,
    Guid ReferenciaId,
    DateOnly Fecha,
    decimal Subtotal,
    decimal Impuesto,
    decimal Total,
    string Moneda,
    EstadoFactura Estado);
