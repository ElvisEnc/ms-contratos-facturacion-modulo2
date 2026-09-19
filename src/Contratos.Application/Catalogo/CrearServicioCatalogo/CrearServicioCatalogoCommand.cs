using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Catalogo.CrearServicioCatalogo;

public sealed record CrearServicioCatalogoCommand(
    TipoServicio Tipo,
    string Nombre,
    decimal Precio,
    string Moneda) : IRequest<Guid>;
