using System.Text.Json;
using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.EventosJson;

public class ServicioContratadoJsonTests
{
    // SISTEMA.md §9.2: siete campos. duracionDias es nulo salvo CATERING;
    // fechaInicio nunca es nulo, ni siquiera fuera de CATERING.
    [Fact]
    public void Serializa_los_siete_campos_con_duracionDias_nulo_fuera_de_catering()
    {
        var evento = new ServicioContratadoIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "Ana Pérez", "1234567",
            TipoServicio.EVALUACION, null, new DateOnly(2026, 9, 7));

        var json = JsonSerializer.Serialize(evento, JsonTestOptions.Options);
        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        Assert.Equal(7, raiz.EnumerateObject().Count());
        Assert.Equal(evento.ContratoId, raiz.GetProperty("contratoId").GetGuid());
        Assert.Equal(evento.PacienteId, raiz.GetProperty("pacienteId").GetGuid());
        Assert.Equal("Ana Pérez", raiz.GetProperty("pacienteNombre").GetString());
        Assert.Equal("1234567", raiz.GetProperty("nroIdentificacion").GetString());
        Assert.Equal("EVALUACION", raiz.GetProperty("tipoServicio").GetString());
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("duracionDias").ValueKind);
        Assert.Equal("2026-09-07", raiz.GetProperty("fechaInicio").GetString());
    }

    [Fact]
    public void Serializa_duracionDias_solo_con_catering()
    {
        var evento = new ServicioContratadoIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "Ana Pérez", "1234567",
            TipoServicio.CATERING, 15, new DateOnly(2026, 9, 7));

        var json = JsonSerializer.Serialize(evento, JsonTestOptions.Options);
        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        Assert.Equal("CATERING", raiz.GetProperty("tipoServicio").GetString());
        Assert.Equal(15, raiz.GetProperty("duracionDias").GetInt32());
        Assert.NotEqual(JsonValueKind.Null, raiz.GetProperty("fechaInicio").ValueKind);
    }
}
