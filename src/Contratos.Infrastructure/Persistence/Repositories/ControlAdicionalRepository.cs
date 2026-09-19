using Contratos.Application.Ports;
using Contratos.Domain.Controles;
using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence.Repositories;

public sealed class ControlAdicionalRepository(ContratosDbContext dbContext) : IControlAdicionalRepository
{
    public async Task<ControlAdicional?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var controlId = ControlId.From(id);
        var query = dbContext.ControlesAdicionales.AsQueryable();
        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(c => c.ControlId == controlId);
    }

    public async Task AddAsync(ControlAdicional entity) => await dbContext.ControlesAdicionales.AddAsync(entity);

    // C10: clave de idempotencia de ProcesarEvaluacionRegistradaCommandHandler.
    public async Task<bool> ExistePorEvaluacionAsync(EvaluacionId evaluacionId)
    {
        return await dbContext.ControlesAdicionales
            .AsNoTracking()
            .AnyAsync(c => c.EvaluacionId == evaluacionId);
    }
}
