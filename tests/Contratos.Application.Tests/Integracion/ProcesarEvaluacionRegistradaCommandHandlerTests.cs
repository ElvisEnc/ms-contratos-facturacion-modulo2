using Contratos.Application.Configuration;
using Contratos.Application.Integracion.ProcesarEvaluacionRegistrada;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Integracion;

public class ProcesarEvaluacionRegistradaCommandHandlerTests
{
    private sealed record Escenario(
        ProcesarEvaluacionRegistradaCommandHandler Handler,
        FakeControlAdicionalRepository Controles,
        FakeContratoRepository Contratos,
        FakeServicioCatalogoRepository Catalogo,
        FakeFacturaRepository Facturas,
        FakeUnitOfWork UnitOfWork);

    private static Escenario CrearEscenario(bool conServicioEvaluacionEnCatalogo = true)
    {
        var controles = new FakeControlAdicionalRepository();
        var contratos = new FakeContratoRepository();
        var catalogo = new FakeServicioCatalogoRepository();
        var facturas = new FakeFacturaRepository();
        var unitOfWork = new FakeUnitOfWork();

        if (conServicioEvaluacionEnCatalogo)
            catalogo.Agregar(ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación corporal y consulta", Money.Crear(150m, "BOB")));

        var opciones = new FacturacionOptions();
        var handler = new ProcesarEvaluacionRegistradaCommandHandler(controles, contratos, catalogo, facturas, opciones, unitOfWork);

        return new Escenario(handler, controles, contratos, catalogo, facturas, unitOfWork);
    }

    private static void AgregarCateringVigente(FakeContratoRepository contratos, Guid pacienteId, DateOnly fecha)
    {
        var servicio = ServicioCatalogo.Crear(TipoServicio.CATERING, "Catering", Money.Crear(500m, "BOB"));
        var contrato = Contrato.Contratar(
            ContratoId.New(),
            PacienteId.From(pacienteId),
            servicio.ObtenerServicioParaContrato(),
            Vigencia.Desde(fecha, 15),
            FacturaId.New());

        contratos.Agregar(contrato);
    }

    [Fact]
    public async Task Handle_ControlSinCatering_GeneraControlYFactura()
    {
        var e = CrearEscenario();
        var command = new ProcesarEvaluacionRegistradaCommand(
            Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), "CONTROL");

        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Single(e.Controles.Controles);
        Assert.Single(e.Facturas.Facturas);
        Assert.Equal(1, e.UnitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_ControlConCateringVigente_NoGeneraNada()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        AgregarCateringVigente(e.Contratos, pacienteId, fecha);

        var command = new ProcesarEvaluacionRegistradaCommand(pacienteId, Guid.NewGuid(), fecha, "CONTROL");

        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Empty(e.Controles.Controles);
        Assert.Empty(e.Facturas.Facturas);
        Assert.Equal(0, e.UnitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_Quincenal_NoGeneraNadaAunqueNoHayaCatering()
    {
        var e = CrearEscenario();
        var command = new ProcesarEvaluacionRegistradaCommand(
            Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), "QUINCENAL");

        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Empty(e.Controles.Controles);
        Assert.Equal(0, e.UnitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_Inicial_NoGeneraNadaAunqueNoHayaCatering()
    {
        var e = CrearEscenario();
        var command = new ProcesarEvaluacionRegistradaCommand(
            Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), "INICIAL");

        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Empty(e.Controles.Controles);
        Assert.Equal(0, e.UnitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_MismaEvaluacionDosVeces_C10_GeneraUnSoloControl()
    {
        var e = CrearEscenario();
        var evaluacionId = Guid.NewGuid();
        var command = new ProcesarEvaluacionRegistradaCommand(
            Guid.NewGuid(), evaluacionId, DateOnly.FromDateTime(DateTime.UtcNow), "CONTROL");

        await e.Handler.Handle(command, CancellationToken.None);
        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Single(e.Controles.Controles);
        Assert.Single(e.Facturas.Facturas);
        Assert.Equal(1, e.UnitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_MismaEvaluacionDosVeces_LaSegundaVezNiSiquieraConsultaLaVigenciaDelCatering()
    {
        var e = CrearEscenario();
        var evaluacionId = Guid.NewGuid();
        var command = new ProcesarEvaluacionRegistradaCommand(
            Guid.NewGuid(), evaluacionId, DateOnly.FromDateTime(DateTime.UtcNow), "CONTROL");

        await e.Handler.Handle(command, CancellationToken.None);
        var llamadasTrasPrimera = e.Contratos.TieneCateringVigenteEnAsyncLlamadas;
        Assert.Equal(1, llamadasTrasPrimera);

        await e.Handler.Handle(command, CancellationToken.None);

        Assert.Equal(llamadasTrasPrimera, e.Contratos.TieneCateringVigenteEnAsyncLlamadas);
    }
}
