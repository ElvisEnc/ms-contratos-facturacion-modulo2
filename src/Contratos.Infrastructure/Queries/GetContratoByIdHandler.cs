using Contratos.Application.Errors;
using Contratos.Application.Queries;
using Contratos.Domain.Shared;
using Contratos.Infrastructure.Persistence;
using Joseco.DDD.Core.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Queries;

public sealed class GetContratoByIdHandler(ContratosDbContext dbContext)
    : IRequestHandler<GetContratoByIdQuery, ContratoDetalleDto>
{
    public async Task<ContratoDetalleDto> Handle(GetContratoByIdQuery request, CancellationToken cancellationToken)
    {
        var contratoId = ContratoId.From(request.ContratoId);

        var contrato = await dbContext.Contratos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContratoId == contratoId, cancellationToken)
            ?? throw new DomainException(ContratosErrors.ContratoNoEncontrado());

        // C5: el contrato y su factura se escriben en el mismo commit — nunca
        // existe el uno sin el otro, aunque no haya FK entre agregados. Si
        // faltara, es un fallo interno (§13.2), no un 500 sin cuerpo.
        var factura = await dbContext.Facturas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FacturaId == contrato.FacturaId, cancellationToken)
            ?? throw new DomainException(FacturasErrors.FacturaNoEncontrada());

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var pago = contrato.Pago is null
            ? null
            : new PagoDto(contrato.Pago.Fecha, contrato.Pago.Monto.Monto, contrato.Pago.Monto.Moneda, contrato.Pago.Referencia);

        return new ContratoDetalleDto(
            contrato.ContratoId.Value,
            contrato.PacienteId.Value,
            new ServicioContratadoDto(contrato.Servicio.Tipo, contrato.Servicio.Nombre, contrato.Servicio.Precio.Monto, contrato.Servicio.Precio.Moneda),
            new VigenciaDto(contrato.Vigencia.FechaInicio, contrato.Vigencia.FechaFin, contrato.Vigencia.DuracionDias),
            contrato.EstadoDerivado(hoy),
            new FacturaResumenDto(factura.FacturaId.Value, factura.Estado, factura.Total.Monto, factura.Total.Moneda),
            pago);
    }
}
