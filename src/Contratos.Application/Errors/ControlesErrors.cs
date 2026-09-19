using Joseco.DDD.Core.Results;

namespace Contratos.Application.Errors;

public static class ControlesErrors
{
    public static Error ControlNoEncontrado() => new(
        "CONTROL_NO_ENCONTRADO",
        "No existe un control adicional con el identificador indicado.",
        ErrorType.NotFound);
}
