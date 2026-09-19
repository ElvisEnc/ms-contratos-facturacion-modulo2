using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Controles;

public static class ControlAdicionalErrors
{
    public static Error CobroInvalido() => new(
        "C21_CONTROL_COBRO_INVALIDO",
        "El cobro del control adicional debe ser mayor a cero.",
        ErrorType.Validation);

    public static Error YaPagado() => new(
        "C21_CONTROL_YA_PAGADO",
        "El control adicional ya fue pagado.",
        ErrorType.Conflict);
}
