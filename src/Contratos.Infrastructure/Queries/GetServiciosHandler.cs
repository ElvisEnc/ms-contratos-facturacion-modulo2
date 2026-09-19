using Contratos.Application.Queries;
using Contratos.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Queries;

public sealed class GetServiciosHandler(ContratosDbContext dbContext)
    : IRequestHandler<GetServiciosQuery, IReadOnlyCollection<ServicioDto>>
{
    public async Task<IReadOnlyCollection<ServicioDto>> Handle(GetServiciosQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ServiciosCatalogo.AsNoTracking();

        if (request.SoloActivos == true)
            query = query.Where(s => s.Activo);

        // §12.5 del maestro: ".Value" sobre un id tipado con HasConversion no se
        // traduce dentro de la expresión — se proyecta el id tipado completo y
        // se extrae el Guid en memoria, después de materializar.
        var proyectados = await query
            .Select(s => new
            {
                s.ServicioId,
                s.Tipo,
                s.Nombre,
                Monto = s.PrecioVigente.Monto,
                Moneda = s.PrecioVigente.Moneda,
                s.Activo
            })
            .ToListAsync(cancellationToken);

        return proyectados
            .Select(s => new ServicioDto(s.ServicioId.Value, s.Tipo, s.Nombre, s.Monto, s.Moneda, s.Activo))
            .ToList();
    }
}
