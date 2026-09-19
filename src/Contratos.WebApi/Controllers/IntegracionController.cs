using Contratos.Application.IntegrationEvents.Entrantes;
using Contratos.Application.Integracion.ProcesarEvaluacionRegistrada;
using Contratos.Application.Integracion.ProcesarPacienteRegistrado;
using Contratos.Application.Integracion.ProcesarPlanAsignado;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contratos.WebApi.Controllers;

// §11.4: los tres endpoints que §10 del maestro asigna a este microservicio.
// El cuerpo enlaza directo contra el record del contrato de integración
// (§9 del maestro) para que un cambio de contrato rompa la compilación en
// vez de fallar en ejecución; los campos que Application no necesita
// (Contacto, Detalle) se descartan aquí.
[ApiController]
[Route("api/integracion")]
public sealed class IntegracionController(ISender sender) : ControllerBase
{
    [HttpPost("paciente-registrado")]
    public async Task<IActionResult> PacienteRegistrado([FromBody] PacienteRegistradoIntegrationEvent evento, CancellationToken ct)
    {
        await sender.Send(new ProcesarPacienteRegistradoCommand(evento.PacienteId, evento.Nombre, evento.NroIdentificacion), ct);
        return Ok();
    }

    [HttpPost("plan-asignado")]
    public async Task<IActionResult> PlanAsignado([FromBody] PlanAlimenticioAsignadoIntegrationEvent evento, CancellationToken ct)
    {
        await sender.Send(new ProcesarPlanAsignadoCommand(evento.PacienteId, evento.PlanId, evento.FechaInicio, evento.DuracionDias), ct);
        return Ok();
    }

    [HttpPost("evaluacion-registrada")]
    public async Task<IActionResult> EvaluacionRegistrada([FromBody] EvaluacionRegistradaIntegrationEvent evento, CancellationToken ct)
    {
        await sender.Send(new ProcesarEvaluacionRegistradaCommand(evento.PacienteId, evento.EvaluacionId, evento.Fecha, evento.Tipo), ct);
        return Ok();
    }
}
