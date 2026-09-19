using Joseco.DDD.Core.Abstractions;
using MediatR;

namespace Contratos.Infrastructure.Persistence;

// §12.6 del maestro: recoge los DomainEvents del ChangeTracker, los limpia,
// los publica por MediatR y DESPUÉS hace SaveChangesAsync. El orden importa:
// las políticas que reaccionan mutan entidades ya trackeadas por el mismo
// DbContext, y esos cambios entran en el mismo SaveChanges.
public sealed class UnitOfWork(ContratosDbContext dbContext, IPublisher publisher) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var entidadesConEventos = dbContext.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        var eventos = entidadesConEventos
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (var entidad in entidadesConEventos)
            entidad.ClearDomainEvents();

        foreach (var evento in eventos)
            await publisher.Publish((object)evento, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
