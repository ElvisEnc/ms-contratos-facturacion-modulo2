using Contratos.Application.Ports;
using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Integracion.ProcesarPlanAsignado;

// Idempotente por pacienteId + planId (§9.3 de DESIGN.md). Puede llegar antes
// que ProcesarPacienteRegistrado: la réplica se crea con Nombre/NroIdentificacion
// vacíos, y los dos eventos son conmutativos porque tocan campos disjuntos.
public sealed class ProcesarPlanAsignadoCommandHandler(
    IPacienteConocidoStore pacienteConocidoStore) : IRequestHandler<ProcesarPlanAsignadoCommand>
{
    public async Task Handle(ProcesarPlanAsignadoCommand request, CancellationToken cancellationToken)
    {
        var pacienteId = PacienteId.From(request.PacienteId);

        var paciente = await pacienteConocidoStore.GetByIdAsync(pacienteId)
            ?? new PacienteConocido { PacienteId = pacienteId };

        paciente.PlanId = request.PlanId;
        paciente.TienePlanActivo = true;
        paciente.PlanFechaInicio = request.FechaInicio;
        paciente.PlanDuracionDias = request.DuracionDias;

        await pacienteConocidoStore.UpsertAsync(paciente);
        await pacienteConocidoStore.SaveChangesAsync();
    }
}
