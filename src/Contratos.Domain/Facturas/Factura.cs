using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Facturas;

public sealed class Factura : AggregateRoot
{
    private readonly List<LineaFactura> _lineas = [];

    public FacturaId FacturaId { get; }
    public PacienteId PacienteId { get; private set; }
    public OrigenFactura Origen { get; private set; }
    public Guid ReferenciaId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public IReadOnlyCollection<LineaFactura> Lineas => _lineas.AsReadOnly();
    public Money Subtotal { get; private set; }
    public Money Impuesto { get; private set; }
    public Money Total { get; private set; }
    public EstadoFactura Estado { get; private set; }

    private Factura()
    {
        FacturaId = null!;
        PacienteId = null!;
        Subtotal = null!;
        Impuesto = null!;
        Total = null!;
    } // EF Core

    private Factura(
        FacturaId facturaId,
        PacienteId pacienteId,
        OrigenFactura origen,
        Guid referenciaId,
        IEnumerable<LineaFactura> lineas,
        Money subtotal,
        Money impuesto,
        Money total)
        : base(facturaId.Value)
    {
        FacturaId = facturaId;
        PacienteId = pacienteId;
        Origen = origen;
        ReferenciaId = referenciaId;
        Fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        _lineas.AddRange(lineas);
        Subtotal = subtotal;
        Impuesto = impuesto;
        Total = total;
        Estado = EstadoFactura.EMITIDA;
    }

    public static Factura Emitir(
        PacienteId pacienteId,
        OrigenFactura origen,
        Guid referenciaId,
        IReadOnlyCollection<DetalleLinea> detalles,
        decimal pctImpuesto)
    {
        // C11 (RN-25): la factura exige al menos una línea.
        if (detalles.Count == 0)
            throw new DomainException(FacturaErrors.SinLineas());

        // C11 (RN-25): el porcentaje de impuesto está siempre en [0, 100].
        if (pctImpuesto < 0 || pctImpuesto > 100)
            throw new DomainException(FacturaErrors.PorcentajeInvalido());

        var lineas = detalles
            .Select(detalle => new LineaFactura(
                LineaFacturaId.New(),
                detalle.Concepto,
                detalle.Cantidad,
                detalle.PrecioUnitario.Clonar()))
            .ToList();

        // C11 (RN-25): los tres importes se calculan, nunca se reciben.
        var subtotal = lineas
            .Select(linea => linea.Subtotal)
            .Aggregate((acumulado, siguiente) => acumulado.Sumar(siguiente));

        var impuesto = subtotal.PorPorcentaje(pctImpuesto);
        var total = subtotal.Sumar(impuesto);

        var factura = new Factura(FacturaId.New(), pacienteId, origen, referenciaId, lineas, subtotal, impuesto, total);

        factura.AddDomainEvent(new FacturaEmitida(
            factura.FacturaId,
            factura.PacienteId,
            factura.Origen,
            factura.ReferenciaId,
            factura.Total,
            factura.Fecha));

        return factura;
    }

    public void MarcarPagada()
    {
        // C6 (RN-18): solo se paga una factura EMITIDA.
        if (Estado != EstadoFactura.EMITIDA)
            throw new DomainException(FacturaErrors.EstadoInvalido());

        Estado = EstadoFactura.PAGADA;
    }

    public void Anular()
    {
        // C6 (RN-18): solo se anula una factura EMITIDA; PAGADA es terminal.
        if (Estado != EstadoFactura.EMITIDA)
            throw new DomainException(FacturaErrors.EstadoInvalido());

        Estado = EstadoFactura.ANULADA;
    }
}
