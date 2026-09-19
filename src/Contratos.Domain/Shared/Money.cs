using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public sealed record Money
{
    private static readonly HashSet<string> MonedasValidas = ["BOB", "USD"];

    public decimal Monto { get; }
    public string Moneda { get; }

    private Money(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    public static Money Crear(decimal monto, string moneda)
    {
        // C18 (RN-25, §8 del maestro): el monto nunca es negativo.
        if (monto < 0)
            throw new DomainException(MoneyErrors.MontoNegativo());

        // C18 (RN-25, §8 del maestro): la moneda pertenece a la lista cerrada { BOB, USD }.
        if (string.IsNullOrWhiteSpace(moneda) || !MonedasValidas.Contains(moneda))
            throw new DomainException(MoneyErrors.MonedaInvalida());

        return new Money(monto, moneda);
    }

    public static Money Cero(string moneda) => Crear(0, moneda);

    public Money Sumar(Money otro)
    {
        // C18 (RN-25, §8 del maestro): solo se suma entre la misma moneda.
        if (Moneda != otro.Moneda)
            throw new DomainException(MoneyErrors.MonedasIncompatibles());

        return Crear(Monto + otro.Monto, Moneda);
    }

    public Money PorPorcentaje(decimal pct)
    {
        var resultado = Math.Round(Monto * pct / 100m, 2, MidpointRounding.AwayFromZero);
        return Crear(resultado, Moneda);
    }

    public Money Clonar() => new(Monto, Moneda);
}
