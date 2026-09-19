using Contratos.Application.Configuration;
using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Contratos.Domain.Controles;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Integracion.ProcesarEvaluacionRegistrada;

// Orden exacto (§12.2 de DESIGN.md): C10 primero de todo -> tipo -> vigencia
// del catering -> catálogo. C7 y C10 no producen error: "no hacer nada"
// responde 200.
public sealed class ProcesarEvaluacionRegistradaCommandHandler(
    IControlAdicionalRepository controlAdicionalRepository,
    IContratoRepository contratoRepository,
    IServicioCatalogoRepository servicioCatalogoRepository,
    IFacturaRepository facturaRepository,
    FacturacionOptions facturacionOptions,
    IUnitOfWork unitOfWork) : IRequestHandler<ProcesarEvaluacionRegistradaCommand>
{
    public async Task Handle(ProcesarEvaluacionRegistradaCommand request, CancellationToken cancellationToken)
    {
        var evaluacionId = EvaluacionId.From(request.EvaluacionId);

        // C10: si el duplicado se detectara al final, el reproceso dependería
        // de que la vigencia no hubiera cambiado entre las dos entregas.
        if (await controlAdicionalRepository.ExistePorEvaluacionAsync(evaluacionId))
            return;

        // C7, primera condición: solo CONTROL cobra. Comparar con == contra el
        // literal haría que "Control" no generara cobro (desviación D-07).
        if (!string.Equals(request.Tipo, "CONTROL", StringComparison.OrdinalIgnoreCase))
            return;

        var pacienteId = PacienteId.From(request.PacienteId);

        // C7, segunda condición (RN-06): lectura binaria, no un contador.
        if (await contratoRepository.TieneCateringVigenteEnAsync(pacienteId, request.Fecha))
            return;

        var servicioCatalogo = await servicioCatalogoRepository.GetByTipoAsync(TipoServicio.EVALUACION)
            ?? throw new DomainException(CatalogoErrors.CatalogoMalConfigurado());

        var cobro = servicioCatalogo.PrecioVigente.Clonar();
        var controlId = ControlId.New();
        var detalle = new DetalleLinea(servicioCatalogo.Nombre, 1, cobro);

        var factura = Factura.Emitir(
            pacienteId,
            OrigenFactura.CONTROL_ADICIONAL,
            controlId.Value,
            [detalle],
            facturacionOptions.PorcentajeImpuesto);

        var control = ControlAdicional.Generar(controlId, pacienteId, evaluacionId, request.Fecha, cobro, factura.FacturaId);

        await facturaRepository.AddAsync(factura);
        await controlAdicionalRepository.AddAsync(control);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
