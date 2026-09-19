using Contratos.Application.Ports;
using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Integracion.ProcesarPacienteRegistrado;

// Idempotente por pacienteId (§9.3 de DESIGN.md): reprocesar el mismo evento
// sobrescribe los mismos valores, no duplica la réplica. Solo muta los campos
// que este evento aporta; TienePlanActivo/PlanFechaInicio/PlanDuracionDias
// quedan intactos si ya los trajo PlanAlimenticioAsignado.
public sealed class ProcesarPacienteRegistradoCommandHandler(
    IPacienteConocidoStore pacienteConocidoStore) : IRequestHandler<ProcesarPacienteRegistradoCommand>
{
    public async Task Handle(ProcesarPacienteRegistradoCommand request, CancellationToken cancellationToken)
    {
        var pacienteId = PacienteId.From(request.PacienteId);

        var paciente = await pacienteConocidoStore.GetByIdAsync(pacienteId)
            ?? new PacienteConocido { PacienteId = pacienteId };

        paciente.Nombre = request.Nombre;
        paciente.NroIdentificacion = request.NroIdentificacion;

        await pacienteConocidoStore.UpsertAsync(paciente);
        await pacienteConocidoStore.SaveChangesAsync();
    }
}
