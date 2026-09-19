using Joseco.DDD.Core.Results;

namespace Contratos.Application.Errors;

public static class FacturasErrors
{
    // Referencia interna: no aparece la factura referenciada por el contrato
    // o el control — esto no debería ocurrir (C5 la garantiza en el commit).
    public static Error FacturaNoEncontrada() => new(
        "FACTURA_NO_ENCONTRADA",
        "No aparece la factura referenciada por este contrato o control; esto no debería ocurrir.",
        ErrorType.Failure);
}
