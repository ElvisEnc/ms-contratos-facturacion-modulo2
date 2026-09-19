using Contratos.Application.Ports;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeContratoRepository : IContratoRepository
{
    private readonly Dictionary<Guid, Contrato> _contratos = [];

    public IReadOnlyCollection<Contrato> Contratos => _contratos.Values.ToList();

    // Instrumentación de espía para Handle_MismaEvaluacionDosVeces_LaSegundaVezNiSiquieraConsultaLaVigenciaDelCatering.
    public int TieneCateringVigenteEnAsyncLlamadas { get; private set; }

    public void Agregar(Contrato contrato) => _contratos[contrato.Id] = contrato;

    public Task<Contrato?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_contratos.GetValueOrDefault(id));

    public Task AddAsync(Contrato entity)
    {
        _contratos[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> ExisteContratoActivoParaPacienteYTipoAsync(PacienteId pacienteId, TipoServicio tipo) =>
        Task.FromResult(_contratos.Values.Any(c =>
            c.PacienteId == pacienteId && c.Servicio.Tipo == tipo && c.Estado == EstadoContrato.ACTIVO));

    public Task<bool> ExisteCateringActivoParaPacienteAsync(PacienteId pacienteId, DateOnly fecha) =>
        Task.FromResult(_contratos.Values.Any(c =>
            c.PacienteId == pacienteId
            && c.EsCatering()
            && c.Estado == EstadoContrato.ACTIVO
            && c.Vigencia.EstaVigenteEn(fecha)));

    public Task<bool> TieneCateringVigenteEnAsync(PacienteId pacienteId, DateOnly fecha)
    {
        TieneCateringVigenteEnAsyncLlamadas++;

        return Task.FromResult(_contratos.Values.Any(c =>
            c.PacienteId == pacienteId
            && c.EsCatering()
            && c.Estado == EstadoContrato.ACTIVO
            && c.Vigencia.EstaVigenteEn(fecha)));
    }

    public Task<IReadOnlyCollection<Contrato>> GetActivosVencidosDePacienteYTipoAsync(
        PacienteId pacienteId, TipoServicio tipo, DateOnly hoy) =>
        Task.FromResult<IReadOnlyCollection<Contrato>>(_contratos.Values
            .Where(c => c.PacienteId == pacienteId
                && c.Servicio.Tipo == tipo
                && c.Estado == EstadoContrato.ACTIVO
                && hoy > c.Vigencia.FechaFin)
            .ToList());
}
