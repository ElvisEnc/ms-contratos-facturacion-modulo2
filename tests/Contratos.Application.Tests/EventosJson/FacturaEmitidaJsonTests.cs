using System.Text.Json;
using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.EventosJson;

public class FacturaEmitidaJsonTests
{
    // SISTEMA.md §9.3: siete campos planos; monto y moneda separados, monto = Total.
    [Fact]
    public void Serializa_monto_y_moneda_por_separado_desde_el_Total()
    {
        var evento = new FacturaEmitidaIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), OrigenFactura.CONTRATACION, Guid.NewGuid(), 113m, "BOB", new DateOnly(2026, 9, 7));

        var json = JsonSerializer.Serialize(evento, JsonTestOptions.Options);
        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        Assert.Equal(7, raiz.EnumerateObject().Count());
        Assert.Equal(evento.FacturaId, raiz.GetProperty("facturaId").GetGuid());
        Assert.Equal(evento.PacienteId, raiz.GetProperty("pacienteId").GetGuid());
        Assert.Equal("CONTRATACION", raiz.GetProperty("origen").GetString());
        Assert.Equal(evento.ReferenciaId, raiz.GetProperty("referenciaId").GetGuid());
        Assert.Equal(113m, raiz.GetProperty("monto").GetDecimal());
        Assert.Equal("BOB", raiz.GetProperty("moneda").GetString());
        Assert.Equal("2026-09-07", raiz.GetProperty("fecha").GetString());
    }
}
