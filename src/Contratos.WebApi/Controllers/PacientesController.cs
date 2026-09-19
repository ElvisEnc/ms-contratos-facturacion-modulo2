using Contratos.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contratos.WebApi.Controllers;

[ApiController]
[Route("api/pacientes")]
public sealed class PacientesController(ISender sender) : ControllerBase
{
    // Lista vacía y 200, nunca 404: la ausencia de facturas es una respuesta
    // legítima sobre un paciente que existe (§11.3).
    [HttpGet("{pacienteId}/facturas")]
    public async Task<IActionResult> ListarFacturas(Guid pacienteId, CancellationToken ct)
    {
        var facturas = await sender.Send(new GetFacturasByPacienteQuery(pacienteId), ct);
        return Ok(facturas);
    }

    [HttpGet("{pacienteId}/controles")]
    public async Task<IActionResult> ListarControles(Guid pacienteId, CancellationToken ct)
    {
        var controles = await sender.Send(new GetControlesByPacienteQuery(pacienteId), ct);
        return Ok(controles);
    }
}
