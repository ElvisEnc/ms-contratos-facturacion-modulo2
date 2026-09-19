using System.Text.Json;
using Contratos.Application.IntegrationEvents.Entrantes;

namespace Contratos.Application.Tests.EventosJson;

public class EvaluacionRegistradaJsonTests
{
    // SISTEMA.md §9.5: pacienteId, evaluacionId, fecha y tipo. `tipo` viaja
    // como string en mayúsculas (TipoEvaluacion es vocabulario de BC1 que BC3
    // no modela como enum — DESIGN.md §5.5/§5.6).
    [Fact]
    public void Deserializa_los_cuatro_campos_con_tipo_como_string()
    {
        var pacienteId = Guid.NewGuid();
        var evaluacionId = Guid.NewGuid();
        var json = $$"""
        {
          "pacienteId": "{{pacienteId}}",
          "evaluacionId": "{{evaluacionId}}",
          "fecha": "2026-09-07",
          "tipo": "CONTROL"
        }
        """;

        var evento = JsonSerializer.Deserialize<EvaluacionRegistradaIntegrationEvent>(json, JsonTestOptions.Options);

        Assert.NotNull(evento);
        Assert.Equal(pacienteId, evento!.PacienteId);
        Assert.Equal(evaluacionId, evento.EvaluacionId);
        Assert.Equal(new DateOnly(2026, 9, 7), evento.Fecha);
        Assert.IsType<string>(evento.Tipo);
        Assert.Equal("CONTROL", evento.Tipo);
    }
}
