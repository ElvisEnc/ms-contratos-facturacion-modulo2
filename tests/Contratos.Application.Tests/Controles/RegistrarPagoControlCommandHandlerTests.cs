using Contratos.Application.Controles.RegistrarPagoControl;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Controles;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Controles;

public class RegistrarPagoControlCommandHandlerTests
{
    private static (ControlAdicional Control, Factura Factura) CrearControlYFactura()
    {
        var pacienteId = PacienteId.From(Guid.NewGuid());
        var controlId = ControlId.New();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var factura = Factura.Emitir(
            pacienteId,
            OrigenFactura.CONTROL_ADICIONAL,
            controlId.Value,
            [new DetalleLinea("Evaluación corporal y consulta", 1, Money.Crear(150m, "BOB"))],
            13m);

        var control = ControlAdicional.Generar(
            controlId, pacienteId, EvaluacionId.From(Guid.NewGuid()), hoy, Money.Crear(150m, "BOB"), factura.FacturaId);

        return (control, factura);
    }

    [Fact]
    public async Task Handle_HappyPath_MarcaControlYFacturaComoPagados()
    {
        var (control, factura) = CrearControlYFactura();
        var controles = new FakeControlAdicionalRepository();
        var facturas = new FakeFacturaRepository();
        controles.Agregar(control);
        facturas.Agregar(factura);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegistrarPagoControlCommandHandler(controles, facturas, unitOfWork);

        await handler.Handle(new RegistrarPagoControlCommand(control.Id), CancellationToken.None);

        Assert.True(control.Pagado);
        Assert.Equal(EstadoFactura.PAGADA, factura.Estado);
        Assert.Equal(1, unitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_ControlInexistente_FallaConControlNoEncontrado()
    {
        var handler = new RegistrarPagoControlCommandHandler(
            new FakeControlAdicionalRepository(), new FakeFacturaRepository(), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new RegistrarPagoControlCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("CONTROL_NO_ENCONTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.NotFound, ex.Error.Type);
    }
}
