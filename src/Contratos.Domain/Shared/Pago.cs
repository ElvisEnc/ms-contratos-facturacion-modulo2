using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public sealed record Pago
{
    public DateOnly Fecha { get; }
    public Money Monto { get; }
    public string Referencia { get; }

    private Pago()
    {
        Monto = null!;
        Referencia = string.Empty;
    } // EF Core: Monto es OwnsOne anidado, no puede pasar por constructor (Joseco.DDD.Core)

    private Pago(DateOnly fecha, Money monto, string referencia)
    {
        Fecha = fecha;
        Monto = monto;
        Referencia = referencia;
    }

    public static Pago Crear(DateOnly fecha, Money monto, string? referencia)
    {
        // C20 (RN-18): el monto del pago es siempre positivo.
        if (monto.Monto <= 0)
            throw new DomainException(PagoErrors.MontoInvalido());

        return new Pago(fecha, monto, referencia ?? string.Empty);
    }

    public Pago Clonar() => new(Fecha, Monto.Clonar(), Referencia);
}
