using Contratos.Application.Queries;
using Contratos.Domain.Shared;
using Contratos.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Queries;

public sealed class GetFacturasByPacienteHandler(ContratosDbContext dbContext)
    : IRequestHandler<GetFacturasByPacienteQuery, IReadOnlyCollection<FacturaDto>>
{
    public async Task<IReadOnlyCollection<FacturaDto>> Handle(GetFacturasByPacienteQuery request, CancellationToken cancellationToken)
    {
        var pacienteId = PacienteId.From(request.PacienteId);

        // §12.5 del maestro: se proyecta el id tipado completo, no ".Value",
        // dentro de la expresión traducida a SQL; el Guid se extrae después.
        var proyectados = await dbContext.Facturas
            .AsNoTracking()
            .Where(f => f.PacienteId == pacienteId)
            .Select(f => new
            {
                f.FacturaId,
                f.Origen,
                f.ReferenciaId,
                f.Fecha,
                SubtotalMonto = f.Subtotal.Monto,
                ImpuestoMonto = f.Impuesto.Monto,
                TotalMonto = f.Total.Monto,
                TotalMoneda = f.Total.Moneda,
                f.Estado
            })
            .ToListAsync(cancellationToken);

        return proyectados
            .Select(f => new FacturaDto(
                f.FacturaId.Value,
                f.Origen,
                f.ReferenciaId,
                f.Fecha,
                f.SubtotalMonto,
                f.ImpuestoMonto,
                f.TotalMonto,
                f.TotalMoneda,
                f.Estado))
            .ToList();
    }
}
