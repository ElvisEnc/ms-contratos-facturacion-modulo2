using Joseco.DDD.Core.Results;

namespace Contratos.Application.Errors;

// §6.1 de DESIGN.md: las invariantes de orquestación se catalogan con el
// mismo prefijo C aunque el agregado no las garantice — es vocabulario de
// dominio aunque lo lance Application.
public static class ContratosErrors
{
    public static Error PacienteDesconocido() => new(
        "C1_PACIENTE_DESCONOCIDO",
        "El paciente no está registrado en este microservicio: el evento PacienteRegistrado de Pacientes todavía no llegó.",
        ErrorType.Validation);

    public static Error SinPlanActivo() => new(
        "C2_SIN_PLAN_ACTIVO",
        "El paciente no tiene un plan alimenticio activo: primero debe contratarse el asesoramiento y activarse el plan.",
        ErrorType.Validation);

    public static Error CateringYaActivo() => new(
        "C3_CATERING_YA_ACTIVO",
        "El paciente ya tiene un contrato de catering activo y vigente.",
        ErrorType.Conflict);

    public static Error DuracionInvalida() => new(
        "C4_DURACION_INVALIDA",
        "La duración del plan alimenticio debe ser de 15 o 30 días.",
        ErrorType.Validation);

    // C13, mitad de Application: el pago no cubre exactamente el total de la
    // factura, en su misma moneda. La otra mitad (unicidad del pago) es
    // ContratoErrors.PagoYaRegistrado en Domain.
    public static Error PagoNoCoincideConTotal() => new(
        "C13_PAGO_NO_COINCIDE_CON_TOTAL",
        "El pago debe cubrir exactamente el total de la factura, en su misma moneda: no hay pagos parciales.",
        ErrorType.Validation);

    public static Error ContratoNoEncontrado() => new(
        "CONTRATO_NO_ENCONTRADO",
        "No existe un contrato con el identificador indicado.",
        ErrorType.NotFound);

    // C16: si la réplica falta al publicar ServicioContratado, es un fallo
    // interno, no del cliente.
    public static Error PacienteSinReplica() => new(
        "PACIENTE_SIN_REPLICA",
        "El paciente no está en la réplica PacienteConocido al publicar el evento de integración; esto no debería ocurrir.",
        ErrorType.Failure);
}
