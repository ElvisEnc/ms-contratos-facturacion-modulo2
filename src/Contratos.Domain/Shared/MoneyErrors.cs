using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class MoneyErrors
{
    public static Error MontoNegativo() => new(
        "C18_MONEY_MONTO_NEGATIVO",
        "El monto no puede ser negativo.",
        ErrorType.Validation);

    public static Error MonedaInvalida() => new(
        "C18_MONEY_MONEDA_INVALIDA",
        "La moneda debe ser BOB o USD.",
        ErrorType.Validation);

    public static Error MonedasIncompatibles() => new(
        "C18_MONEY_MONEDAS_INCOMPATIBLES",
        "No se pueden sumar montos de monedas distintas.",
        ErrorType.Validation);
}
