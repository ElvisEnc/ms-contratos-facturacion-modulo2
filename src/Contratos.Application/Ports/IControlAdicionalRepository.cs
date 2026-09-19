using Contratos.Domain.Controles;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Application.Ports;

public interface IControlAdicionalRepository : IRepository<ControlAdicional>
{
    // C10: clave de idempotencia de ProcesarEvaluacionRegistradaCommandHandler.
    public Task<bool> ExistePorEvaluacionAsync(EvaluacionId evaluacionId);
}
