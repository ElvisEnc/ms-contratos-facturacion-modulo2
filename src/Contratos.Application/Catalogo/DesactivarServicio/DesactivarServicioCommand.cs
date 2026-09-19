using MediatR;

namespace Contratos.Application.Catalogo.DesactivarServicio;

public sealed record DesactivarServicioCommand(Guid ServicioId) : IRequest;
