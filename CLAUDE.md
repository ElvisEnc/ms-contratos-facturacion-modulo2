# ms-contratos-facturacion — Guía de trabajo

Microservicio del sistema NUR-TRICENTER. BC3 — Contratos y Facturación.
Subdominio genérico. La especificación vinculante son `docs/SISTEMA.md` (copia
literal del maestro, no se edita aquí) y `docs/DESIGN.md` (este
microservicio). Ambos mandan sobre el código.

## Jerarquía documental

Todo lo que necesitas está bajo `docs/`, en este repositorio. **No hay nada que buscar
fuera de él:** la guía, la especificación y el maestro se copiaron aquí al montarlo.

| Archivo | Qué es |
|---|---|
| `docs/SISTEMA.md` | El maestro del sistema. Copia literal, no se edita aquí |
| `docs/DESIGN.md` | La especificación de este microservicio |
| `docs/GUIA.md` | El procedimiento de las seis fases, con el prompt y la condición de cierre de cada una |
| `docs/DECISIONES.md` | Las decisiones de proyecto ya cerradas, con lo que se descartó y por qué |
| `docs/JOSECO-DDD-CORE.md` | La API real del núcleo DDD y sus trampas |
| `docs/INCOHERENCIAS.md` | Las contradicciones detectadas aquí. Empieza vacío |

SISTEMA.md manda sobre DESIGN.md, y DESIGN.md manda sobre el código.
Si algo debe cambiar en un contrato de integración o en una convención
común, se cambia en SISTEMA.md primero. Nunca al revés.

**Las guías dicen cómo se construye; la especificación dice qué se construye.** Una guía
no manda sobre `DESIGN.md`: si la contradice, es una incoherencia y se registra.

## Protocolo de incoherencias — PARADA OBLIGATORIA

Si detectas una contradicción entre docs/SISTEMA.md, docs/DESIGN.md y/o el código:

1. NO la resuelvas por tu cuenta. NO elijas la opción que te parezca mejor.
2. NO sigas escribiendo código que dependa del punto en disputa.
3. Añade una entrada a `docs/INCOHERENCIAS.md` con este formato exacto:

   ### INC-<n> · <título en una línea>
   - **Detectada en:** <fase / archivo / momento>
   - **Fuente A dice:** <cita textual + §sección>
   - **Fuente B dice:** <cita textual + §sección>
   - **Por qué no puedo continuar:** <qué decisión concreta bloquea>
   - **Opciones:** A) … B) … C) …
   - **Mi recomendación y por qué:** <una, con motivo técnico>
   - **Impacto si se elige mal:** <qué se rompe después>
   - **Estado:** ABIERTA

4. Dímelo en la respuesta, en la primera línea, empezando por `⚠ INC-<n>`.
5. Si puedes seguir con otra parte que no dependa de eso, sigue y déjalo
   aislado. Si no, para y espera.

Nunca cambies el estado de una incoherencia. Eso lo hago yo.

## Reglas no negociables

- **Modelo rico.** Ningún setter público en el dominio. Nada de clases con solo
  propiedades y la lógica en el handler: eso es el modelo anémico y es lo que
  la Actividad 3 penaliza.
- **Las invariantes viven donde dice §6 de docs/DESIGN.md**, que para cada una
  indica si es del agregado, del VO, de la entidad hija o del handler. No se
  mueven de sitio. Cada comprobación cita su código: `// C11 (RN-25)`.
- **Los códigos no se inventan ni se renumeran.** RN-xx y HU-xx salen de
  SISTEMA.md; C1–C22 de docs/DESIGN.md §6. Si falta uno, es una incoherencia.
- **Regla de dependencias:** Domain ← Application ← Infrastructure ← WebApi.
  `Domain` referencia únicamente `Joseco.DDD.Core`; nada de EF Core, HTTP ni
  MediatR, salvo los `record : DomainEvent`.
- **Errores.** Los agregados lanzan `DomainException(Error)` con un `Error` del
  catálogo de docs/DESIGN.md §13 y el `ErrorType` exacto que ahí se declara
  (SISTEMA.md §12.1). Nunca excepciones nativas de .NET desde Domain, nunca un
  código inventado, y **nunca se copia el ErrorType de un código parecido**.
  Desde `Domain` solo salen `Validation` y `Conflict`.
