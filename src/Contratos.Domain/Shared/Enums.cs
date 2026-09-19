namespace Contratos.Domain.Shared;

public enum TipoServicio
{
    EVALUACION,
    ASESORAMIENTO,
    CATERING
}

public enum EstadoContrato
{
    ACTIVO,
    COMPLETADO,
    CANCELADO
}

public enum EstadoFactura
{
    EMITIDA,
    PAGADA,
    ANULADA
}

public enum OrigenFactura
{
    CONTRATACION,
    CONTROL_ADICIONAL
}
