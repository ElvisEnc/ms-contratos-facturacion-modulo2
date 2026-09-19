using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Catalogo;

public class ServicioCatalogoTests
{
    [Fact]
    public void Crear_SinNombre_FallaConNombreRequerido()
    {
        var ex = Assert.Throws<DomainException>(() =>
            ServicioCatalogo.Crear(TipoServicio.EVALUACION, " ", Money.Crear(100m, "BOB")));

        Assert.Equal("C15_SERVICIO_NOMBRE_REQUERIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void ActualizarPrecio_SobreServicioInactivo_FallaConInactivo()
    {
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));
        servicio.Desactivar();

        var ex = Assert.Throws<DomainException>(() => servicio.ActualizarPrecio(Money.Crear(150m, "BOB")));

        Assert.Equal("C15_SERVICIO_CATALOGO_INACTIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void ObtenerServicioParaContrato_TrasActualizarPrecio_ConservaElPrecioCopiado()
    {
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));
        var copia = servicio.ObtenerServicioParaContrato();

        servicio.ActualizarPrecio(Money.Crear(999m, "BOB"));

        Assert.Equal(100m, copia.Precio.Monto);
    }

    [Fact]
    public void ObtenerServicioParaContrato_DevuelveUnMoneyClonado_NoLaMismaInstancia()
    {
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));

        var copia = servicio.ObtenerServicioParaContrato();

        Assert.NotSame(servicio.PrecioVigente, copia.Precio);
    }

    [Fact]
    public void Desactivar_LlamadoDosVeces_EsIdempotente()
    {
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));

        servicio.Desactivar();
        servicio.Desactivar();

        Assert.False(servicio.Activo);
    }
}
