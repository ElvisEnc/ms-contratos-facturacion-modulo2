using MediatR;

namespace Contratos.Application.Queries;

public sealed record GetControlesByPacienteQuery(Guid PacienteId) : IRequest<IReadOnlyCollection<ControlDto>>;

public sealed record ControlDto(
    Guid ControlId,
    Guid EvaluacionId,
    DateOnly Fecha,
    decimal Cobro,
    string Moneda,
    bool Pagado,
    Guid FacturaId);
