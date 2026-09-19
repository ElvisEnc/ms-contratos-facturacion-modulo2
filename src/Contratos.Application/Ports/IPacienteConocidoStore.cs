using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;

namespace Contratos.Application.Ports;

// Persiste directamente contra el DbContext, fuera del UnitOfWork: no emite
// eventos de dominio y el ChangeTracker no le aporta nada (§9.4, §12.9 del maestro).
public interface IPacienteConocidoStore
{
    public Task<PacienteConocido?> GetByIdAsync(PacienteId pacienteId);
    public Task UpsertAsync(PacienteConocido paciente);
    public Task SaveChangesAsync();
}
