using Contratos.Application.Queries;
using Contratos.Domain.Shared;
using Contratos.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Queries;

public sealed class GetControlesByPacienteHandler(ContratosDbContext dbContext)
    : IRequestHandler<GetControlesByPacienteQuery, IReadOnlyCollection<ControlDto>>
{
    public async Task<IReadOnlyCollection<ControlDto>> Handle(GetControlesByPacienteQuery request, CancellationToken cancellationToken)
    {
        var pacienteId = PacienteId.From(request.PacienteId);

        var proyectados = await dbContext.ControlesAdicionales
            .AsNoTracking()
            .Where(c => c.PacienteId == pacienteId)
            .Select(c => new
            {
                c.ControlId,
                c.EvaluacionId,
                c.Fecha,
                CobroMonto = c.Cobro.Monto,
                CobroMoneda = c.Cobro.Moneda,
                c.Pagado,
                c.FacturaId
            })
            .ToListAsync(cancellationToken);

        return proyectados
            .Select(c => new ControlDto(
                c.ControlId.Value,
                c.EvaluacionId.Value,
                c.Fecha,
                c.CobroMonto,
                c.CobroMoneda,
                c.Pagado,
                c.FacturaId.Value))
            .ToList();
    }
}
