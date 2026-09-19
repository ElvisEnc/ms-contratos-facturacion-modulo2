using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Contratos;

public class ContratoTests
{
    private static Contrato CrearContrato(TipoServicio tipo, DateOnly fechaInicio, int duracionDias)
    {
        var servicio = Servicio.Crear(tipo, "Catering mensual", Money.Crear(1000m, "BOB"));
        var vigencia = Vigencia.Desde(fechaInicio, duracionDias);

        return Contrato.Contratar(
            ContratoId.New(),
            PacienteId.From(Guid.NewGuid()),
            servicio,
            vigencia,
            FacturaId.New());
    }

    [Fact]
    public void Completar_ConVigenciaVencida_PasaACompletado()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);

        contrato.Completar(inicio.AddDays(15));

        Assert.Equal(EstadoContrato.COMPLETADO, contrato.Estado);
    }

    [Fact]
    public void Completar_ConVigenciaEnCurso_FallaConNoCompletable()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);

        var ex = Assert.Throws<DomainException>(() => contrato.Completar(inicio.AddDays(5)));

        Assert.Equal("C12_CONTRATO_NO_COMPLETABLE", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void Completar_SobreContratoCancelado_FallaConNoCompletable()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);
        contrato.Cancelar();

        var ex = Assert.Throws<DomainException>(() => contrato.Completar(inicio.AddDays(30)));

        Assert.Equal("C12_CONTRATO_NO_COMPLETABLE", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void RegistrarPago_DosVeces_FallaConPagoYaRegistrado()
    {
        var contrato = CrearContrato(TipoServicio.EVALUACION, new DateOnly(2026, 1, 1), 1);
        contrato.RegistrarPago(Pago.Crear(new DateOnly(2026, 1, 1), Money.Crear(226m, "BOB"), "REF-1"));

        var ex = Assert.Throws<DomainException>(() =>
            contrato.RegistrarPago(Pago.Crear(new DateOnly(2026, 1, 1), Money.Crear(226m, "BOB"), "REF-2")));

        Assert.Equal("C13_PAGO_YA_REGISTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void RegistrarPago_SobreContratoCancelado_FallaConEstadoInvalido()
    {
        var contrato = CrearContrato(TipoServicio.EVALUACION, new DateOnly(2026, 1, 1), 1);
        contrato.Cancelar();

        var ex = Assert.Throws<DomainException>(() =>
            contrato.RegistrarPago(Pago.Crear(new DateOnly(2026, 1, 1), Money.Crear(226m, "BOB"), null)));

        Assert.Equal("C14_CONTRATO_ESTADO_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void Cancelar_ContratoYaCancelado_FallaConNoCancelable()
    {
        var contrato = CrearContrato(TipoServicio.EVALUACION, new DateOnly(2026, 1, 1), 1);
        contrato.Cancelar();

        var ex = Assert.Throws<DomainException>(contrato.Cancelar);

        Assert.Equal("C14_CONTRATO_NO_CANCELABLE", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void EstadoDerivado_DentroDeVigencia_DevuelveActivo()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);

        Assert.Equal(EstadoContrato.ACTIVO, contrato.EstadoDerivado(inicio.AddDays(5)));
    }

    [Fact]
    public void EstadoDerivado_ElDiaExactoDeFechaFin_DevuelveActivo()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);

        Assert.Equal(EstadoContrato.ACTIVO, contrato.EstadoDerivado(inicio.AddDays(14)));
    }

    [Fact]
    public void EstadoDerivado_ElDiaSiguienteAFechaFin_DevuelveCompletado()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);

        Assert.Equal(EstadoContrato.COMPLETADO, contrato.EstadoDerivado(inicio.AddDays(15)));
    }

    [Fact]
    public void EstadoDerivado_SobreContratoCancelado_DevuelveCanceladoAunqueVigenciaHayaPasado()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var contrato = CrearContrato(TipoServicio.CATERING, inicio, 15);
        contrato.Cancelar();

        Assert.Equal(EstadoContrato.CANCELADO, contrato.EstadoDerivado(inicio.AddDays(100)));
    }

    [Theory]
    [InlineData(TipoServicio.CATERING, true)]
    [InlineData(TipoServicio.EVALUACION, false)]
    [InlineData(TipoServicio.ASESORAMIENTO, false)]
    public void EsCatering_DistingueCorrectamente(TipoServicio tipo, bool esperado)
    {
        var contrato = CrearContrato(tipo, new DateOnly(2026, 1, 1), 1);

        Assert.Equal(esperado, contrato.EsCatering());
    }
}
