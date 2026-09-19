using Contratos.Application.Ports;
using Contratos.Domain.Controles;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeControlAdicionalRepository : IControlAdicionalRepository
{
    private readonly Dictionary<Guid, ControlAdicional> _controles = [];

    public IReadOnlyCollection<ControlAdicional> Controles => _controles.Values.ToList();

    public void Agregar(ControlAdicional control) => _controles[control.Id] = control;

    public Task<ControlAdicional?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_controles.GetValueOrDefault(id));

    public Task AddAsync(ControlAdicional entity)
    {
        _controles[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> ExistePorEvaluacionAsync(EvaluacionId evaluacionId) =>
        Task.FromResult(_controles.Values.Any(c => c.EvaluacionId == evaluacionId));
}
