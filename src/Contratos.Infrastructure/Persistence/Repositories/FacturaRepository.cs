using Contratos.Application.Ports;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence.Repositories;

public sealed class FacturaRepository(ContratosDbContext dbContext) : IFacturaRepository
{
    // Una factura sin sus líneas no es una factura: C11 no se puede verificar
    // sobre ella (§14.5).
    public async Task<Factura?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var facturaId = FacturaId.From(id);
        // La navegación es el campo privado "_lineas" (§12.2 del maestro): la
        // propiedad pública Lineas está Ignore()-ada del modelo, así que el
        // Include va por el nombre de la navegación, no por expresión.
        var query = dbContext.Facturas.Include("_lineas").AsQueryable();
        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(f => f.FacturaId == facturaId);
    }

    public async Task AddAsync(Factura entity) => await dbContext.Facturas.AddAsync(entity);
}
