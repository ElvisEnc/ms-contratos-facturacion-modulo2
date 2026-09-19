using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class LineaFacturaErrors
{
    public static Error ConceptoRequerido() => new(
        "C19_LINEA_CONCEPTO_REQUERIDO",
        "El concepto de la línea de factura es obligatorio.",
        ErrorType.Validation);

    public static Error CantidadInvalida() => new(
        "C19_LINEA_CANTIDAD_INVALIDA",
        "La cantidad debe ser mayor a cero.",
        ErrorType.Validation);
}
