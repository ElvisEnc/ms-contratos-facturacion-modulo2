using Contratos.Application.Catalogo.ActualizarPrecio;
using Contratos.Application.Catalogo.CrearServicioCatalogo;
using Contratos.Application.Catalogo.DesactivarServicio;
using Contratos.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contratos.WebApi.Controllers;

[ApiController]
[Route("api/servicios")]
public sealed class ServiciosController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearServicioCatalogoCommand command, CancellationToken ct)
    {
        var servicioId = await sender.Send(command, ct);
        return Ok(servicioId);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool? activos, CancellationToken ct)
    {
        var servicios = await sender.Send(new GetServiciosQuery(activos), ct);
        return Ok(servicios);
    }

    [HttpPut("{id}/precio")]
    public async Task<IActionResult> ActualizarPrecio(Guid id, [FromBody] ActualizarPrecioBody body, CancellationToken ct)
    {
        await sender.Send(new ActualizarPrecioCommand(id, body.Precio, body.Moneda), ct);
        return Ok();
    }

    // Desactiva, no borra — es idempotente (§11.1).
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        await sender.Send(new DesactivarServicioCommand(id), ct);
        return Ok();
    }
}

public sealed record ActualizarPrecioBody(decimal Precio, string Moneda);
