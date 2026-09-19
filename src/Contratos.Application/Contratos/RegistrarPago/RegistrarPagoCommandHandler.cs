using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Contratos.RegistrarPago;

public sealed class RegistrarPagoCommandHandler(
    IContratoRepository contratoRepository,
    IFacturaRepository facturaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegistrarPagoCommand>
{
    public async Task Handle(RegistrarPagoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await contratoRepository.GetByIdAsync(request.ContratoId)
            ?? throw new DomainException(ContratosErrors.ContratoNoEncontrado());

        var factura = await facturaRepository.GetByIdAsync(contrato.FacturaId.Value)
            ?? throw new DomainException(FacturasErrors.FacturaNoEncontrada());

        var monto = Money.Crear(request.Monto, request.Moneda);

        // C13: monto y moneda deben coincidir EXACTAMENTE con el total de la
        // factura, comprobado ANTES de MarcarPagada. Sin esta comparación se
        // podría registrar un pago de 1 BOB contra una factura de 5085 BOB y
        // la factura quedaría PAGADA (desviación D-02, §19 de DESIGN.md).
        if (monto != factura.Total)
            throw new DomainException(ContratosErrors.PagoNoCoincideConTotal());

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        contrato.RegistrarPago(Pago.Crear(hoy, monto, request.Referencia));
        factura.MarcarPagada();

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
