using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Contratos.CancelarContrato;

public sealed class CancelarContratoCommandHandler(
    IContratoRepository contratoRepository,
    IFacturaRepository facturaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelarContratoCommand>
{
    public async Task Handle(CancelarContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await contratoRepository.GetByIdAsync(request.ContratoId)
            ?? throw new DomainException(ContratosErrors.ContratoNoEncontrado());

        var factura = await facturaRepository.GetByIdAsync(contrato.FacturaId.Value)
            ?? throw new DomainException(FacturasErrors.FacturaNoEncontrada());

        contrato.Cancelar();

        // C8: la factura se anula solo si sigue EMITIDA. Una factura PAGADA
        // permanece PAGADA — no es un error, es una rama condicional.
        if (factura.Estado == EstadoFactura.EMITIDA)
            factura.Anular();

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
