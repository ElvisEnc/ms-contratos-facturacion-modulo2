using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public sealed record Vigencia
{
    public DateOnly FechaInicio { get; }
    public DateOnly FechaFin { get; }
    public int DuracionDias { get; }

    public Vigencia(DateOnly fechaInicio, DateOnly fechaFin, int duracionDias)
    {
        // C17 (RN-01): la duración es siempre positiva.
        if (duracionDias <= 0)
            throw new DomainException(VigenciaErrors.DuracionIncoherente());

        // C17 (RN-01): la fecha de fin no es anterior a la de inicio.
        if (fechaFin < fechaInicio)
            throw new DomainException(VigenciaErrors.FechasInvalidas());

        // C17 (RN-01): FechaFin = FechaInicio + DuracionDias - 1.
        if (fechaFin != fechaInicio.AddDays(duracionDias - 1))
            throw new DomainException(VigenciaErrors.DuracionIncoherente());

        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        DuracionDias = duracionDias;
    }

    public static Vigencia Desde(DateOnly fechaInicio, int duracionDias)
    {
        // C17 (RN-01): la duración es siempre positiva.
        if (duracionDias <= 0)
            throw new DomainException(VigenciaErrors.DuracionIncoherente());

        return new Vigencia(fechaInicio, fechaInicio.AddDays(duracionDias - 1), duracionDias);
    }

    public bool EstaVigenteEn(DateOnly fecha) => fecha >= FechaInicio && fecha <= FechaFin;

    public Vigencia Clonar() => new(FechaInicio, FechaFin, DuracionDias);
}
