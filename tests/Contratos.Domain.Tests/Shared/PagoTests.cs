using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class PagoTests
{
    [Fact]
    public void Constructor_ConMontoInvalido_Falla()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Pago.Crear(new DateOnly(2026, 1, 1), Money.Crear(0m, "BOB"), "REF-1"));

        Assert.Equal("C20_PAGO_MONTO_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Constructor_ConReferenciaNula_LaNormalizaAVacio()
    {
        var pago = Pago.Crear(new DateOnly(2026, 1, 1), Money.Crear(100m, "BOB"), null);

        Assert.Equal(string.Empty, pago.Referencia);
    }
}
