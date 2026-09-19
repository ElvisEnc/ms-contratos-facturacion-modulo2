using Contratos.Application.EventHandlers;
using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.EventHandlers;

public class PublicarFacturaEmitidaHandlerTests
{
    [Fact]
    public async Task Handle_MapeaMontoYMonedaDesdeElTotal()
    {
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarFacturaEmitidaHandler(publisher);

        // Subtotal 100 + impuesto 13% = Total 113: si el handler publicara el
        // subtotal en vez del Total, este test lo detectaría (113 != 100).
        var factura = Factura.Emitir(
            PacienteId.From(Guid.NewGuid()),
            OrigenFactura.CONTRATACION,
            Guid.NewGuid(),
            [new DetalleLinea("Servicio de prueba", 1, Money.Crear(100m, "BOB"))],
            13m);
        var evento = Assert.Single(factura.DomainEvents.OfType<FacturaEmitida>());

        await handler.Handle(evento, CancellationToken.None);

        var publicado = Assert.IsType<FacturaEmitidaIntegrationEvent>(Assert.Single(publisher.Publicados));
        Assert.Equal(113m, publicado.Monto);
        Assert.Equal("BOB", publicado.Moneda);
        Assert.NotEqual(factura.Subtotal.Monto, publicado.Monto);
    }
}
