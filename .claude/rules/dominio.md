---
description: Reglas obligatorias al escribir o modificar la capa Domain de este microservicio
paths:
  - "src/*.Domain/**/*.cs"
  - "tests/**/Domain/**/*.cs"
---

# Reglas de la capa Domain

Fuente vinculante: `docs/DESIGN.md` §5 y §6 de este microservicio, y `docs/SISTEMA.md` §12.

## Encapsulamiento
- Propiedades con `private set` o solo getter. Ningún setter público.
- Colecciones: campo privado `_items` + `IReadOnlyCollection<T> Items => _items.AsReadOnly();`
- Constructor privado. Se crea con un método factory estático que valida.
- Las entidades hijas se crean **desde la raíz del agregado**, nunca por fuera.

## Invariantes
- Se comprueban dentro del agregado, antes de mutar estado.
- Cada una cita su código en el comentario: `// P3 (RN-07)`.
- Fallan lanzando `throw new DomainException(<Agregado>Errors.<Nombre>(...))` (SISTEMA.md §12.1).
- El catálogo de errores está en `docs/DESIGN.md` §13, con su `ErrorType` exacto.
  **Copiar el `ErrorType` de un error parecido es un bug**: un mismo código puede
  tener tipo distinto según quién lo lanza (SISTEMA.md §12.1).
- Desde `Domain` solo salen `ErrorType.Validation` y `ErrorType.Conflict`. Una
  precondición sobre una entidad hija del propio agregado es `Validation`, nunca `NotFound`.
- **No existe `Error.Validation(...)`** en `Joseco.DDD.Core`. Se usa el constructor:
  `new Error(codigo, mensaje, ErrorType.Validation, args)`.

## Prohibido en Domain
- `using Microsoft.EntityFrameworkCore` · `using MediatR` (salvo el `record : DomainEvent`,
  que hereda `INotification` desde el paquete) · `using Npgsql` · cualquier cosa de ASP.NET
  · `using System.Text.Json`. **El hook `guardian-capas` lo bloquea**, no es una petición.
- Excepciones nativas de .NET. Todo fallo esperable es `DomainException(Error)`.
  El middleware traduce `DomainException` y, como única excepción documentada,
  `ArgumentException` → 400 (SISTEMA.md §12.1). Cualquier otra sale como 500.
- **Nunca `Parse`, siempre `TryParse`** (SISTEMA.md §12.1.1). Toda conversión de texto
  a enum, número, `Guid` o fecha usa `TryParse`; si falla, `DomainException` con código
  de catálogo. `Enum.Parse<T>(...)` sin guarda devuelve 500 donde debía devolver 400.
- `DateTime.Now`. Usar `DateTime.UtcNow` o, mejor, recibir la fecha como parámetro
  para que los tests sean deterministas (SISTEMA.md §12.11).
- `Guid` externo en el constructor de un agregado: `Entity(Guid.Empty)` del paquete
  lanza `ArgumentException`. Los IDs se generan con `Guid.NewGuid()` dentro del factory.

## Value Objects
- `sealed record` con validación en el factory. **No `record struct`**: los IDs tipados
  son de referencia, para que la ausencia sea `null` y no `default`.
- Todo VO de varios campos expone `Clonar()` con copia profunda (SISTEMA.md §12.3).
- `Money` para todo importe. `decimal` suelto está prohibido, y `Money` valida su
  moneda contra la lista cerrada `{ BOB, USD }` (SISTEMA.md §8).

## Eventos de dominio
- `sealed record <Nombre> : DomainEvent` en `Domain/<Agregado>/Events/`.
- Se registran con `AddDomainEvent(...)` dentro del agregado, en el mismo método que
  produce el cambio, después de validar.
- `DomainEvent.OccuredOn` va con una sola `r` y el paquete lo fija con `DateTime.Now`.
  **Todo evento de integración fija `DateTime.UtcNow` al construirse** (SISTEMA.md §12.11).
