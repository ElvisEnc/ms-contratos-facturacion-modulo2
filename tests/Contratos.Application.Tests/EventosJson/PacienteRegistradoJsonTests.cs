using System.Text.Json;
using Contratos.Application.IntegrationEvents.Entrantes;

namespace Contratos.Application.Tests.EventosJson;

public class PacienteRegistradoJsonTests
{
    // SISTEMA.md §9.1: pacienteId, nombre, nroIdentificacion y contacto (objeto
    // anidado, no aplanado), aunque BC3 descarte el valor de contacto.
    [Fact]
    public void Deserializa_el_contacto_anidado()
    {
        var pacienteId = Guid.NewGuid();
        var json = $$"""
        {
          "pacienteId": "{{pacienteId}}",
          "nombre": "Ana Pérez",
          "nroIdentificacion": "1234567",
          "contacto": {
            "telefono": "70000000",
            "email": "ana@example.com",
            "direccion": "Av. Siempre Viva 123"
          }
        }
        """;

        var evento = JsonSerializer.Deserialize<PacienteRegistradoIntegrationEvent>(json, JsonTestOptions.Options);

        Assert.NotNull(evento);
        Assert.Equal(pacienteId, evento!.PacienteId);
        Assert.Equal("Ana Pérez", evento.Nombre);
        Assert.Equal("1234567", evento.NroIdentificacion);
        Assert.NotNull(evento.Contacto);
        Assert.Equal("70000000", evento.Contacto.Telefono);
        Assert.Equal("ana@example.com", evento.Contacto.Email);
        Assert.Equal("Av. Siempre Viva 123", evento.Contacto.Direccion);
    }
}
