using Contratos.Application.Contratos.RegistrarPago;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Contratos;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Contratos;

public class RegistrarPagoCommandHandlerTests
{
    private static (Contrato Contrato, Factura Factura) CrearContratoYFactura(decimal precioLinea, decimal pctImpuesto, string moneda = "BOB")
    {
        var pacienteId = PacienteId.From(Guid.NewGuid());
        var contratoId = ContratoId.New();

        var factura = Factura.Emitir(
            pacienteId,
            OrigenFactura.CONTRATACION,
            contratoId.Value,
            [new DetalleLinea("Servicio de prueba", 1, Money.Crear(precioLinea, moneda))],
            pctImpuesto);

        var servicio = Servicio.Crear(TipoServicio.EVALUACION, "Servicio de prueba", Money.Crear(precioLinea, moneda));
        var contrato = Contrato.Contratar(
            contratoId,
            pacienteId,
            servicio,
            Vigencia.Desde(DateOnly.FromDateTime(DateTime.UtcNow), 1),
            factura.FacturaId);

        return (contrato, factura);
    }

    private static (RegistrarPagoCommandHandler Handler, FakeContratoRepository Contratos, FakeFacturaRepository Facturas, FakeUnitOfWork UnitOfWork)
        CrearHandler(Contrato contrato, Factura factura)
    {
        var contratos = new FakeContratoRepository();
        var facturas = new FakeFacturaRepository();
        contratos.Agregar(contrato);
        facturas.Agregar(factura);
        var unitOfWork = new FakeUnitOfWork();

        return (new RegistrarPagoCommandHandler(contratos, facturas, unitOfWork), contratos, facturas, unitOfWork);
    }

    [Fact]
    public async Task Handle_HappyPath_MarcaFacturaPagadaYRegistraElPagoEnElContrato()
    {
        var (contrato, factura) = CrearContratoYFactura(200m, 13m); // subtotal 200, impuesto 26, total 226
        var (handler, _, _, unitOfWork) = CrearHandler(contrato, factura);
        var command = new RegistrarPagoCommand(contrato.Id, 226m, "BOB", "REF-1");

        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(contrato.Pago);
        Assert.Equal(226m, contrato.Pago!.Monto.Monto);
        Assert.Equal("BOB", contrato.Pago.Monto.Moneda);
        Assert.Equal(EstadoFactura.PAGADA, factura.Estado);
        Assert.Equal(1, unitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_MontoDistintoDelTotal_FallaConC13()
    {
        var (contrato, factura) = CrearContratoYFactura(200m, 13m); // total 226
        var (handler, _, _, _) = CrearHandler(contrato, factura);
        var command = new RegistrarPagoCommand(contrato.Id, 1m, "BOB", null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal("C13_PAGO_NO_COINCIDE_CON_TOTAL", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_MonedaDistintaDeLaDeLaFactura_FallaConC13()
    {
        var (contrato, factura) = CrearContratoYFactura(200m, 13m, moneda: "BOB"); // total 226 BOB
        var (handler, _, _, _) = CrearHandler(contrato, factura);
        var command = new RegistrarPagoCommand(contrato.Id, 226m, "USD", null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal("C13_PAGO_NO_COINCIDE_CON_TOTAL", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_ContratoInexistente_FallaConContratoNoEncontrado()
    {
        var contratos = new FakeContratoRepository();
        var facturas = new FakeFacturaRepository();
        var handler = new RegistrarPagoCommandHandler(contratos, facturas, new FakeUnitOfWork());
        var command = new RegistrarPagoCommand(Guid.NewGuid(), 226m, "BOB", null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal("CONTRATO_NO_ENCONTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.NotFound, ex.Error.Type);
    }
}
