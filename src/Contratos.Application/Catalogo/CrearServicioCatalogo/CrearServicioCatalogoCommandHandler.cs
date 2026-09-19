using Contratos.Application.Ports;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using MediatR;

namespace Contratos.Application.Catalogo.CrearServicioCatalogo;

public sealed class CrearServicioCatalogoCommandHandler(
    IServicioCatalogoRepository servicioCatalogoRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CrearServicioCatalogoCommand, Guid>
{
    public async Task<Guid> Handle(CrearServicioCatalogoCommand request, CancellationToken cancellationToken)
    {
        var precio = Money.Crear(request.Precio, request.Moneda);
        var servicio = ServicioCatalogo.Crear(request.Tipo, request.Nombre, precio);

        await servicioCatalogoRepository.AddAsync(servicio);
        await unitOfWork.CommitAsync(cancellationToken);

        return servicio.ServicioId.Value;
    }
}
