using MediatR;

namespace Contratos.Application.Contratos.CancelarContrato;

public sealed record CancelarContratoCommand(Guid ContratoId) : IRequest;
