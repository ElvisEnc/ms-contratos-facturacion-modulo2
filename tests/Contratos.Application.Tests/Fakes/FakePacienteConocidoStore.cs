using Contratos.Application.Ports;
using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakePacienteConocidoStore : IPacienteConocidoStore
{
    private readonly Dictionary<Guid, PacienteConocido> _pacientes = [];

    public int Cantidad => _pacientes.Count;
    public int SaveChangesLlamadas { get; private set; }

    public void Agregar(PacienteConocido paciente) => _pacientes[paciente.PacienteId.Value] = paciente;

    public Task<PacienteConocido?> GetByIdAsync(PacienteId pacienteId) =>
        Task.FromResult(_pacientes.GetValueOrDefault(pacienteId.Value));

    public Task UpsertAsync(PacienteConocido paciente)
    {
        _pacientes[paciente.PacienteId.Value] = paciente;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        SaveChangesLlamadas++;
        return Task.CompletedTask;
    }
}
