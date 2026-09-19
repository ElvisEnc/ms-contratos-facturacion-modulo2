using Contratos.Application.Ports;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Contratos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence.Repositories;

public sealed class ContratoRepository(ContratosDbContext dbContext) : IContratoRepository
{
    public async Task<Contrato?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        var contratoId = ContratoId.From(id);
        var query = dbContext.Contratos.AsQueryable();
        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(c => c.ContratoId == contratoId);
    }

    public async Task AddAsync(Contrato entity) => await dbContext.Contratos.AddAsync(entity);

    // C3 (RN-19): solo aplica a servicios distintos de CATERING.
    public async Task<bool> ExisteContratoActivoParaPacienteYTipoAsync(PacienteId pacienteId, TipoServicio tipo)
    {
        return await dbContext.Contratos
            .AsNoTracking()
            .AnyAsync(c => c.PacienteId == pacienteId
                && c.Servicio.Tipo == tipo
                && c.Estado == EstadoContrato.ACTIVO);
    }

    // C3 (RN-19): un contrato de catering activo cuya vigencia solape con la fecha dada.
    public async Task<bool> ExisteCateringActivoParaPacienteAsync(PacienteId pacienteId, DateOnly fecha)
    {
        return await dbContext.Contratos
            .AsNoTracking()
            .AnyAsync(c => c.PacienteId == pacienteId
                && c.Servicio.Tipo == TipoServicio.CATERING
                && c.Estado == EstadoContrato.ACTIVO
                && c.Vigencia.FechaInicio <= fecha
                && c.Vigencia.FechaFin >= fecha);
    }

    // C7, segunda condición (RN-06): lectura binaria, no un contador.
    public async Task<bool> TieneCateringVigenteEnAsync(PacienteId pacienteId, DateOnly fecha)
    {
        return await dbContext.Contratos
            .AsNoTracking()
            .AnyAsync(c => c.PacienteId == pacienteId
                && c.Servicio.Tipo == TipoServicio.CATERING
                && c.Estado == EstadoContrato.ACTIVO
                && c.Vigencia.FechaInicio <= fecha
                && c.Vigencia.FechaFin >= fecha);
    }

    // C12 (RN-20): cierre perezoso antes de recontratar.
    public async Task<IReadOnlyCollection<Contrato>> GetActivosVencidosDePacienteYTipoAsync(
        PacienteId pacienteId, TipoServicio tipo, DateOnly hoy)
    {
        return await dbContext.Contratos
            .Where(c => c.PacienteId == pacienteId
                && c.Servicio.Tipo == tipo
                && c.Estado == EstadoContrato.ACTIVO
                && c.Vigencia.FechaFin < hoy)
            .ToListAsync();
    }
}
