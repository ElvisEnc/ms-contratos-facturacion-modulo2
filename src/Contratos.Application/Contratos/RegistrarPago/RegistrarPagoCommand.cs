using MediatR;

namespace Contratos.Application.Contratos.RegistrarPago;

public sealed record RegistrarPagoCommand(
    Guid ContratoId,
    decimal Monto,
    string Moneda,
    string? Referencia) : IRequest;
