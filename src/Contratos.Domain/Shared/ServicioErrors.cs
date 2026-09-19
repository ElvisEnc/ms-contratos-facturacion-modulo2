using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class ServicioErrors
{
    public static Error NombreRequerido() => new(
        "C15_SERVICIO_NOMBRE_REQUERIDO",
        "El nombre del servicio del catálogo es obligatorio.",
        ErrorType.Validation);
}
