using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Controles.RegistrarPagoControl;

public sealed class RegistrarPagoControlCommandHandler(
    IControlAdicionalRepository controlAdicionalRepository,
    IFacturaRepository facturaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegistrarPagoControlCommand>
{
    public async Task Handle(RegistrarPagoControlCommand request, CancellationToken cancellationToken)
    {
        var control = await controlAdicionalRepository.GetByIdAsync(request.ControlId)
            ?? throw new DomainException(ControlesErrors.ControlNoEncontrado());

        var factura = await facturaRepository.GetByIdAsync(control.FacturaId.Value)
            ?? throw new DomainException(FacturasErrors.FacturaNoEncontrada());

        control.MarcarPagado();
        factura.MarcarPagada();

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
