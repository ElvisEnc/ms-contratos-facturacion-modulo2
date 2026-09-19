using Contratos.Application.Contratos.CancelarContrato;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Contratos;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Contratos;

public class CancelarContratoCommandHandlerTests
{
    private static (Contrato Contrato, Factura Factura) CrearContratoYFactura()
    {
        var pacienteId = PacienteId.From(Guid.NewGuid());
        var contratoId = ContratoId.New();

        var factura = Factura.Emitir(
            pacienteId,
            OrigenFactura.CONTRATACION,
            contratoId.Value,
            [new DetalleLinea("Servicio de prueba", 1, Money.Crear(200m, "BOB"))],
            13m);

        var servicio = Servicio.Crear(TipoServicio.EVALUACION, "Servicio de prueba", Money.Crear(200m, "BOB"));
        var contrato = Contrato.Contratar(
            contratoId,
            pacienteId,
            servicio,
            Vigencia.Desde(DateOnly.FromDateTime(DateTime.UtcNow), 1),
            factura.FacturaId);

        return (contrato, factura);
    }

    [Fact]
    public async Task Handle_FacturaEmitida_SeAnulaAlCancelar()
    {
        var (contrato, factura) = CrearContratoYFactura();
        var contratos = new FakeContratoRepository();
        var facturas = new FakeFacturaRepository();
        contratos.Agregar(contrato);
        facturas.Agregar(factura);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelarContratoCommandHandler(contratos, facturas, unitOfWork);

        await handler.Handle(new CancelarContratoCommand(contrato.Id), CancellationToken.None);

        Assert.Equal(EstadoContrato.CANCELADO, contrato.Estado);
        Assert.Equal(EstadoFactura.ANULADA, factura.Estado);
        Assert.Equal(1, unitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_FacturaYaPagada_C8_PermanecePagadaAlCancelar()
    {
        var (contrato, factura) = CrearContratoYFactura();
        factura.MarcarPagada();
        var contratos = new FakeContratoRepository();
        var facturas = new FakeFacturaRepository();
        contratos.Agregar(contrato);
        facturas.Agregar(factura);
        var handler = new CancelarContratoCommandHandler(contratos, facturas, new FakeUnitOfWork());

        await handler.Handle(new CancelarContratoCommand(contrato.Id), CancellationToken.None);

        Assert.Equal(EstadoContrato.CANCELADO, contrato.Estado);
        Assert.Equal(EstadoFactura.PAGADA, factura.Estado); // C8: una factura pagada permanece pagada
    }

    [Fact]
    public async Task Handle_ContratoInexistente_FallaConContratoNoEncontrado()
    {
        var handler = new CancelarContratoCommandHandler(
            new FakeContratoRepository(), new FakeFacturaRepository(), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new CancelarContratoCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("CONTRATO_NO_ENCONTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.NotFound, ex.Error.Type);
    }
}
