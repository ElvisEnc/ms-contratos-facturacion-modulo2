using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public static class TypedIdValidator
{
    public static void ValidarNoVacio(Guid id)
    {
        // C22: ningún identificador tipado admite Guid.Empty.
        if (id == Guid.Empty)
            throw new DomainException(new Error(
                "ID_VACIO",
                "El identificador no puede estar vacío.",
                ErrorType.Validation));
    }
}
