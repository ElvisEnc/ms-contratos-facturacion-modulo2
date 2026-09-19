using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Contratos;

public static class ContratoErrors
{
    public static Error PagoYaRegistrado() => new(
        "C13_PAGO_YA_REGISTRADO",
        "El contrato ya tiene un pago registrado.",
        ErrorType.Conflict);

    public static Error EstadoInvalido() => new(
        "C14_CONTRATO_ESTADO_INVALIDO",
        "El contrato debe estar en estado ACTIVO para realizar esta operación.",
        ErrorType.Conflict);

    public static Error NoCancelable() => new(
        "C14_CONTRATO_NO_CANCELABLE",
        "Solo se puede cancelar un contrato en estado ACTIVO.",
        ErrorType.Conflict);

    public static Error NoCompletable() => new(
        "C12_CONTRATO_NO_COMPLETABLE",
        "Solo se completa un contrato ACTIVO cuya vigencia ya terminó.",
        ErrorType.Conflict);
}
