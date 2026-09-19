using System.Text.Json;
using System.Text.Json.Serialization;

namespace Contratos.Application.Tests.EventosJson;

// SISTEMA.md §12.1: los enums viajan como string en mayúsculas, nunca como
// ordinal. En producción JsonStringEnumConverter se registra en Program.cs y
// en LoggingIntegrationEventPublisher; aquí se replica esa configuración
// para probar el contrato tal como viaja realmente.
internal static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