- **Nunca `Parse`, siempre `TryParse`** (SISTEMA.md §12.1.1). Ninguna
  conversión de texto a enum, número, `Guid` o fecha —binding de un DTO,
  mapeo de un evento entrante, lectura de configuración— puede lanzar una
  excepción nativa. `Enum.Parse<T>(...)` sin guarda devuelve **500** donde
  debía devolver 400. La conversión fallida lanza `DomainException` con
  código de catálogo.
- **Colecciones**: campo privado `_lineas` + `IReadOnlyCollection<T>` público.
- **Idempotencia** en los tres endpoints de integración entrante.
- Un caso de uso, una carpeta, con su Command y su Handler.

## Joseco.DDD.Core

La API real está en `docs/JOSECO-DDD-CORE.md`. Léela antes de usar cualquier
tipo del paquete. En particular:

- NO existe `Error.Validation(...)`. Se usa el constructor:
  `new Error(codigo, mensaje, ErrorType.Validation, args)`.
- `DomainEvent.OccuredOn` va con una sola `r` y usa `DateTime.Now` (hora
  local). **Todo evento de integración fija `DateTime.UtcNow` al construirse**
  (INC-S4, `SISTEMA.md` §12.11).
- `Entity(Guid.Empty)` lanza `ArgumentException`, no `DomainException`. Por eso
  **ningún `Guid` externo llega al constructor de un agregado**: se genera con
  `Guid.NewGuid()` dentro del factory (INC-S2).
- `IRepository<T>` solo tiene `GetByIdAsync` y `AddAsync`. Lo demás va en
  `IContratoRepository`, `IFacturaRepository`, `IControlAdicionalRepository` e
  `IServicioCatalogoRepository`, declaradas en Domain (§14.5).
- `builder.Ignore(x => x.DomainEvents)` en la configuración de cada agregado.
- Los agregados lanzan, no devuelven `Result`. No mezcles los dos estilos.

## Reglas propias de BC3

**El dinero SIEMPRE es `Money`, nunca un `decimal` suelto.** Es la regla central:
BC3 es el único microservicio que maneja montos (SISTEMA.md §8) y ningún método
del dominio recibe ni devuelve un `decimal` de dinero. `Money` valida al
construirse que el monto no sea negativo y que la moneda esté en la **lista
cerrada `{ BOB, USD }`**, exacta y en mayúsculas: `"EUR"` se rechaza en el VO, no
en el handler. El `decimal` solo aparece en los DTO de la API y en los contratos
de evento (§9 del maestro: `monto` + `moneda`). Y **todo VO de varios campos
expone `Clonar()` y se clona al cambiar de dueño**: el precio recorre
`ServicioCatalogo.PrecioVigente` → `Servicio.Precio` → `LineaFactura.PrecioUnitario`,
y compartir la instancia deja columnas en `null` sin que ningún test lo note.

**Cuatro agregados, y son cuatro:** `ServicioCatalogo`, `Contrato`, `Factura` y
`ControlAdicional`. `LineaFactura` es entidad hija de `Factura` y la única
colección. `PacienteConocido` es un **read model, no un agregado**:
`IPacienteConocidoStore`, con su propio `SaveChangesAsync` fuera del `UnitOfWork`
(§5.7). `Factura` es agregado propio porque nace de dos orígenes y uno —el
control adicional— **no tiene contrato** (§5.2). Es materia de defensa.

**Once invariantes NO viven en el agregado**, y no es un olvido (§6.1):

| Código | Vive en | Por qué |
|---|---|---|
| C1, C2, C4, C9 | `ContratarServicioCommandHandler` | Dependen de la réplica `PacienteConocido` o del catálogo; el agregado no ve ninguno de los dos |
| C3 | Ídem + índice único filtrado en base de datos | Es una regla sobre el conjunto de contratos del paciente |
| C5 | Ídem + `UnitOfWork` | Es una regla sobre dos agregados a la vez |
| C7, C10 | `ProcesarEvaluacionRegistradaCommandHandler` | Cruzan `ControlAdicional`, `Contrato` y el catálogo |
| C8 | `CancelarContratoCommandHandler` | Cruza `Contrato` y `Factura` |
| C13 (monto exacto) | `RegistrarPagoCommandHandler` | El total está en `Factura`, el pago en `Contrato` |
| C16 | `PublicarServicioContratadoHandler` | El agregado no accede a réplicas de otros contextos |

