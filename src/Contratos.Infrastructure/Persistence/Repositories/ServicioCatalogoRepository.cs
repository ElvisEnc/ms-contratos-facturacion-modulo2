using Contratos.Application.Ports;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence.Repositories;

public sealed class ServicioCatalogoRepository(ContratosDbContext dbContext) : IServicioCatalogoRepository
{
    public async Task<ServicioCatalogo?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var servicioId = ServicioCatalogoId.From(id);
        var query = dbContext.ServiciosCatalogo.AsQueryable();
        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(s => s.ServicioId == servicioId);
    }

    public async Task AddAsync(ServicioCatalogo entity) => await dbContext.ServiciosCatalogo.AddAsync(entity);

    // C9, y resolución del cobro de un control adicional (C7). D-13: filtra por
    // Activo — sin este filtro, con dos servicios del mismo tipo el precio
    // contratado depende del orden de inserción.
    public async Task<ServicioCatalogo?> GetByTipoAsync(TipoServicio tipo)
    {
        return await dbContext.ServiciosCatalogo
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Tipo == tipo && s.Activo);
    }
}
