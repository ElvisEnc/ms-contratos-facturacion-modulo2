namespace Contratos.Application.IntegrationEvents.Entrantes;

// BC2 -> BC3 (SISTEMA.md §9.6). `fechaInicio` y `duracionDias` se guardan
// (RN-01b); `detalle` se deserializa completo, con sus tres niveles, y se
// descarta — tipado y no como JsonElement opaco (DESIGN.md §9.1).
public sealed record PlanAlimenticioAsignadoIntegrationEvent(
    Guid PlanId,
    Guid PacienteId,
    DateOnly FechaInicio,
    int DuracionDias,
    IReadOnlyList<DiaDetalle> Detalle);

// `dia` es una fecha absoluta, no un número relativo al inicio del plan.
public sealed record DiaDetalle(
    DateOnly Dia,
    IReadOnlyList<TiempoDetalle> Tiempos);

public sealed record TiempoDetalle(
    TipoComida Tipo,
    int Orden,
    IReadOnlyList<RecetaDetalle> Recetas);

public sealed record RecetaDetalle(
    Guid RecetaId,
    string NombreReceta,
    decimal Cantidad,
    string Unidad);

// Vocabulario de BC2 (SISTEMA.md §8); se declara aquí, no en Domain, porque
// BC3 no modela nada más de ese vocabulario: el detalle se descarta completo.
public enum TipoComida
{
    DESAYUNO,
    MEDIA_MANANA,
    ALMUERZO,
    MERIENDA,
    CENA,
    EXTRA
}
