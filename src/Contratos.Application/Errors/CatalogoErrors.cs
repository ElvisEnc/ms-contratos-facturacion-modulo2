using Joseco.DDD.Core.Results;

namespace Contratos.Application.Errors;

public static class CatalogoErrors
{
    public static Error ServicioInactivo() => new(
        "C9_SERVICIO_INACTIVO",
        "No se puede contratar un servicio inactivo en el catálogo.",
        ErrorType.Validation);

    public static Error ServicioNoEncontrado() => new(
        "SERVICIO_NO_ENCONTRADO",
        "No existe un servicio del catálogo con el identificador indicado.",
        ErrorType.NotFound);

    // Referencia interna que el sistema debería garantizar: el catálogo no
    // tiene configurado un servicio activo para el tipo pedido.
    public static Error CatalogoMalConfigurado() => new(
        "CATALOGO_MAL_CONFIGURADO",
        "El catálogo no tiene configurado un servicio activo para este tipo. Contacte al administrador.",
        ErrorType.Failure);
}
