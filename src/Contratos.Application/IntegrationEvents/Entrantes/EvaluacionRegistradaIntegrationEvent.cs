namespace Contratos.Application.IntegrationEvents.Entrantes;

// BC1 -> BC3 (SISTEMA.md §9.5). `Tipo` viaja como string, no como enum
// TipoEvaluacion: es vocabulario de BC1, y BC3 solo necesita responder
// "¿es de tipo CONTROL?" — DESIGN.md §5.5/§5.6, excepción documentada a la
// regla general de enums de SISTEMA.md §9.
public sealed record EvaluacionRegistradaIntegrationEvent(
    Guid PacienteId,
    Guid EvaluacionId,
    DateOnly Fecha,
    string Tipo);
