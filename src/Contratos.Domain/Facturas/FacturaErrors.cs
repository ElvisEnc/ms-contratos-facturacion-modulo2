using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Facturas;

public static class FacturaErrors
{
    public static Error SinLineas() => new(
        "C11_FACTURA_SIN_LINEAS",
        "La factura debe tener al menos una línea.",
        ErrorType.Validation);

    public static Error PorcentajeInvalido() => new(
        "C11_PORCENTAJE_IMPUESTO_INVALIDO",
        "El porcentaje de impuesto debe estar entre 0 y 100.",
        ErrorType.Validation);

    public static Error EstadoInvalido() => new(
        "C6_ESTADO_FACTURA_INVALIDO",
        "La factura debe estar en estado EMITIDA para realizar esta operación.",
        ErrorType.Conflict);
}
