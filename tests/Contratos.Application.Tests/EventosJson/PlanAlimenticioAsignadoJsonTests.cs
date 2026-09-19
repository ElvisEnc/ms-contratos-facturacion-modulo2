using System.Text.Json;
using Contratos.Application.IntegrationEvents.Entrantes;

namespace Contratos.Application.Tests.EventosJson;

public class PlanAlimenticioAsignadoJsonTests
{
    // SISTEMA.md §9.6: planId, pacienteId, fechaInicio, duracionDias y detalle
    // completo de tres niveles (DiaDetalle -> TiempoDetalle -> RecetaDetalle),
    // con "dia" como fecha absoluta, no un número relativo.
    [Fact]
    public void Deserializa_el_detalle_completo_de_tres_niveles()
    {
        var planId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var recetaId = Guid.NewGuid();
        var json = $$"""
        {
          "planId": "{{planId}}",
          "pacienteId": "{{pacienteId}}",
          "fechaInicio": "2026-09-07",
          "duracionDias": 15,
          "detalle": [
            {
              "dia": "2026-09-07",
              "tiempos": [
                {
                  "tipo": "DESAYUNO",
                  "orden": 1,
                  "recetas": [
                    {
                      "recetaId": "{{recetaId}}",
                      "nombreReceta": "Batido de avena",
                      "cantidad": 1.5,
                      "unidad": "taza"
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;

        var evento = JsonSerializer.Deserialize<PlanAlimenticioAsignadoIntegrationEvent>(json, JsonTestOptions.Options);

        Assert.NotNull(evento);
        Assert.Equal(planId, evento!.PlanId);
        Assert.Equal(pacienteId, evento.PacienteId);
        Assert.Equal(new DateOnly(2026, 9, 7), evento.FechaInicio);
        Assert.Equal(15, evento.DuracionDias);

        var dia = Assert.Single(evento.Detalle);
        Assert.Equal(new DateOnly(2026, 9, 7), dia.Dia);

        var tiempo = Assert.Single(dia.Tiempos);
        Assert.Equal(TipoComida.DESAYUNO, tiempo.Tipo);
        Assert.Equal(1, tiempo.Orden);

        var receta = Assert.Single(tiempo.Recetas);
        Assert.Equal(recetaId, receta.RecetaId);
        Assert.Equal("Batido de avena", receta.NombreReceta);
        Assert.Equal(1.5m, receta.Cantidad);
        Assert.Equal("taza", receta.Unidad);
    }
}
