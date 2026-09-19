using Contratos.Domain.Controles;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Controles;

public class ControlAdicionalTests
{
    private static ControlAdicional Generar(decimal cobro) => ControlAdicional.Generar(
        ControlId.New(),
        PacienteId.From(Guid.NewGuid()),
        EvaluacionId.From(Guid.NewGuid()),
        new DateOnly(2026, 1, 1),
        Money.Crear(cobro, "BOB"),
        FacturaId.New());

    [Fact]
    public void Generar_ConCobroPositivo_NaceSinPagar()
    {
        var control = Generar(50m);

        Assert.False(control.Pagado);
    }

    [Fact]
    public void Generar_ConCobroCero_FallaConCobroInvalido()
    {
        var ex = Assert.Throws<DomainException>(() => Generar(0m));

        Assert.Equal("C21_CONTROL_COBRO_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void MarcarPagado_SobreControlNoPagado_QuedaPagado()
    {
        var control = Generar(50m);

        control.MarcarPagado();

        Assert.True(control.Pagado);
    }

    [Fact]
    public void MarcarPagado_SobreControlYaPagado_FallaConYaPagado()
    {
        var control = Generar(50m);
        control.MarcarPagado();

        var ex = Assert.Throws<DomainException>(control.MarcarPagado);

        Assert.Equal("C21_CONTROL_YA_PAGADO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }
}
