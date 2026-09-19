using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Catalogo.DesactivarServicio;

// Desactivar() no valida el estado previo (§10.1: idempotente).
public sealed class DesactivarServicioCommandHandler(
    IServicioCatalogoRepository servicioCatalogoRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DesactivarServicioCommand>
{
    public async Task Handle(DesactivarServicioCommand request, CancellationToken cancellationToken)
    {
        var servicio = await servicioCatalogoRepository.GetByIdAsync(request.ServicioId)
            ?? throw new DomainException(CatalogoErrors.ServicioNoEncontrado());

        servicio.Desactivar();

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
