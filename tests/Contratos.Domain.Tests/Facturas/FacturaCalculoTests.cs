using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Facturas;

public class FacturaCalculoTests
{
    private static Factura Emitir(IReadOnlyCollection<DetalleLinea> detalles, decimal pct) =>
        Factura.Emitir(PacienteId.From(Guid.NewGuid()), OrigenFactura.CONTRATACION, Guid.NewGuid(), detalles, pct);

    [Fact]
    public void Emitir_ConDosLineas_CalculaSubtotalEsperado()
    {
        var detalles = new[]
        {
            new DetalleLinea("Evaluación", 1, Money.Crear(200m, "BOB")),
            new DetalleLinea("Asesoramiento", 1, Money.Crear(150m, "BOB"))
        };

        var factura = Emitir(detalles, 0m);

        Assert.Equal(350m, factura.Subtotal.Monto);
    }

    [Fact]
    public void Emitir_ConPorcentaje13_CalculaImpuestoYTotalEsperados()
    {
        var detalles = new[] { new DetalleLinea("Catering", 1, Money.Crear(1000m, "BOB")) };

        var factura = Emitir(detalles, 13m);

        Assert.Equal(130m, factura.Impuesto.Monto);
        Assert.Equal(1130m, factura.Total.Monto);
    }

    [Fact]
    public void Emitir_ConPorcentajeCero_ImpuestoEsCeroYTotalIgualASubtotal()
    {
        var detalles = new[] { new DetalleLinea("Catering", 1, Money.Crear(500m, "BOB")) };

        var factura = Emitir(detalles, 0m);

        Assert.Equal(0m, factura.Impuesto.Monto);
        Assert.Equal(factura.Subtotal.Monto, factura.Total.Monto);
    }

    [Fact]
    public void Emitir_ConPorcentaje21_AjustaElCalculo()
    {
        var detalles = new[] { new DetalleLinea("Catering", 1, Money.Crear(1000m, "BOB")) };

        var factura = Emitir(detalles, 21m);

        Assert.Equal(210m, factura.Impuesto.Monto);
        Assert.Equal(1210m, factura.Total.Monto);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void Emitir_ConPorcentajeFueraDeRango_FallaConPorcentajeImpuestoInvalido(decimal pct)
    {
        var detalles = new[] { new DetalleLinea("Catering", 1, Money.Crear(1000m, "BOB")) };

        var ex = Assert.Throws<DomainException>(() => Emitir(detalles, pct));

        Assert.Equal("C11_PORCENTAJE_IMPUESTO_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Emitir_SinLineas_FallaConFacturaSinLineas()
    {
        var ex = Assert.Throws<DomainException>(() => Emitir([], 13m));

        Assert.Equal("C11_FACTURA_SIN_LINEAS", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Emitir_ConLineasDeMonedasDistintas_FallaConMonedasIncompatibles()
    {
        var detalles = new[]
        {
            new DetalleLinea("Catering", 1, Money.Crear(1000m, "BOB")),
            new DetalleLinea("Asesoramiento", 1, Money.Crear(50m, "USD"))
        };

        var ex = Assert.Throws<DomainException>(() => Emitir(detalles, 13m));

        Assert.Equal("C18_MONEY_MONEDAS_INCOMPATIBLES", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Emitir_ConMontoQueRequiereRedondeo_SubtotalMasImpuestoIgualaTotalExactamente()
    {
        var detalles = new[] { new DetalleLinea("Catering", 1, Money.Crear(333.33m, "BOB")) };

        var factura = Emitir(detalles, 13m);

        Assert.Equal(factura.Subtotal.Monto + factura.Impuesto.Monto, factura.Total.Monto);
    }
}
