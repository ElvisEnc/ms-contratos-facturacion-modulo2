using Contratos.Application.Ports;
using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence;

// Persiste directamente contra el DbContext, fuera del UnitOfWork: el read
// model no es un agregado, no emite eventos de dominio y el ChangeTracker
// no le aporta nada (§9.4, §12.9 del maestro).
public sealed class PacienteConocidoStore(ContratosDbContext dbContext) : IPacienteConocidoStore
{
    public async Task<PacienteConocido?> GetByIdAsync(PacienteId pacienteId)
    {
        return await dbContext.PacientesConocidos
            .FirstOrDefaultAsync(p => p.PacienteId == pacienteId);
    }

    public async Task UpsertAsync(PacienteConocido paciente)
    {
        var existente = await dbContext.PacientesConocidos
            .FirstOrDefaultAsync(p => p.PacienteId == paciente.PacienteId);

        if (existente is null)
        {
            await dbContext.PacientesConocidos.AddAsync(paciente);
            return;
        }

        dbContext.Entry(existente).CurrentValues.SetValues(paciente);
    }

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}
