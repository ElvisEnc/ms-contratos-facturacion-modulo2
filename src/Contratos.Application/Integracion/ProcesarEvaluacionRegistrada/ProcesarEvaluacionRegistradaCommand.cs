using MediatR;

namespace Contratos.Application.Integracion.ProcesarEvaluacionRegistrada;

// Tipo viaja como string, no como enum (§5.5/§5.6 de DESIGN.md): TipoEvaluacion
// es vocabulario de BC1 que BC3 no modela a propósito. La comparación contra
// "CONTROL" se hace sin distinguir mayúsculas, dentro del handler.
public sealed record ProcesarEvaluacionRegistradaCommand(
    Guid PacienteId,
    Guid EvaluacionId,
    DateOnly Fecha,
    string Tipo) : IRequest;
