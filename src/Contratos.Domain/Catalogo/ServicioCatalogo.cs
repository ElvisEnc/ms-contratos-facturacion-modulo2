using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Catalogo;

public sealed class ServicioCatalogo : AggregateRoot
{
    public ServicioCatalogoId ServicioId { get; }
    public TipoServicio Tipo { get; }
    public string Nombre { get; private set; }
    public Money PrecioVigente { get; private set; }
    public bool Activo { get; private set; }

    private ServicioCatalogo()
    {
        Nombre = string.Empty;
        PrecioVigente = null!;
        ServicioId = null!;
    } // EF Core

    private ServicioCatalogo(ServicioCatalogoId servicioId, TipoServicio tipo, string nombre, Money precio)
        : base(servicioId.Value)
    {
        ServicioId = servicioId;
        Tipo = tipo;
        Nombre = nombre;
        PrecioVigente = precio;
        Activo = true;
    }

    public static ServicioCatalogo Crear(TipoServicio tipo, string nombre, Money precio)
    {
        // C15 (RN-26): el nombre del servicio del catálogo es obligatorio.
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException(ServicioCatalogoErrors.NombreRequerido());

        return new ServicioCatalogo(ServicioCatalogoId.New(), tipo, nombre, precio.Clonar());
    }

    public void ActualizarPrecio(Money nuevoPrecio)
    {
        // C15 (RN-26): solo se tarifa un servicio activo.
        if (!Activo)
            throw new DomainException(ServicioCatalogoErrors.Inactivo());

        var precioAnterior = PrecioVigente;
        PrecioVigente = nuevoPrecio.Clonar();
        AddDomainEvent(new PrecioActualizado(ServicioId, precioAnterior, PrecioVigente.Clonar()));
    }

    public void Desactivar()
    {
        Activo = false;
    }

    // C15 (RN-26): única puerta por la que un precio sale del catálogo, siempre clonado.
    public Servicio ObtenerServicioParaContrato() => Servicio.Crear(Tipo, Nombre, PrecioVigente.Clonar());
}
