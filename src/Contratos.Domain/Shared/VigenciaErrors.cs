using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class VigenciaErrors
{
    public static Error FechasInvalidas() => new(
        "C17_VIGENCIA_FECHAS_INVALIDAS",
        "La fecha de fin no puede ser anterior a la fecha de inicio.",
        ErrorType.Validation);

    public static Error DuracionIncoherente() => new(
        "C17_VIGENCIA_DURACION_INCOHERENTE",
        "La duración en días no coincide con el rango de fechas.",
        ErrorType.Validation);
}
