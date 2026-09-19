using Contratos.Application.Queries;
using Contratos.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Queries;

public sealed class GetContratosHandler(ContratosDbContext dbContext)
    : IRequestHandler<GetContratosQuery, IReadOnlyCollection<ContratoResumenDto>>
{
    public async Task<IReadOnlyCollection<ContratoResumenDto>> Handle(GetContratosQuery request, CancellationToken cancellationToken)
    {
        // El período se filtra en SQL por fecha_contratacion; el estado se
        // resuelve EN MEMORIA con EstadoDerivado(hoy) — la columna puede decir
        // ACTIVO en un contrato cuya vigencia ya pasó y no se cerró todavía
        // (§5.7, §10.2 del DESIGN).
        var contratos = await dbContext.Contratos
            .AsNoTracking()
            .Where(c => c.FechaContratacion >= request.Desde && c.FechaContratacion <= request.Hasta)
            .ToListAsync(cancellationToken);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        return contratos
            .Select(c => new { Contrato = c, Estado = c.EstadoDerivado(hoy) })
            .Where(x => request.Estado is null || x.Estado == request.Estado)
            .Select(x => new ContratoResumenDto(
                x.Contrato.ContratoId.Value,
                x.Contrato.PacienteId.Value,
                x.Contrato.Servicio.Tipo,
                x.Contrato.FechaContratacion,
                x.Contrato.Vigencia.FechaInicio,
                x.Contrato.Vigencia.FechaFin,
                x.Estado))
            .ToList();
    }
}
