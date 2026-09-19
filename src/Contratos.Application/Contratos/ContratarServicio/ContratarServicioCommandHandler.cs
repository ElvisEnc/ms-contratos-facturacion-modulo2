using Contratos.Application.Configuration;
using Contratos.Application.Errors;
using Contratos.Application.Ports;
using Contratos.Domain.Contratos;
using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Contratos.Application.Contratos.ContratarServicio;

// Orden exacto (§12.1 de DESIGN.md): C1 -> C9 -> C2 -> C4 -> C12 -> C3 ->
// ObtenerServicioParaContrato -> emitir factura -> crear contrato -> un solo commit (C5).
public sealed class ContratarServicioCommandHandler(
    IPacienteConocidoStore pacienteConocidoStore,
    IServicioCatalogoRepository servicioCatalogoRepository,
    IContratoRepository contratoRepository,
    IFacturaRepository facturaRepository,
    FacturacionOptions facturacionOptions,
    IUnitOfWork unitOfWork) : IRequestHandler<ContratarServicioCommand, ContratarServicioResult>
{
    public async Task<ContratarServicioResult> Handle(ContratarServicioCommand request, CancellationToken cancellationToken)
    {
        var pacienteId = PacienteId.From(request.PacienteId);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        // C1: solo se contrata para un paciente conocido.
        var replica = await pacienteConocidoStore.GetByIdAsync(pacienteId)
            ?? throw new DomainException(ContratosErrors.PacienteDesconocido());

        // C9: no se contrata un servicio inactivo (ni ausente) en el catálogo.
        var servicioCatalogo = await servicioCatalogoRepository.GetByTipoAsync(request.TipoServicio)
            ?? throw new DomainException(CatalogoErrors.CatalogoMalConfigurado());

        if (!servicioCatalogo.Activo)
            throw new DomainException(CatalogoErrors.ServicioInactivo());

        Vigencia vigencia;
        if (request.TipoServicio == TipoServicio.CATERING)
        {
            // C2: contratar CATERING exige plan alimenticio activo replicado.
            if (!replica.TienePlanActivo)
                throw new DomainException(ContratosErrors.SinPlanActivo());

            // C4 (RN-01b): la vigencia se copia de la réplica del plan, nunca del
            // command; la FechaInicio que traiga el cliente se ignora en silencio.
            var duracionDias = replica.PlanDuracionDias;
            if (duracionDias is not (15 or 30))
                throw new DomainException(ContratosErrors.DuracionInvalida());

            vigencia = Vigencia.Desde(replica.PlanFechaInicio!.Value, duracionDias.Value);
        }
        else
        {
            // EVALUACION y ASESORAMIENTO duran un día; la fecha ausente significa hoy.
            var fechaInicio = request.FechaInicio ?? hoy;
            vigencia = Vigencia.Desde(fechaInicio, 1);
        }

        // C12: cierre perezoso de contratos vencidos, previo a aplicar C3.
        var vencidos = await contratoRepository.GetActivosVencidosDePacienteYTipoAsync(
            pacienteId, request.TipoServicio, hoy);
        foreach (var vencido in vencidos)
            vencido.Completar(hoy);

        // C3 (RN-19, INC-S10): la unicidad de "activo" solo aplica a CATERING,
        // comprobando además que la vigencia solape.
        if (request.TipoServicio == TipoServicio.CATERING
            && await contratoRepository.ExisteCateringActivoParaPacienteAsync(pacienteId, vigencia.FechaInicio))
        {
            throw new DomainException(ContratosErrors.CateringYaActivo());
        }

        // C15/RN-26: el precio se copia del catálogo, clonado, y queda congelado.
        var servicio = servicioCatalogo.ObtenerServicioParaContrato();

        // La factura necesita el ContratoId como referencia antes de que el
        // contrato exista: se genera aquí y se reutiliza en Contrato.Contratar.
        var contratoId = ContratoId.New();
        var detalle = new DetalleLinea(servicio.Nombre, 1, servicio.Precio);

        var factura = Factura.Emitir(
            pacienteId,
            OrigenFactura.CONTRATACION,
            contratoId.Value,
            [detalle],
            facturacionOptions.PorcentajeImpuesto);

        var contrato = Contrato.Contratar(contratoId, pacienteId, servicio, vigencia, factura.FacturaId);

        await facturaRepository.AddAsync(factura);
        await contratoRepository.AddAsync(contrato);

        // C5: un solo commit — publica ServicioContratado y FacturaEmitida
        // (vía las políticas de §8.2) antes de SaveChangesAsync.
        await unitOfWork.CommitAsync(cancellationToken);

        return new ContratarServicioResult(contrato.ContratoId.Value, factura.FacturaId.Value);
    }
}
