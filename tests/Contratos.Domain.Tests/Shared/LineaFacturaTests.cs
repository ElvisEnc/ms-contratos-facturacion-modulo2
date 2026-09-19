using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class LineaFacturaTests
{
    [Fact]
    public void Subtotal_SeCalculaYNoSeRecibe()
    {
        var linea = new LineaFactura(LineaFacturaId.New(), "Catering", 3, Money.Crear(100m, "BOB"));

        Assert.Equal(300m, linea.Subtotal.Monto);
    }

    [Fact]
    public void Constructor_ConCantidadCero_Falla()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new LineaFactura(LineaFacturaId.New(), "Catering", 0, Money.Crear(100m, "BOB")));

        Assert.Equal("C19_LINEA_CANTIDAD_INVALIDA", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Constructor_SinConcepto_Falla()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new LineaFactura(LineaFacturaId.New(), " ", 1, Money.Crear(100m, "BOB")));

        Assert.Equal("C19_LINEA_CONCEPTO_REQUERIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }
}
