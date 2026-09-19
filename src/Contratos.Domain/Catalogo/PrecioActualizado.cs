using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Catalogo;

public sealed record PrecioActualizado(
    ServicioCatalogoId ServicioId,
    Money PrecioAnterior,
    Money PrecioNuevo) : DomainEvent;
