using MediatR;

namespace Contratos.Application.Integracion.ProcesarPlanAsignado;

// Incluye PlanId (D-12, §9.15 del maestro): sin él la réplica no distingue
// "reprocesan el mismo plan" de "llegó un plan distinto". `detalle` se
// recibe y se descarta antes de llegar aquí.
public sealed record ProcesarPlanAsignadoCommand(
    Guid PacienteId,
    Guid PlanId,
    DateOnly FechaInicio,
    int DuracionDias) : IRequest;
