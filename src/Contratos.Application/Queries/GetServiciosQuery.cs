using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Queries;

// El record vive en Application, el handler en Infrastructure/Queries/
// (§12.8 del maestro): necesita LINQ y EF Core directos.
public sealed record GetServiciosQuery(bool? SoloActivos) : IRequest<IReadOnlyCollection<ServicioDto>>;

public sealed record ServicioDto(Guid ServicioId, TipoServicio Tipo, string Nombre, decimal Precio, string Moneda, bool Activo);
