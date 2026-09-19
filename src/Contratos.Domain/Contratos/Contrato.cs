using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Contratos;

public sealed class Contrato : AggregateRoot
{
    public ContratoId ContratoId { get; }
    public PacienteId PacienteId { get; }
    public Servicio Servicio { get; private set; }
    public DateOnly FechaContratacion { get; }
    public Vigencia Vigencia { get; private set; }
    public EstadoContrato Estado { get; private set; }
    public FacturaId FacturaId { get; }
    public Pago? Pago { get; private set; }

    private Contrato()
    {
        ContratoId = null!;
        PacienteId = null!;
        Servicio = null!;
        Vigencia = null!;
        FacturaId = null!;
    } // EF Core

    private Contrato(ContratoId contratoId, PacienteId pacienteId, Servicio servicio, Vigencia vigencia, FacturaId facturaId)
        : base(contratoId.Value)
    {
        ContratoId = contratoId;
        PacienteId = pacienteId;
        Servicio = servicio;
        FechaContratacion = DateOnly.FromDateTime(DateTime.UtcNow);
        Vigencia = vigencia;
        FacturaId = facturaId;
        Estado = EstadoContrato.ACTIVO;
    }

    public static Contrato Contratar(
        ContratoId contratoId,
        PacienteId pacienteId,
        Servicio servicio,
        Vigencia vigencia,
        FacturaId facturaId)
    {
        var contrato = new Contrato(contratoId, pacienteId, servicio.Clonar(), vigencia.Clonar(), facturaId);

        contrato.AddDomainEvent(new ServicioContratado(
            contrato.ContratoId,
            contrato.PacienteId,
            contrato.Servicio.Tipo,
            contrato.Vigencia.DuracionDias,
            contrato.Vigencia.FechaInicio));

        return contrato;
    }

    public void RegistrarPago(Pago pago)
    {
        // C14 (RN-20): solo se registra un pago sobre un contrato ACTIVO.
        if (Estado != EstadoContrato.ACTIVO)
            throw new DomainException(ContratoErrors.EstadoInvalido());

        // C13 (RN-18): el pago es único; el monto exacto lo valida Application (§6.1).
        if (Pago is not null)
            throw new DomainException(ContratoErrors.PagoYaRegistrado());

        Pago = pago.Clonar();
        AddDomainEvent(new PagoRegistrado(ContratoId, Pago));
    }

    public void Cancelar()
    {
        // C14 (RN-20): CANCELADO es terminal, solo se llega desde ACTIVO.
        if (Estado != EstadoContrato.ACTIVO)
            throw new DomainException(ContratoErrors.NoCancelable());

        Estado = EstadoContrato.CANCELADO;
        AddDomainEvent(new ContratoCancelado(ContratoId, FacturaId));
    }

    public void Completar(DateOnly hoy)
    {
        // C12 (RN-20): cierre perezoso, solo si estaba ACTIVO y la vigencia ya terminó.
        if (Estado != EstadoContrato.ACTIVO || hoy <= Vigencia.FechaFin)
            throw new DomainException(ContratoErrors.NoCompletable());

        Estado = EstadoContrato.COMPLETADO;
        AddDomainEvent(new ContratoCompletado(ContratoId, PacienteId, Servicio.Tipo, Vigencia.FechaFin));
    }

    public EstadoContrato EstadoDerivado(DateOnly hoy)
    {
        if (Estado == EstadoContrato.CANCELADO)
            return EstadoContrato.CANCELADO;

        return hoy > Vigencia.FechaFin ? EstadoContrato.COMPLETADO : EstadoContrato.ACTIVO;
    }

    public bool EstaVigente(DateOnly hoy) => Estado == EstadoContrato.ACTIVO && Vigencia.EstaVigenteEn(hoy);

    public bool EsCatering() => Servicio.Tipo == TipoServicio.CATERING;
}
