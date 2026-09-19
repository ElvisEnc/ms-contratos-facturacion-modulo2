using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class MoneyTests
{
    [Theory]
    [InlineData("BOB")]
    [InlineData("USD")]
    public void Crear_ConMonedaValida_ConstruyeElVO(string moneda)
    {
        var dinero = Money.Crear(100m, moneda);

        Assert.Equal(moneda, dinero.Moneda);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("bob")]
    public void Crear_ConMonedaFueraDeLaListaCerrada_FallaConMonedaInvalida(string moneda)
    {
        var ex = Assert.Throws<DomainException>(() => Money.Crear(100m, moneda));

        Assert.Equal("C18_MONEY_MONEDA_INVALIDA", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Constructor_ConMontoNegativo_Falla()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Crear(-1m, "BOB"));

        Assert.Equal("C18_MONEY_MONTO_NEGATIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Sumar_ConMonedasDistintas_Falla()
    {
        var bob = Money.Crear(100m, "BOB");
        var usd = Money.Crear(100m, "USD");

        var ex = Assert.Throws<DomainException>(() => bob.Sumar(usd));

        Assert.Equal("C18_MONEY_MONEDAS_INCOMPATIBLES", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void PorPorcentaje_Redondea2Decimales()
    {
        var dinero = Money.Crear(333.33m, "BOB");

        var resultado = dinero.PorPorcentaje(13m);

        Assert.Equal(43.33m, resultado.Monto);
    }

    [Fact]
    public void Clonar_DevuelveOtraInstanciaConElMismoValor()
    {
        var original = Money.Crear(100m, "BOB");

        var clon = original.Clonar();

        Assert.NotSame(original, clon);
        Assert.Equal(original.Monto, clon.Monto);
        Assert.Equal(original.Moneda, clon.Moneda);
    }
}
