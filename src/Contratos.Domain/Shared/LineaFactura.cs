using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public sealed class LineaFactura : Entity
{
    public LineaFacturaId LineaId { get; }
    public string Concepto { get; }
    public int Cantidad { get; }
    public Money PrecioUnitario { get; }

    private LineaFactura()
    {
        Concepto = string.Empty;
        PrecioUnitario = null!;
        LineaId = null!;
    } // EF Core

    internal LineaFactura(LineaFacturaId lineaId, string concepto, int cantidad, Money precioUnitario)
        : base(lineaId.Value)
    {
        // C19 (RN-25): el concepto es obligatorio.
        if (string.IsNullOrWhiteSpace(concepto))
            throw new DomainException(LineaFacturaErrors.ConceptoRequerido());

        // C19 (RN-25): la cantidad es siempre positiva.
        if (cantidad <= 0)
            throw new DomainException(LineaFacturaErrors.CantidadInvalida());

        LineaId = lineaId;
        Concepto = concepto;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }

    // C19 (RN-25): el subtotal se calcula, nunca se recibe ni se persiste.
    public Money Subtotal => Money.Crear(PrecioUnitario.Monto * Cantidad, PrecioUnitario.Moneda);
}
