using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Catalogo;

public static class ServicioCatalogoErrors
{
    public static Error NombreRequerido() => new(
        "C15_SERVICIO_NOMBRE_REQUERIDO",
        "El nombre del servicio del catálogo es obligatorio.",
        ErrorType.Validation);

    public static Error Inactivo() => new(
        "C15_SERVICIO_CATALOGO_INACTIVO",
        "No se puede actualizar el precio de un servicio inactivo.",
        ErrorType.Conflict);
}
