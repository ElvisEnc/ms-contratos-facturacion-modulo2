using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Shared;

public sealed record Servicio
{
    public TipoServicio Tipo { get; }
    public string Nombre { get; }
    public Money Precio { get; }

    private Servicio()
    {
        Nombre = string.Empty;
        Precio = null!;
    } // EF Core: Precio es OwnsOne anidado, no puede pasar por constructor (Joseco.DDD.Core)

    private Servicio(TipoServicio tipo, string nombre, Money precio)
    {
        Tipo = tipo;
        Nombre = nombre;
        Precio = precio;
    }

    public static Servicio Crear(TipoServicio tipo, string nombre, Money precio)
    {
        // C15 (RN-26): el nombre del servicio es obligatorio.
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException(ServicioErrors.NombreRequerido());

        return new Servicio(tipo, nombre, precio);
    }

    public Servicio Clonar() => new(Tipo, Nombre, Precio.Clonar());
}
