using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Facturas;

public class FacturaTests
{
    private static Factura CrearFacturaEmitida()
    {
        var detalle = new DetalleLinea("Evaluación nutricional", 1, Money.Crear(350m, "BOB"));
        return Factura.Emitir(
            PacienteId.From(Guid.NewGuid()),
            OrigenFactura.CONTRATACION,
            Guid.NewGuid(),
            [detalle],
            13m);
    }

    [Fact]
    public void MarcarPagada_SobreEmitida_PasaAPagada()
    {
        var factura = CrearFacturaEmitida();

        factura.MarcarPagada();

        Assert.Equal(EstadoFactura.PAGADA, factura.Estado);
    }

    [Fact]
    public void MarcarPagada_SobrePagada_FallaConEstadoFacturaInvalido()
    {
        var factura = CrearFacturaEmitida();
        factura.MarcarPagada();

        var ex = Assert.Throws<DomainException>(factura.MarcarPagada);

        Assert.Equal("C6_ESTADO_FACTURA_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void Anular_SobreEmitida_PasaAAnulada()
    {
        var factura = CrearFacturaEmitida();

        factura.Anular();

        Assert.Equal(EstadoFactura.ANULADA, factura.Estado);
    }

    [Fact]
    public void Anular_SobrePagada_FallaConEstadoFacturaInvalido()
    {
        var factura = CrearFacturaEmitida();
        factura.MarcarPagada();

        var ex = Assert.Throws<DomainException>(factura.Anular);

        Assert.Equal("C6_ESTADO_FACTURA_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public void Anular_SobreAnulada_Falla()
    {
        var factura = CrearFacturaEmitida();
        factura.Anular();

        var ex = Assert.Throws<DomainException>(factura.Anular);

        Assert.Equal("C6_ESTADO_FACTURA_INVALIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }
}
