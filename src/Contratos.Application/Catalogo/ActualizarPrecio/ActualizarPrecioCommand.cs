using MediatR;

namespace Contratos.Application.Catalogo.ActualizarPrecio;

public sealed record ActualizarPrecioCommand(
    Guid ServicioId,
    decimal Precio,
    string Moneda) : IRequest;
