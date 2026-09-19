using Contratos.Application.Contratos.CancelarContrato;
using Contratos.Application.Contratos.ContratarServicio;
using Contratos.Application.Contratos.RegistrarPago;
using Contratos.Application.Queries;
using Contratos.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contratos.WebApi.Controllers;

[ApiController]
[Route("api/contratos")]
public sealed class ContratosController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Contratar([FromBody] ContratarServicioCommand command, CancellationToken ct)
    {
        var resultado = await sender.Send(command, ct);
        return Ok(resultado);
    }

    [HttpPost("{id}/pago")]
    public async Task<IActionResult> RegistrarPago(Guid id, [FromBody] RegistrarPagoBody body, CancellationToken ct)
    {
        await sender.Send(new RegistrarPagoCommand(id, body.Monto, body.Moneda, body.Referencia), ct);
        return Ok();
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        await sender.Send(new CancelarContratoCommand(id), ct);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] EstadoContrato? estado, CancellationToken ct)
    {
        var contratos = await sender.Send(new GetContratosQuery(desde, hasta, estado), ct);
        return Ok(contratos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var contrato = await sender.Send(new GetContratoByIdQuery(id), ct);
        return Ok(contrato);
    }
}

public sealed record RegistrarPagoBody(decimal Monto, string Moneda, string? Referencia);
