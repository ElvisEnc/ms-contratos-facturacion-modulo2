using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class VigenciaTests
{
    [Fact]
    public void Constructor_ConFechaFinAnteriorAInicio_Falla()
    {
        var inicio = new DateOnly(2026, 1, 15);
        var fin = new DateOnly(2026, 1, 1);

        var ex = Assert.Throws<DomainException>(() => new Vigencia(inicio, fin, 15));

        Assert.Equal("C17_VIGENCIA_FECHAS_INVALIDAS", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Constructor_ConDuracionIncoherenteConFechas_Falla()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var fin = new DateOnly(2026, 1, 15);

        var ex = Assert.Throws<DomainException>(() => new Vigencia(inicio, fin, 30));

        Assert.Equal("C17_VIGENCIA_DURACION_INCOHERENTE", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void Desde_Con15Dias_ProduceFechaFinInicioMas14Dias()
    {
        var inicio = new DateOnly(2026, 1, 1);

        var vigencia = Vigencia.Desde(inicio, 15);

        Assert.Equal(inicio.AddDays(14), vigencia.FechaFin);
    }

    [Fact]
    public void EstaVigenteEn_EsVerdaderoEnAmbosBordesYFalsoUnDiaDespues()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var vigencia = Vigencia.Desde(inicio, 15);

        Assert.True(vigencia.EstaVigenteEn(vigencia.FechaInicio));
        Assert.True(vigencia.EstaVigenteEn(vigencia.FechaFin));
        Assert.False(vigencia.EstaVigenteEn(vigencia.FechaFin.AddDays(1)));
    }
}
