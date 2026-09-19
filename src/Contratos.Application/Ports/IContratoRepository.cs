using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Application.Ports;

public interface IContratoRepository : IRepository<Contrato>
{
    // C3 (RN-19): solo aplica a servicios distintos de CATERING.
    public Task<bool> ExisteContratoActivoParaPacienteYTipoAsync(PacienteId pacienteId, TipoServicio tipo);

    // C3 (RN-19): un contrato de catering activo cuya vigencia solape con la fecha dada.
    public Task<bool> ExisteCateringActivoParaPacienteAsync(PacienteId pacienteId, DateOnly fecha);

    // C7, segunda condición (RN-06): lectura binaria, no un contador.
    public Task<bool> TieneCateringVigenteEnAsync(PacienteId pacienteId, DateOnly fecha);

    // C12 (RN-20): cierre perezoso antes de recontratar.
    public Task<IReadOnlyCollection<Contrato>> GetActivosVencidosDePacienteYTipoAsync(
        PacienteId pacienteId, TipoServicio tipo, DateOnly hoy);
}
