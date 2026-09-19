using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Controles;

public sealed class ControlAdicional : AggregateRoot
{
    public ControlId ControlId { get; }
    public PacienteId PacienteId { get; }
    public EvaluacionId EvaluacionId { get; }
    public DateOnly Fecha { get; }
    public Money Cobro { get; private set; }
    public FacturaId FacturaId { get; }
    public bool Pagado { get; private set; }

    private ControlAdicional()
    {
        ControlId = null!;
        PacienteId = null!;
        EvaluacionId = null!;
        Cobro = null!;
        FacturaId = null!;
    } // EF Core

    private ControlAdicional(
        ControlId controlId,
        PacienteId pacienteId,
        EvaluacionId evaluacionId,
        DateOnly fecha,
        Money cobro,
        FacturaId facturaId)
        : base(controlId.Value)
    {
        ControlId = controlId;
        PacienteId = pacienteId;
        EvaluacionId = evaluacionId;
        Fecha = fecha;
        Cobro = cobro;
        FacturaId = facturaId;
        Pagado = false;
    }

    public static ControlAdicional Generar(
        ControlId controlId,
        PacienteId pacienteId,
        EvaluacionId evaluacionId,
        DateOnly fecha,
        Money cobro,
        FacturaId facturaId)
    {
        // C21 (RN-06): el control nace con un cobro positivo.
        if (cobro.Monto <= 0)
            throw new DomainException(ControlAdicionalErrors.CobroInvalido());

        var control = new ControlAdicional(controlId, pacienteId, evaluacionId, fecha, cobro.Clonar(), facturaId);

        control.AddDomainEvent(new ControlAdicionalGenerado(
            control.ControlId,
            control.PacienteId,
            control.EvaluacionId,
            control.Cobro));

        return control;
    }

    public void MarcarPagado()
    {
        // C21 (RN-18): se paga una sola vez.
        if (Pagado)
            throw new DomainException(ControlAdicionalErrors.YaPagado());

        Pagado = true;
    }
}
