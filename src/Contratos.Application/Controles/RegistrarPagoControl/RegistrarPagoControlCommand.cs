using MediatR;

namespace Contratos.Application.Controles.RegistrarPagoControl;

// Sin monto: el importe de un control es el total de su factura, ya
// calculado; pedirlo abriría la puerta a un descuadre.
public sealed record RegistrarPagoControlCommand(Guid ControlId) : IRequest;
