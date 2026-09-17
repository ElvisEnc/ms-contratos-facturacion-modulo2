---
description: Reglas de EF Core, mapeo y migraciones para la capa Infrastructure
paths:
  - "src/*.Infrastructure/**/*.cs"
---

# Reglas de persistencia

Este proyecto usa **mapeo directo del dominio con EF Core** (SISTEMA.md §12): un solo
`DbContext`, sin modelo de persistencia paralelo. El esquema exacto está en
`docs/DESIGN.md` §14.

## Mapeo
- Un `IEntityTypeConfiguration<T>` por agregado, en `Persistence/Configurations/`.
- IDs tipados con `HasConversion`.
- Value objects: `OwnsOne` / `OwnsMany`, o `HasConversion` si es de un solo campo.
- Colecciones privadas: `builder.HasMany(typeof(TEntity), "_items")` — nunca
  `HasMany(x => x.Items)`, que exigiría un setter público — **y además**
  `builder.Ignore(x => x.Items)` sobre la propiedad pública. Sin ese `Ignore`, EF
  descubre `Items` por convención, le asigna `_items` como backing field y choca con
  la navegación por string: el modelo falla en tiempo de diseño (SISTEMA.md §12.2).
- `builder.Ignore(x => x.DomainEvents);` en **todos** los agregados.
- `ValueGeneratedNever()` en **toda** clave primaria `Guid` generada por el cliente
  (SISTEMA.md §12.5). Sin esto, una entidad hija nueva puede marcarse `Modified` en vez
  de `Added` y lanzar `DbUpdateConcurrencyException`.
- Nombres de tabla y columna en `snake_case`, en minúsculas.
- Estructuras anidadas: `OwnsMany(...).ToJson()` → `jsonb`, con `HasJsonPropertyName(...)`,
  **nunca** `HasColumnName(...)` dentro del JSON (SISTEMA.md §12.4).

## Consultas LINQ con IDs tipados
- Se compara **el id tipado completo**, nunca su `.Value`: `entidad.Id == TipoId.From(guid)`.
  `entidad.Id.Value == guid` lanza `InvalidOperationException` en ejecución y **no lo
  detectan ni el compilador ni los tests con repositorios fake** (SISTEMA.md §12.5).
- Lectura siempre `AsNoTracking()`.
- Los handlers de query viven aquí, en `Infrastructure/Queries/`; el `record` de la query
  vive en `Application` (SISTEMA.md §12.8). Es la única excepción consciente al reparto
  de capas y hay que saber justificarla en la defensa.

## Índices
- Los que declara `docs/DESIGN.md` §14, con `IsUnique()` donde diga.
- Un índice único filtrado es el **respaldo físico** de una invariante de unicidad. Si la
  invariante existe y el índice no, solo vive en memoria y dos peticiones concurrentes la
  saltan (SISTEMA.md §12.5).

## Migraciones
- Una migración por cambio de esquema, con nombre descriptivo.
- **Después de generar, se abre el archivo y se lee.** No se aplica a ciegas: la migración
  es el contrato con la base de datos y es lo más caro de revertir.
- Se aplican **automáticamente al arrancar** en Development, resolviendo el migrador desde
  un scope antes del pipeline HTTP (SISTEMA.md §12.12).
- Nunca `dotnet ef migrations remove` ni `database drop`: denegados por permiso y por hook.

## UnitOfWork
- `CommitAsync` recorre el `ChangeTracker`, recoge los `DomainEvents`, **los limpia**, los
  publica por MediatR y **luego** hace `SaveChangesAsync` (SISTEMA.md §12.6). El orden
  importa: las políticas que mutan entidades ya trackeadas entran en el mismo `SaveChanges`.
- Una política que necesite datos **ya persistidos** no funciona con este orden. La solución
  no es reordenar el commit.
