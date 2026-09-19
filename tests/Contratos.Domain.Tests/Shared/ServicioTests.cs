using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class ServicioTests
{
    [Fact]
    public void Constructor_SinNombre_Falla()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Servicio.Crear(TipoServicio.EVALUACION, " ", Money.Crear(100m, "BOB")));

        Assert.Equal("C15_SERVICIO_NOMBRE_REQUERIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }
}
