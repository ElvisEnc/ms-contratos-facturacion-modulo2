using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class PagoErrors
{
    public static Error MontoInvalido() => new(
        "C20_PAGO_MONTO_INVALIDO",
        "El monto del pago debe ser mayor a cero.",
        ErrorType.Validation);
}