Sus objetos `Error` **sí** se catalogan con el prefijo `C` (§12.1 del maestro):
el vocabulario del error es vocabulario de dominio aunque lo lance Application.

**C11: los tres importes se calculan, nunca se reciben.** `Factura.Emitir` recibe
**líneas y un porcentaje** (`0 ≤ pct ≤ 100`). El impuesto se redondea una sola vez
sobre el subtotal ya sumado (`MidpointRounding.AwayFromZero`) para que
`subtotal + impuesto = total` al céntimo. La tasa vive en `FacturacionOptions`,
13 por defecto, nunca en Domain.

**C17 y C4: la vigencia no se elige, se deriva.** `FechaFin = FechaInicio +
DuracionDias − 1` —15 días desde el 1 terminan el 15— y `EstaVigenteEn` es
verdadero en **ambos** bordes. Para `CATERING` las fechas salen de
`PacienteConocido` (RN-01b): el comando no las acepta y la `FechaInicio` opcional
se **ignora en silencio**. `COMPLETADO` se **deriva** para leer —`EstadoDerivado(hoy)`,
nunca la columna— y se materializa con `Contrato.Completar(hoy)` antes de
recontratar: ese cierre perezoso hace viable el índice único filtrado de C3.

**RN-06 es una lectura binaria, no un contador.** Con catering vigente en la fecha
nunca se cobra un control, y una `QUINCENAL` no genera cobro jamás. No cobrar es
**200 sin efecto, no un error**, y C10 se comprueba **antes que cualquier otra
condición** del handler.

## Cómo trabajamos

- Fases en orden. Ninguna empieza si la anterior no compila y no pasa tests.
- **Cada fase tiene su apartado en `docs/GUIA.md`**, con el prompt literal, el reparto
  `[TÚ]` / `[CC]` y la condición de cierre. Cuando te pida trabajar en una fase, lee ese
  apartado antes de proponer nada, y al cerrarla recorre su condición de cierre citándola
  textualmente. No improvises un procedimiento paralelo al que la guía ya escribe.
- Antes de escribir código de una fase, propón el plan y espera aprobación.
- Al terminar un bloque: `dotnet build` y `dotnet test`.
- No ejecutes `git commit` ni `git push`. Prepara el mensaje y para.
- No ejecutes nada que borre datos o migraciones.

## Stack

- .NET 10 (`net10.0`) · C# 13
- `Joseco.DDD.Core` 1.0.2
- MediatR **12.5.0** (CQRS) — versión pineada, no la subas: 13+ exige
  licencia y avisa en cada build (INC-S3)
- EF Core 10 · PostgreSQL 16 · xUnit · Swashbuckle

## Estructura

```
src/Contratos.Domain          los cuatro agregados, VOs (Money primero),
                              eventos de dominio, catálogos *Errors
src/Contratos.Application     commands, queries (records), handlers, políticas,
                              IntegrationEvents/, ReadModels/, puertos
src/Contratos.Infrastructure  DbContext, configuraciones, repositorios,
                              PacienteConocidoStore, UnitOfWork, Queries/,
                              publicador de eventos
src/Contratos.WebApi          controllers, Program.cs, middleware de excepciones
tests/Contratos.Domain.Tests       un test por invariante de dominio
tests/Contratos.Application.Tests  orquestación, idempotencia y contratos
```

## Comandos

```
dotnet build
dotnet test
dotnet run --project src/Contratos.WebApi     # http://localhost:5030/swagger
dotnet ef migrations add <Nombre> --project src/Contratos.Infrastructure --startup-project src/Contratos.WebApi
dotnet format
```

PostgreSQL en Docker: contenedor `nur-tricenter-postgres`, base `contratos_db`,
puerto 5432. La API escucha en **5030**.

## Commits

Conventional commits en español, imperativo, sin punto final.
`feat(dominio): agregar invariante C11 de calculo de factura`
Tipos: feat · fix · test · refactor · docs · chore
