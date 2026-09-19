using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Catalogo.ActualizarPrecio;

public sealed class ActualizarPrecioCommandHandler(
    IServicioCatalogoRepository servicioCatalogoRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActualizarPrecioCommand>
{
    public async Task Handle(ActualizarPrecioCommand request, CancellationToken cancellationToken)
    {
        var servicio = await servicioCatalogoRepository.GetByIdAsync(request.ServicioId)
            ?? throw new DomainException(CatalogoErrors.ServicioNoEncontrado());

        servicio.ActualizarPrecio(Money.Crear(request.Precio, request.Moneda));

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
