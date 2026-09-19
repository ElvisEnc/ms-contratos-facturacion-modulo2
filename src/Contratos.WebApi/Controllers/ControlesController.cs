using Contratos.Application.Controles.RegistrarPagoControl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contratos.WebApi.Controllers;

[ApiController]
[Route("api/controles")]
public sealed class ControlesController(ISender sender) : ControllerBase
{
    // Sin cuerpo: el importe ya está calculado en la factura del control (§10.1).
    [HttpPost("{id}/pago")]
    public async Task<IActionResult> RegistrarPago(Guid id, CancellationToken ct)
    {
        await sender.Send(new RegistrarPagoControlCommand(id), ct);
        return Ok();
    }
}
