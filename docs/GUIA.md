# `ms-contratos-facturacion` — Guía de construcción, fases 1 a 6

> **BC3 — Contratos y Facturación** · subdominio genérico · puerto `5030` · base `contratos_db` · invariantes `C1`–`C22` · historias HU-19 a HU-24
>
> Fases **1 y 2** = Actividad 3 (capa de dominio). Fases **3 a 6** = Actividad 4
> (microservicio completo, demo y defensa). Requiere `00-ARRANQUE.md` hecho:
> el repositorio montado con su andamiaje y PostgreSQL arriba.

---

## Cómo se ejecuta esta guía

Todo lo de aquí lo lanzas **tú**, desde `claude` abierto en la raíz del repositorio.
No hay ningún comando que ejecute la guía por ti: los bloques marcados `[CC]` son
prompts literales que copias y pegas.

| Tag | Quién ejecuta | Modo | Coste |
|---|---|---|---|
| `[TÚ]` | Tú, a mano en PowerShell | — | cero |
| `[TÚ-CHECK]` | Tú, y miras la salida | — | cero |
| `[CC]` | Claude Code, prompt literal | normal | normal |
| `[CC-PLAN]` | Claude Code, prompt literal | **modo plan** (`Shift+Tab`) | normal |
| `[CC-SUB]` | Subagente, contexto aislado | normal | aislado |
| `[GATE]` | Puerta de fase, no se salta | — | cero |

**Modo plan.** `Shift+Tab` hasta que la línea de estado diga `plan mode`. Ahí Claude
lee y propone, pero no escribe ni ejecuta nada con efectos. Lees el plan, lo corriges
y recién entonces apruebas. Se sale del modo plan al aprobar. Todo bloque `[CC-PLAN]`
de esta guía asume que ya estás en modo plan **antes** de pegar el prompt.

**Subagentes.** Se invocan con una frase, dentro de la sesión normal:

```
Usa el subagente revisor-ddd para revisar src/<Nombre>.Domain.
```

Arrancan con contexto limpio y te devuelven solo el informe. Los tres —`revisor-ddd`,
`verificador-invariantes`, `auditor-contratos`— **no pueden escribir**: informan, y
los arreglos los pides tú después. `/agents` los lista.

**Skills disponibles en el repositorio:**

- `/incoherencia <descripción>` — registra una contradicción entre `SISTEMA.md`,
  `DESIGN.md` y el código, con el formato exacto, y **no la resuelve**.
- `/defensa` — te interroga sobre el código que existe de verdad en el repositorio.

**Hooks que corren solos, sin que los invoques** (`/hooks` los lista):
al abrir sesión te avisa de las incoherencias abiertas; antes de cada edición bloquea
un `using` de infraestructura dentro de `Domain`; antes de cada comando bloquea lo
destructivo; y al terminar cada turno formatea, compila y **no te devuelve el turno
con el build roto**.

**Ciclo de cada fase, que no cambia:**

```
1. [TÚ]       Leer la sección del DESIGN.md que toca. Diez minutos. No se salta.
2. [CC-PLAN]  Pedir el plan. Leerlo. Corregirlo. Aprobarlo.
3. [CC]       Ejecutar, bloque por bloque.
4. [TÚ-CHECK] dotnet build && dotnet test
5. [CC-SUB]   Revisión con el subagente que toque.
6. [TÚ]       Leer el diff entero.
7. [TÚ]       git commit
8. [GATE]     ¿Compila, pasan los tests, el diff es lo que pediste, no hay INC abiertas?
```

`/clear` entre fases, siempre. Sobrevive lo que importa: `CLAUDE.md`, las reglas de
`.claude/rules/`, los agentes y los hooks.

---

## Qué se entrega y qué se evalúa — Actividad 3

| | |
|---|---|
| **Entregable** | Repositorio Git con la capa de dominio y un `README.md` con el diagrama de clases renderizado en GitHub |
| **Se evalúa** | Que el modelo sea **rico**: agregados con comportamiento e invariantes dentro, no clases con propiedades y la lógica en otro sitio |
| **Se penaliza** | El modelo anémico. Es el único error que hunde la nota por sí solo |
| **Cierra en** | Fase 2 |

Lo que un evaluador mira primero, en este orden: ¿los agregados tienen métodos de negocio o solo `get/set`? ¿dónde se comprueban las invariantes? ¿hay un test por invariante? ¿el diagrama del README coincide con el código?

En este microservicio hay una segunda cosa que se mira: **si el dinero es un tipo o es un `decimal`**. Un `decimal` suelto en una firma del dominio de BC3 es la versión monetaria del modelo anémico.

**Recordatorio de tags:** `[TÚ]` lo haces tú, cero tokens · `[TÚ-CHECK]` comando cuya salida miras tú · `[CC]` prompt literal · `[CC-PLAN]` en modo plan · `[CC-SUB]` subagente · `[GATE]` puerta.

---
## Antes de empezar

`[TÚ]` Cinco minutos, y evitan la mitad de los problemas:

- [ ] `docs/SISTEMA.md`, `docs/DESIGN.md` y `docs/JOSECO-DDD-CORE.md` están dentro del repositorio.
- [ ] `docs/GUIA.md` es la de **este** microservicio, no la de otro. Es el procedimiento que sigues tú y el documento al que apuntas a Claude cuando le pides que se ciña a una fase.
- [ ] `CLAUDE.md` está en la **raíz**, no en `.claude\`.
- [ ] Leíste `docs/DECISIONES.md`. Están **todas cerradas**: no hay nada que decidir antes de la fase 1. Se lee para saber qué se decidió y por qué, que es material de defensa.
- [ ] `docker compose ps` muestra `nur-tricenter-postgres` healthy.
- [ ] Leíste **`docs/DESIGN.md` §5, §6 y §7 enteras**. No se salta. Diez minutos ahora, o tres horas después revisando un modelo que implementó otra cosa.
- [ ] Leíste `docs/SISTEMA.md` §8, la ficha de `Money` y la lista cerrada de monedas. Es media página y es la regla que más se incumple en este BC.

---
## Fase 1 — Solución, proyectos y paquetes

> **Ficha de la fase 1**
>
> | | |
> |---|---|
> | Quién | **Tú, a mano.** No se le pide a Claude Code: no hay ninguna decisión aquí y cuesta contexto para siempre |
> | Modo | — |
> | Reglas que se cargan solas | ninguna todavía |
> | Subagentes | ninguno |
> | Hooks que van a actuar | ninguno: no hay turnos de Claude en esta fase |
> | Cierra con | `dotnet build` en verde y el primer commit |

Es la fase más mecánica del proyecto. **Hazla tú.** Pedírsela a Claude Code cuesta tokens de entrada, de salida y de contexto para siempre, a cambio de nada: no hay ninguna decisión aquí.

`[TÚ]` En la raíz del repositorio:

```powershell
dotnet new sln -n Contratos

dotnet new classlib -o src/Contratos.Domain              -f net10.0
dotnet new classlib -o src/Contratos.Application         -f net10.0
dotnet new classlib -o src/Contratos.Infrastructure      -f net10.0
dotnet new webapi   -o src/Contratos.WebApi              -f net10.0
dotnet new xunit    -o tests/Contratos.Domain.Tests      -f net10.0
dotnet new xunit    -o tests/Contratos.Application.Tests -f net10.0

dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj).FullName
```

> Son **dos** proyectos de test, no uno: `docs/DESIGN.md` §17 los separa porque la fase 2 cierra con uno y la fase 3 con el otro.

`[TÚ]` Las referencias, en el único orden que respeta la regla de dependencias:

```powershell
dotnet add src/Contratos.Application         reference src/Contratos.Domain
dotnet add src/Contratos.Infrastructure      reference src/Contratos.Application
dotnet add src/Contratos.WebApi              reference src/Contratos.Application
dotnet add src/Contratos.WebApi              reference src/Contratos.Infrastructure
dotnet add tests/Contratos.Domain.Tests      reference src/Contratos.Domain
dotnet add tests/Contratos.Application.Tests reference src/Contratos.Domain
dotnet add tests/Contratos.Application.Tests reference src/Contratos.Application
```

> `Infrastructure` referencia a `Application`, no al revés. Si en algún momento necesitas la flecha contraria, algo está en la capa equivocada.

`[TÚ]` Los paquetes. **Solo `Joseco.DDD.Core` en Domain**:

```powershell
dotnet add src/Contratos.Domain          package Joseco.DDD.Core --version 1.0.2

dotnet add src/Contratos.Application     package MediatR --version 12.5.0

dotnet add src/Contratos.Infrastructure  package Microsoft.EntityFrameworkCore
dotnet add src/Contratos.Infrastructure  package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Contratos.Infrastructure  package Microsoft.EntityFrameworkCore.Tools

dotnet add src/Contratos.WebApi          package Swashbuckle.AspNetCore
dotnet add src/Contratos.WebApi          package Microsoft.EntityFrameworkCore.Design
```

> **La versión de MediatR está decidida y pineada: `12.5.0`** (INC-S3, cerrada el 5-sep-2026). Es la última bajo MIT; 13+ exige licencia y emite un aviso en cada `dotnet build`. Además es la rama que acompaña a `MediatR.Contracts` 2.0.1, de la que depende `Joseco.DDD.Core` 1.0.2. `SISTEMA.md` §11 lo fija. **No la subas.**

`[TÚ]` Borra la basura de plantilla: `Class1.cs` de las tres bibliotecas, `WeatherForecast.cs` y el endpoint de ejemplo del WebApi, `UnitTest1.cs` de los dos proyectos de test.

`[TÚ]` `src/Contratos.WebApi/Properties/launchSettings.json` → `applicationUrl` a `http://localhost:5030`. **Ahora, no al final.** Es el error que más veces revienta una demo.

`[TÚ-CHECK]`

```powershell
dotnet build
```

`[GATE]` Compila sin warnings de referencias circulares. Si no compila, no sigas.

`[TÚ]` `git add -A; git commit -m "chore: estructura de solucion y referencias de proyecto"`

---
## Fase 2 — El dominio

> **Ficha de la fase 2**
>
> | | |
> |---|---|
> | Modo | **Plan** para el plan de construcción · **normal** para cada bloque de ejecución |
> | Antes de empezar | `/clear`. Esta fase lee mucho y conviene arrancar con la ventana limpia |
> | Reglas que se cargan solas | `.claude/rules/dominio.md`, en cuanto se toca `src/*.Domain/` |
> | Subagentes al cerrar | `revisor-ddd` y `verificador-invariantes`, en ese orden |
> | Hooks que van a actuar | `guardian-capas` deniega un `using` de EF Core, MediatR, ASP.NET o `System.Text.Json` dentro de `Domain` y avisa de `DateTime.Now` · `cierre-de-turno` formatea y compila al final de cada turno |
> | Si algo no cuadra | `/incoherencia <qué chocó>` y para. No dejes que lo resuelva |
> | Cierra con | build y tests en verde, un test por invariante, los dos subagentes sin hallazgos altos, el diff leído y el commit |

Aquí se juega la Actividad 3 entera.

### El plan

`[CC-PLAN]` · contexto: alto · Entra en modo plan con **`Shift+Tab`** y pega esto:

```
Vamos a construir la capa de dominio de ms-contratos-facturacion (fase 2).

Lee completas:
- docs/DESIGN.md §5 (modelo de dominio, incluidos §5.2 límites de agregados,
  §5.3 IDs tipados, §5.4 value objects, §5.5 enumeraciones, §5.6 métodos de
  negocio y §5.7 decisiones discutibles)
- docs/DESIGN.md §6 (invariantes C1 a C22, con §6.1 y §6.2)
- docs/DESIGN.md §7 (máquinas de estado de Contrato, Factura y ControlAdicional)
- docs/DESIGN.md §8 (eventos de dominio y políticas)
- docs/DESIGN.md §13 (catálogo de errores)
- docs/SISTEMA.md §8, la ficha de Money y la lista cerrada de monedas
- docs/JOSECO-DDD-CORE.md entero

Propón el plan de construcción de src/Contratos.Domain: qué archivos, en qué
orden, y qué contiene cada uno. Sigue el orden interno de la fase 2 de §17,
que empieza por los IDs tipados y pone Money antes que cualquier otro VO.
Incluye la lista de las invariantes C1 a C22 diciendo, para cada una, dónde la
vas a poner según §6 y §6.1.

No escribas código todavía. Si algo de lo que leas no cuadra con
docs/SISTEMA.md, aplica el protocolo de incoherencias del CLAUDE.md y para.
```

`[TÚ]` **Lee el plan y compruébalo contra §6.** Lo que tiene que decir:

- Cuatro agregados: `ServicioCatalogo`, `Contrato`, `Factura` y `ControlAdicional`. Si propone `Factura` como entidad hija de `Contrato`, no leyó §5.2 — corrígelo antes de aprobar, y recuérdale que la factura de un control adicional no tiene contrato al que colgarse.
- **Once invariantes fuera del agregado**: C1, C2, C3, C4, C5, C7, C8, C9, C10, C16 y la mitad de C13. Si las mete dentro, no leyó §6.1.
- `LineaFactura` como **entidad hija con `LineaFacturaId`**, no como VO ni con clave sombra `int`.
- `Money` como primer VO de todos, con la lista cerrada `{ BOB, USD }` validada dentro del propio VO.
- `PacienteConocido` **no está en Domain**: es un read model y vive en `Application` (§5.7). Si aparece en el plan del dominio, córtalo.
- Los siete eventos de dominio de §8.1, con su payload completo, incluido `ContratoCompletado`.

### IDs tipados, value objects y enumeraciones

`[CC]` · contexto: medio

```
Ejecuta la primera parte del plan aprobado: IDs tipados (§5.3), value objects
(§5.4) y enumeraciones (§5.5) de docs/DESIGN.md.

Orden obligatorio: primero los siete IDs tipados y TypedIdValidator (C22),
después los enums propios, después Money, y solo entonces el resto.

Reglas:
- Los value objects son `sealed record` y validan en su factory o constructor.
- Money valida el monto no negativo Y la moneda contra la lista cerrada
  { BOB, USD } DENTRO DEL PROPIO VO, con comparación exacta y en mayúsculas.
  "EUR" y "bob" se rechazan. Expone Sumar (misma moneda), PorPorcentaje
  (redondeo a 2 decimales con MidpointRounding.AwayFromZero), Cero y Clonar.
- Vigencia.Desde deriva FechaFin = FechaInicio + DuracionDias - 1. El
  constructor completo valida la coherencia de los tres campos y EstaVigenteEn
  devuelve verdadero en AMBOS bordes.
- LineaFactura: Subtotal es propiedad CALCULADA, Cantidad x PrecioUnitario. No
  se recibe por parámetro (C19).
- Pago: monto > 0 y referencia nula normalizada a cadena vacía, nunca null (C20).
- TODO VO de varios campos expone Clonar() con copia profunda de su Money
  anidado: Servicio, Vigencia y Pago incluidos, no solo Money.
- Ningún método de dominio recibe ni devuelve un decimal de dinero: siempre Money.
- Todo fallo lanza DomainException con el Error del catálogo §13 y el
  ErrorType exacto que ahí se declara.
- Recuerda: no existe Error.Validation(...). Usa el constructor de Error.

No toques todavía los agregados.
```

### Los agregados `ServicioCatalogo` y `Factura`

`[CC]` · contexto: medio

```
Implementa los agregados `ServicioCatalogo` y `Factura` según docs/DESIGN.md
§5, §5.6 y §7.2.

Invariantes que les corresponden según §6: C15 y C6, C11.
(C18 y C19 ya están en sus VOs.)

Reglas:
- Constructores privados. Se crean con ServicioCatalogo.Crear y Factura.Emitir.
- Propiedades con `private set`. Las líneas de factura van en un campo privado
  `_lineas` expuesto como IReadOnlyCollection<LineaFactura>.
- Cada invariante se comprueba ANTES de mutar estado y cita su código y su RN
  en un comentario: // C11 (RN-25)
- ObtenerServicioParaContrato() devuelve un Servicio con el precio CLONADO. Es
  la única puerta por la que un precio sale del catálogo (C15, RN-26).
- ActualizarPrecio solo funciona si el servicio está activo; Desactivar es
  idempotente y no comprueba el estado previo.
- Factura.Emitir recibe LÍNEAS y un PORCENTAJE (0 a 100), nunca importes.
  Calcula Subtotal como suma de las líneas, Impuesto como porcentaje del
  subtotal ya sumado —redondeando UNA sola vez, no línea a línea— y Total
  como Subtotal + Impuesto, de modo que subtotal + impuesto = total al céntimo.
- LineaFactura se construye dentro del agregado y su PrecioUnitario es un
  Money clonado.
- Máquina de estados de §7.2: PAGADA y ANULADA son terminales, y ambas
  transiciones solo salen de EMITIDA (C6).
- Registra los eventos de dominio de §8.1 con AddDomainEvent, dentro del
  método que produce el cambio, después de validar: FacturaEmitida lleva el
  Total como Money entero, y PrecioActualizado lleva precio anterior y nuevo.

Crea también `ServicioCatalogoErrors` y `FacturaErrors` con los códigos de §13,
con el prefijo de invariante completo: C11_FACTURA_SIN_LINEAS, no
FACTURA_SIN_LINEAS.
```

### Los agregados `Contrato` y `ControlAdicional`

`[CC]` · contexto: medio

```
Implementa los agregados `Contrato` y `ControlAdicional` según docs/DESIGN.md
§5, §5.6, §7.1 y §7.3.

Invariantes que les corresponden: C12, C13 (unicidad del pago), C14 y C21.
(C17 y C20 ya están en sus VOs.)

Reglas:
- Contrato.Contratar recibe el FacturaId ya generado: no existe un contrato sin
  factura ni por un instante (C5), y por eso FacturaId no es nullable ni tiene
  setter. Lo mismo para ControlAdicional.Generar: nace con su FacturaId.
- Contrato.RegistrarPago exige Estado ACTIVO y Pago null. La comprobación del
  monto exacto contra el total de la factura NO va aquí: es orquestación (§6.1).
- Contrato.Cancelar solo desde ACTIVO; CANCELADO es terminal (C14).
- Contrato.Completar(hoy) exige Estado ACTIVO y hoy > Vigencia.FechaFin (C12).
  Es el cierre perezoso de §5.7 y hoy no existe en ningún linaje del código:
  hay que escribirlo, con su evento ContratoCompletado.
- EstadoDerivado(hoy) devuelve CANCELADO si lo está, COMPLETADO si
  hoy > Vigencia.FechaFin, y ACTIVO en otro caso. No escribe nada.
- Las reglas temporales reciben `hoy` como parámetro; las fechas del acto
  —FechaContratacion, Factura.Fecha— las sella el agregado. Nada de IClock.
- ControlAdicional.Generar exige Cobro > 0 y nace con Pagado = false;
  MarcarPagado es irreversible y falla la segunda vez (C21).
- Eventos de §8.1: ServicioContratado, PagoRegistrado, ContratoCancelado,
  ContratoCompletado, ControlAdicionalGenerado. El evento de dominio
  ServicioContratado lleva DuracionDias como int NO nulable: la nulidad del
  contrato externo la impone la política, no el dominio (§8.3).

Crea `ContratoErrors` y `ControlAdicionalErrors` con los códigos de §13, con el
prefijo de invariante completo.
```

### Los tests — uno por invariante, como mínimo

`[CC]` · contexto: medio

```
Escribe los tests unitarios de la capa de dominio en tests/Contratos.Domain.Tests,
siguiendo la tabla de docs/DESIGN.md §15.1: esa tabla ya trae la clase y el
nombre de cada test. Úsala como especificación, no inventes otra.

Regla de SISTEMA.md §13.1: como mínimo UN test por invariante declarada, y el
nombre del test cita el código de la invariante. Una invariante sin test tiene
que ser visible por ausencia.

Para C6, C11, C12, C13 (unicidad), C14, C15, C17, C18, C19, C20, C21 y C22:
- Un test del camino que la viola, afirmando el código de error exacto del
  catálogo §13 Y su ErrorType. No basta con afirmar que lanza.
- Un test del camino feliz cuando aporte algo.

Cuatro tests que no se pueden omitir:
- Money rechaza "EUR" con C18_MONEY_MONEDA_INVALIDA.
- Vigencia.Desde con 15 días produce FechaFin = FechaInicio + 14.
- Factura.Emitir con un monto que exige redondeo cumple
  subtotal + impuesto = total exactamente.
- ObtenerServicioParaContrato devuelve un Money CLONADO, no la misma
  instancia: compara referencias, no valores.

C1, C2, C3, C4, C5, C7, C8, C9, C10, C13 (monto exacto) y C16 NO se prueban
aquí: son de orquestación y se prueban en la fase 3 (§15.2). Deja un comentario
en el proyecto de tests diciéndolo, para que su ausencia no parezca un olvido.

Sin base de datos, sin repositorios, sin mocks de infraestructura.
```

`[TÚ-CHECK]`

```powershell
dotnet build
dotnet test tests/Contratos.Domain.Tests
```

### La revisión

`[CC-SUB]` · Dos subagentes, en este orden:

```
Usa el subagente revisor-ddd para revisar src/Contratos.Domain.
```

```
Usa el subagente verificador-invariantes para verificar C1 a C22.
```

`[TÚ]` Aplica los hallazgos de gravedad alta y media. Los bajos, decídelos tú.

**Lo que el revisor debe encontrar vacío si el dominio está bien:** ningún setter público, ninguna `List<T>` expuesta, ningún `using` de EF Core o MediatR fuera de los `record : DomainEvent`, ninguna excepción nativa, ninguna invariante fuera del sitio que le asigna §6, y **ningún `decimal` de dinero en una firma pública**.

### El diff

`[TÚ]` `git diff` y **léelo entero**. Es la fase más barata y la que más vale. Estás firmando este código: en la defensa te van a preguntar por qué `Factura` es un agregado propio y no una entidad hija de `Contrato`, y la respuesta tiene que salir de tu cabeza.

`[GATE]` No se pasa a la fase siguiente hasta que:

- [ ] `dotnet build` sin warnings
- [ ] `dotnet test` en verde
- [ ] Un test por cada invariante del dominio, con su código citado
- [ ] `revisor-ddd` sin hallazgos altos
- [ ] `verificador-invariantes` sin códigos incompletos
- [ ] Leíste el diff
- [ ] `docs/INCOHERENCIAS.md` sin entradas ABIERTAS que afecten al dominio

---
## Dónde se gana la nota en este dominio

La diferencia entre un modelo que aprueba y uno que destaca, en BC3, está en cuatro sitios concretos:

**1. `Factura` y `ControlAdicional` como agregados propios.** Es la decisión de modelado más discutible del microservicio y §5.2 la argumenta con tres razones ordenadas por peso. La primera es la que cierra la discusión: una factura nace de una contratación **o de un control adicional**, y el control adicional existe precisamente cuando **no hay contrato de catering**. Si `Factura` viviera dentro de `Contrato`, no habría agregado al que colgar la factura de un control. La pregunta inversa —«cuatro raíces, ¿no son demasiadas?»— cae seguro.

**2. Las once invariantes de orquestación.** Que C1, C2, C3, C4, C5, C7, C8, C9, C10, C16 y la mitad de C13 no estén en los agregados **no es un fallo, es la respuesta correcta**, y §6.1 explica por qué cada una. Un evaluador que ve C3 dentro de `Contrato` sabe que el modelo no entendió los límites de consistencia. Ten preparada la frase: «un agregado nunca carga otro agregado, y ninguno ve el conjunto».

**3. `Money` y `Factura.Emitir`.** Es el ejemplo de libro de value object con comportamiento y de agregado que calcula en vez de recibir. Un `Money` que solo guarda un decimal y una cadena es una estructura de datos; uno que valida su moneda contra una lista cerrada, se niega a sumarse con otra moneda y se clona al cambiar de dueño es dominio. Y `Factura.Emitir` recibe **líneas y un porcentaje**: el cliente HTTP no tiene forma de aportar el total. Ahí se demuestra RN-25 sin decir una palabra.

**4. `COMPLETADO` derivado más el cierre perezoso.** `EstadoDerivado(hoy)` es aritmética, no un hecho que alguien decida, así que las consultas nunca leen la columna. Pero la columna **sí se materializa** justo antes de recontratar, con `Contrato.Completar(hoy)`. Esa combinación es lo que hace viable el índice único filtrado que respalda RN-19: el argumento contra ese índice —«un contrato vencido bloquearía la recontratación para siempre»— es correcto en el diagnóstico, y la respuesta no fue renunciar al índice sino eliminar la causa. Eso es diseño, no configuración.

---
## Errores ya cometidos en este dominio

De `docs/DESIGN.md` §19, que auditó los dos linajes de código reales. Estos ya se pagaron una vez:

| # | Qué pasó | Cómo se evita ahora |
|---|---|---|
| D-01 | `Money` solo comprobaba que la moneda no fuera vacía: aceptaba `"EUR"` y `"XYZ"` en el único microservicio que maneja dinero | El prompt de §3.2 exige la lista cerrada dentro del VO, y §3.5 pide el test que rechaza `"EUR"` |
| D-04 | `EstaVigenteEn` calculaba `FechaInicio.AddDays(DuracionDias)` **sin el `−1`**: un catering de 15 días era vigente 16, y ese día extra no se cobraba un control que sí correspondía | El prompt de §3.2 fija la aritmética en el VO `Vigencia`, que es exactamente para lo que existe |
| D-06 | `Contrato.Completar(hoy)` no existía en ningún linaje: `COMPLETADO` estaba declarado en el enum y **ningún camino de código lo asignaba** | El prompt de §3.4 lo pide con su guarda y su evento. Es prerrequisito del índice único de C3 |
| D-10 | Un linaje emitía la factura con un `Money subtotal` suelto y una tasa como **fracción** (`0.13m`) en vez de líneas y porcentaje | El prompt de §3.3 fija líneas + porcentaje `0..100`. Fracción y porcentaje conviviendo hacen que un `13` mal leído multiplique el impuesto por cien |
| D-11 | `Contrato` aplanado: `TipoServicio`, `PrecioContratado`, `FechaInicio?` y `DuracionDias?` sueltos, sin `Vigencia` ni `Pago` | Los VO de §5.4 son obligatorios. La aritmética encapsulada es lo que habría evitado D-04 |
| D-16 | Solo `Money` tenía `Clonar()`; `Servicio`, `Vigencia` y `Pago` no. Fallo latente, no activo | El prompt de §3.2 lo exige para todo VO de varios campos (§12.3 del maestro) |
| D-17 | Errores sin el prefijo de invariante: `FACTURA_SIN_LINEAS`, `MONEY_MONTO_NEGATIVO`, `CONTROL_YA_PAGADO`… y un linaje con **otra numeración** | Los prompts de §3.3 y §3.4 exigen el código literal de §13. La numeración de este `DESIGN.md` es la vinculante |

---
## El `README.md` de la Actividad 3

`[CC]` · contexto: bajo · **Solo cuando el dominio esté cerrado**, para que el diagrama refleje el código real:

```
Escribe el README.md de este repositorio para la entrega de la Actividad 3.

Contenido:
1. Qué es ms-contratos-facturacion: propósito del bounded context en una frase
   y su papel en NUR-TRICENTER (BC3, subdominio genérico, el contexto que
   habilita a los demás).
2. Funcionalidades, referenciadas a las historias HU-19 a HU-24.
3. Diagrama de clases de la capa de dominio en Mermaid (classDiagram), para
   que GitHub lo renderice. SOLO esta capa: los cuatro agregados, la entidad
   hija, los value objects, las enumeraciones y sus relaciones. Nada de
   infraestructura y nada de PacienteConocido, que es un read model.
4. Tabla de invariantes C1 a C22 con la regla de negocio que implementa cada
   una y dónde se hace cumplir, distinguiendo las de dominio de las once de
   orquestación.
5. Puesta en marcha: docker compose up -d, dotnet build, dotnet test.

El diagrama tiene que coincidir EXACTAMENTE con las clases que existen en
src/Contratos.Domain. Léelas antes de dibujarlo; no lo generes desde el
DESIGN.md, porque el código es lo que se entrega.
```

`[TÚ-CHECK]` Sube el README a GitHub y **mira que el diagrama renderice**. Un `classDiagram` con un error de sintaxis se ve como un bloque de texto crudo, y eso en una entrega que pide «diagrama renderizado en GitHub» es media nota.

---
## Checklist de entrega — Actividad 3

- [ ] La solución compila y `dotnet test` pasa
- [ ] `src/Contratos.Domain` no referencia EF Core, MediatR (salvo `DomainEvent`), ASP.NET ni `System.Text.Json`
- [ ] Ningún setter público, ninguna `List<T>` expuesta, ningún constructor público que salte la validación
- [ ] **Ningún `decimal` de dinero en una firma del dominio**: siempre `Money`
- [ ] Las veintidós invariantes implementadas, cada una donde §6 dice, con su código citado en comentario
- [ ] Los códigos de error con su prefijo de invariante completo, tal cual §13
- [ ] Un test por cada invariante de dominio, con el código en el nombre
- [ ] `README.md` con el diagrama de clases renderizando en GitHub
- [ ] `docs/INCOHERENCIAS.md` presente, con las que hayas encontrado y su resolución
- [ ] Historial de commits en conventional commits, legible
- [ ] Repositorio **propio**, no una carpeta dentro de otro (SISTEMA.md §15.2)

`[TÚ]` Ensayo rápido antes de entregar:

```
/defensa
```

Responde las cinco primeras preguntas. Si alguna te deja en blanco, esa parte del modelo aún no es tuya.

---
## Qué se entrega y qué se evalúa — Actividad 4

| | |
|---|---|
| **Entregable** | Repositorio con el microservicio completo y un `README.md` con descripción, funcionalidades y el diagrama de clases de su propia capa de dominio |
| **Se evalúa** | DDD + Arquitectura Limpia + CQRS, exponiendo una API |
| **Defensa** | Demo del funcionamiento **y** exposición del código demostrando la estructura solicitada |
| **Cierra en** | Fase 6 |

La demo es la mitad de la nota y es la parte que más se rompe por detalles tontos: un puerto que no coincide, una migración que no se aplicó, un `demo.http` que no cubre lo que dices que cubre.

---
## Antes de la fase 3 — un ajuste del andamiaje

`[TÚ]` **Activa los tests en el hook de cierre de turno.** En
`.claude/hooks/cierre-de-turno.ps1` hay un bloque comentado marcado `2b`.
Descoméntalo ahora, que la fase 2 ya dejó tests: a partir de aquí es **imposible
terminar un turno con tests en rojo**, no solo con el build roto. Cuesta entre diez
y veinte segundos por turno, y a cambio no se entrega una fase con la suite caída.

`[TÚ-CHECK]` Comprueba que sigue funcionando: rompe un test a propósito, pídele a
Claude cualquier cosa pequeña y mira que no te devuelva el turno. Después arréglalo.

---

## Fase 3 — Application

> **Ficha de la fase 3**
>
> | | |
> |---|---|
> | Modo | **Plan** para el plan · **normal** para casos de uso, contratos y tests |
> | Antes de empezar | `/clear`, y activar los tests en el hook de cierre de turno (justo arriba) |
> | Reglas que se cargan solas | ninguna: `Application` no tiene regla de capa propia, manda el `CLAUDE.md` |
> | Subagentes al cerrar | `auditor-contratos` — compara campo a campo contra `SISTEMA.md` §9 |
> | Hooks que van a actuar | `cierre-de-turno`, ahora también con los tests: **no se termina un turno con la suite en rojo** |
> | Cierra con | build y tests en verde, el auditor sin discrepancias, el diff leído y el commit |

Aquí entran las **once invariantes de orquestación** que el dominio deliberadamente no tiene: C1, C2, C3, C4, C5, C7, C8, C9, C10, C16 y la mitad de C13.

### El plan

`[CC-PLAN]` · contexto: alto · `Shift+Tab`, y:

```
Fase 3: capa Application de ms-contratos-facturacion.

Lee completas:
- docs/DESIGN.md §9 (integración con otros contextos, con §9.2 el cálculo de la
  vigencia, §9.3 idempotencia y §9.4 read models), §10 (diseño CQRS),
  §12 (flujos principales), §6.1 (invariantes de orquestación)
- docs/SISTEMA.md §9 (contratos de eventos) y §8 (tipos compartidos), solo los
  cinco eventos que este microservicio publica o consume: PacienteRegistrado,
  PlanAlimenticioAsignado, EvaluacionRegistrada, ServicioContratado y
  FacturaEmitida
- docs/SISTEMA.md §12.6 (unidad de trabajo y eventos), §12.9 (read models de
  integración), §13.2 (qué se prueba aquí)

Propón el plan: casos de uso (una carpeta cada uno, con Command y Handler),
queries como records, las dos políticas de §8.2, contratos de
IntegrationEvents/ entrantes y salientes, el read model PacienteConocido con
IPacienteConocidoStore, FacturacionOptions, y los puertos (las cuatro
interfaces de repositorio de §14.5, IUnitOfWork, IIntegrationEventPublisher).

Di explícitamente en qué handler va cada una de C1, C2, C3, C4, C5, C7, C8,
C9, C10, C16 y la mitad de C13.

No escribas código todavía.
```

`[TÚ]` Comprueba en el plan:

- C1, C2, C3, C4, C5, C9 y C12 (la llamada al cierre perezoso) en `ContratarServicioCommandHandler`; C7 y C10 en `ProcesarEvaluacionRegistradaCommandHandler`; C8 en `CancelarContratoCommandHandler`; C13 (monto exacto) en `RegistrarPagoCommandHandler`; C16 en `PublicarServicioContratadoHandler`.
- Las dos políticas son **traductoras puras**: no mutan estado. `PublicarServicioContratadoHandler` sí **lee** la réplica, porque C16 lo exige; eso no la convierte en un caso de uso.
- Los `record` de las cinco queries están en `Application`; sus **handlers** irán en `Infrastructure/Queries/` en la fase 4 (§12.8 del maestro). Si el plan pone los handlers de query aquí, corrígelo.
- `ContratarServicioCommand` tiene exactamente tres campos: `PacienteId`, `TipoServicio` y una `FechaInicio` opcional. Si aparece `DuracionDias` o `FechaFin`, el plan rompió RN-01b antes de escribir una línea.

### Los casos de uso

`[CC]` · contexto: medio

```
Ejecuta el plan aprobado: commands, handlers y políticas de Application.

Reglas:
- Un caso de uso, una carpeta, con su Command y su Handler. Los tres commands
  de integración van en Application/Integracion/, separados de los comerciales.
- El handler orquesta: carga los agregados, llama a los métodos de negocio,
  hace un solo CommitAsync. NUNCA implementa lógica que pertenece al agregado.
- C1, C2, C3, C4, C5, C7, C8, C9, C10, C16 y C13 (monto exacto) SÍ van aquí,
  cada una en su handler (§6.1), lanzando el Error catalogado que le
  corresponde con su ErrorType exacto de §13.
- ContratarServicioCommandHandler, en el orden de §12.1: réplica (C1),
  catálogo (C9), plan activo (C2), vigencia derivada del plan (C4), cierre
  perezoso de los vencidos (C12), C3, ObtenerServicioParaContrato, emitir la
  factura, crear el contrato con su FacturaId, un solo CommitAsync (C5).
- Para CATERING las fechas salen SIEMPRE de PacienteConocido, nunca del
  command; la FechaInicio que traiga el cliente se ignora en silencio (C4).
  Si PlanDuracionDias no es 15 ni 30, C4_DURACION_INVALIDA.
- C13: RegistrarPagoCommandHandler compara monto Y moneda contra
  Factura.Total ANTES de MarcarPagada. Sin esa comparación se registra un pago
  de 1 BOB contra una factura de 5085 BOB y la factura queda PAGADA: es la
  desviación D-02 de §19 y es el descuadre contable más grave del
  microservicio. RegistrarPagoControlCommand, en cambio, NO lleva monto.
- ProcesarEvaluacionRegistradaCommandHandler, en este orden exacto: C10
  primero de todo, luego el tipo, luego la vigencia del catering, luego el
  catálogo. Si el duplicado se detectara al final, el reproceso dependería de
  que la vigencia no hubiera cambiado entre las dos entregas.
- El tipo de evaluación se compara con
  string.Equals(tipo, "CONTROL", StringComparison.OrdinalIgnoreCase). Comparar
  con == contra el literal hace que "Control" no genere cobro y responda 200:
  un fallo invisible (desviación D-07 de §19). Solo CONTROL cobra: INICIAL ya
  se facturó al contratar EVALUACION, y QUINCENAL no se cobra nunca (D-03).
- C7 y C10 no producen error: el camino "no hacer nada" responde 200.
- Las políticas de §8.2 traducen evento de dominio a evento de integración.
  PublicarServicioContratadoHandler toma pacienteNombre y nroIdentificacion de
  la réplica (C16) y anula duracionDias para todo tipo que no sea CATERING,
  dejando fechaInicio SIEMPRE con valor. PublicarFacturaEmitidaHandler aplana
  el Total en monto + moneda: publica el TOTAL, nunca el subtotal.
- ProcesarPlanAsignadoCommand incluye PlanId y lo guarda en la réplica: sin él
  no se distingue "reprocesan el mismo plan" de "llegó un plan distinto", y
  §9.15 del maestro lo declara parte de la clave de idempotencia (D-12).
```

### Los contratos de integración

`[CC]` · contexto: medio

```
Crea los contratos en Application/IntegrationEvents/, separando Entrantes/ y
Salientes/, uno por evento que este microservicio publica o consume, según
docs/DESIGN.md §9 y docs/SISTEMA.md §9.

Regla dura de SISTEMA.md §9: TODO campo listado, INCLUIDOS LOS OPCIONALES,
debe existir en el DTO del consumidor, con el mismo nombre y el mismo tipo,
aunque hoy no se use su valor.

Dos campos que se aceptan y se descartan, y que igualmente se declaran tipados:
- PacienteRegistrado.contacto es un objeto ANIDADO (telefono, email,
  direccion). No se aplana en campos sueltos.
- PlanAlimenticioAsignado.detalle es DiaDetalle[] con sus TRES niveles
  completos: DiaDetalle -> TiempoDetalle -> RecetaDetalle, con `dia` como fecha
  absoluta. Nada de JsonElement opaco: si el contrato cambia, tiene que romper
  la compilación, no fallar en ejecución.

Salientes:
- ServicioContratado: siete campos, duracionDias int? nulo fuera de CATERING,
  fechaInicio nunca nulo.
- FacturaEmitida: siete campos planos, monto y moneda separados, monto = Total.

Los tres endpoints de integración entrante son idempotentes: reprocesar el
mismo evento no duplica el efecto y responde 200.
```

### Los tests de Application

`[CC]` · contexto: medio

```
Escribe los tests de Application en tests/Contratos.Application.Tests siguiendo
la tabla de docs/DESIGN.md §15.2, que ya trae la clase y el nombre de cada
test. Repositorios fake en memoria: FakeContratoRepository,
FakeFacturaRepository, FakeControlAdicionalRepository,
FakeServicioCatalogoRepository, FakePacienteConocidoStore,
FakeIntegrationEventPublisher, FakeUnitOfWork. Sin PostgreSQL, sin
Testcontainers.

Obligatorios:
1. Un test por cada invariante de orquestación: C1, C2, C3, C4, C7, C8, C9,
   C10, C13 (monto exacto) y C16, afirmando el código de error exacto y su
   ErrorType. C5 se prueba comprobando que factura y contrato entran en el
   mismo commit.
2. Idempotencia de LOS TRES endpoints de integración entrante: reprocesar
   paciente-registrado y plan-asignado no duplica la réplica, y reprocesar
   evaluacion-registrada genera UN SOLO control. Añade además el test de que
   la segunda vez ni siquiera consulta la vigencia del catering.
3. RN-01b y RN-26 en el handler: que la vigencia se copia del plan y no del
   command, que la fechaInicio enviada por el cliente se ignora para catering,
   y que un cambio de precio posterior no altera un contrato ya emitido.
4. Que el total usa el porcentaje de FacturacionOptions y no un 13 fijo.
5. Un test de contrato por cada evento publicado y consumido (§15.3):
   serializa o deserializa el DTO y compara campo a campo contra la tabla del
   evento en SISTEMA.md §9, INCLUIDOS los opcionales y los tres niveles del
   detalle del plan.
```

`[TÚ-CHECK]` `dotnet build; dotnet test`
`[CC-SUB]` `Usa el subagente auditor-contratos.`
`[GATE]` Build y tests en verde · auditor sin discrepancias · diff leído.

---
## Fase 4 — Infrastructure

> **Ficha de la fase 4**
>
> | | |
> |---|---|
> | Modo | **Plan** para el plan de persistencia · **normal** para ejecutarlo |
> | Antes de empezar | `/clear` |
> | Reglas que se cargan solas | `.claude/rules/persistencia.md`, en cuanto se toca `src/*.Infrastructure/` |
> | Subagentes al cerrar | `revisor-ddd`, para comprobar que el mapeo no metió concesiones en el dominio |
> | Hooks que van a actuar | `bloquear-destructivos` deniega `ef database drop` y `ef migrations remove`. Si de verdad hace falta, lo corres tú a mano |
> | Lo que no se delega | **leer la migración generada entera** antes de aplicarla. Es el contrato con la base y lo más caro de revertir |
> | Cierra con | las tablas y los índices únicos existiendo en Postgres, y `dotnet test` todavía en verde |

### Persistencia

`[CC-PLAN]` · contexto: alto

```
Fase 4: capa Infrastructure de ms-contratos-facturacion.

Lee docs/DESIGN.md §14 completa (§14.1 a §14.5) y docs/SISTEMA.md §12.2 a
§12.6 y §12.12.

Propón el plan: ContratosDbContext con los cinco DbSet, una
IEntityTypeConfiguration por agregado más la del read model, los cuatro
repositorios con los métodos de §14.5, PacienteConocidoStore, UnitOfWork,
Infrastructure/Queries/ con los cinco handlers de query, y
LoggingIntegrationEventPublisher.

No escribas código todavía.
```

`[CC]` · contexto: medio

```
Ejecuta el plan. Reglas que docs/SISTEMA.md fija y que aquí se incumplieron
antes (§19):

- Money SIEMPRE con OwnsOne a DOS columnas, *_monto y *_moneda, jamás a una.
  Son ocho instancias de Money en cinco tablas: precio del catálogo, precio del
  servicio congelado, precio unitario de la línea, subtotal, impuesto y total
  de la factura, monto del pago y cobro del control.
- Factura.Lineas es la única colección: builder.HasMany(typeof(LineaFactura),
  "_lineas").WithOne().HasForeignKey("factura_id").OnDelete(Cascade) Y ADEMÁS
  builder.Ignore(f => f.Lineas) sobre la propiedad pública (§12.2). NADA de
  OwnsMany con clave sombra int autogenerada: es la desviación D-08 de §19 y
  contradice también ValueGeneratedNever() de §12.5.
- builder.Ignore(l => l.Subtotal) en LineaFactura: se calcula, no se persiste.
- builder.Ignore(x => x.DomainEvents) en todos los agregados, y
  Ignore<DomainEvent>() en el DbContext.
- ValueGeneratedNever() en las CINCO PK uuid, factura_lineas incluida.
- Índices de §14.3, con IsUnique() donde diga:
  * único filtrado (paciente_id, servicio_tipo) WHERE estado = 'ACTIVO' en
    contratos — respaldo físico de C3 / RN-19, obligatorio según §12.5. El
    código anterior lo creó y luego lo DEGRADÓ a no único porque un catering
    vencido bloqueaba la recontratación (D-05): la causa era la ausencia del
    cierre perezoso de C12, que ahora sí existe. El índice va único.
  * único (evaluacion_id) en controles_adicionales — respaldo de C10.
  * único filtrado (tipo) WHERE activo = true en servicios_catalogo (D-13). Y
    ServicioCatalogoRepository.GetByTipoAsync FILTRA por Activo: sin ese filtro
    el precio contratado depende del orden de inserción.
  * los seis de consulta: paciente_id y fecha_contratacion en contratos,
    paciente_id y referencia_id en facturas, paciente_id en controles. El
    código anterior no declaraba ninguno y las tres queries hacían recorrido
    secuencial (D-15).
- Ninguna FK entre agregados: contratos.factura_id, contratos.paciente_id,
  facturas.referencia_id y controles.factura_id son uuid sueltos. La única FK
  real de la base es factura_lineas.factura_id, que vive dentro del agregado.
- UnitOfWork: recoge los DomainEvents del ChangeTracker, los limpia, los
  publica por MediatR y DESPUÉS hace SaveChangesAsync (§12.6). El orden importa.
- PacienteConocidoStore persiste con su propio SaveChangesAsync, fuera del
  UnitOfWork: el read model no es un agregado y no emite eventos.
- FacturaRepository.GetByIdAsync hace Include(f => f.Lineas): una factura sin
  sus líneas no permite verificar C11.
- LoggingIntegrationEventPublisher: sus JsonSerializerOptions llevan
  PropertyNamingPolicy = JsonNamingPolicy.CamelCase ADEMÁS del
  JsonStringEnumConverter, para que el log coincida con el payload de
  SISTEMA.md §9, que es justo lo que ese log documenta.
- Las queries van en Infrastructure/Queries/ con AsNoTracking(), devuelven DTOs
  y convierten el Guid a su ID tipado en la PRIMERA línea, comparando tipos
  completos dentro del Where: nunca f.PacienteId.Value == guid (§12.5).
- GetContratosQuery filtra el período en SQL por fecha_contratacion y resuelve
  el estado EN MEMORIA con EstadoDerivado(hoy): la columna puede decir ACTIVO
  en un contrato ya vencido.
```

### La migración

`[CC]` · contexto: bajo

```
Genera la migración inicial:

dotnet ef migrations add InicialContratos --project src/Contratos.Infrastructure --startup-project src/Contratos.WebApi

No la apliques. Muéstrame el archivo generado.
```

`[TÚ]` **Lee la migración entera.** Es el contrato con la base de datos y lo más caro de revertir. Comprueba una por una:

- [ ] Las seis tablas y sus columnas en `snake_case`
- [ ] **Cada `Money` en dos columnas**: `*_monto numeric(18,2)` y `*_moneda varchar(3)`. Ocho pares en total
- [ ] Los tres índices únicos, dos de ellos **con filtro** (`WHERE estado = 'ACTIVO'`, `WHERE activo = true`)
- [ ] `factura_lineas.id` es `uuid` y **no** una columna de identidad `integer`
- [ ] Ninguna columna `subtotal` en `factura_lineas`
- [ ] Ninguna tabla fantasma de `DomainEvent` (si aparece, falta un `Ignore`)
- [ ] Los tipos: `date` para `DateOnly`, y **ninguna columna `jsonb`** — §12.4 del maestro no aplica en este BC

`[TÚ-CHECK]`

```powershell
dotnet ef database update --project src/Contratos.Infrastructure --startup-project src/Contratos.WebApi
docker exec nur-tricenter-postgres psql -U postgres -d contratos_db -c "\dt"
docker exec nur-tricenter-postgres psql -U postgres -d contratos_db -c "\di"
```

`[GATE]` Las seis tablas existen · los tres índices únicos existen y dos están filtrados · `dotnet test` sigue en verde.

---
## Fase 5 — WebApi

> **Ficha de la fase 5**
>
> | | |
> |---|---|
> | Modo | **Normal.** Aquí se ejecuta un diseño ya decidido: los endpoints los fija §11 del `DESIGN.md`. Entra en plan solo si vas a cambiar el diseño del middleware |
> | Antes de empezar | `/clear` |
> | Reglas que se cargan solas | ninguna |
> | Subagentes | ninguno obligatorio |
> | Hooks que van a actuar | `cierre-de-turno` con build y tests |
> | Cierra con | la API levantando en su puerto, Swagger con todos los endpoints de §11, y un POST y un GET reales contra PostgreSQL |

`[CC]` · contexto: medio

```
Fase 5: capa WebApi.

Lee docs/DESIGN.md §11 (API REST, §11.1 a §11.4) y docs/SISTEMA.md §12.1,
§12.10, §12.11 y §12.12.

Implementa:
- Controllers FINOS: reciben, mapean a Command o Query, delegan a MediatR y
  devuelven. Ninguna lógica de negocio, ningún if de dominio.
- Los endpoints exactos de §11, con los verbos, rutas y códigos de respuesta
  que ahí se declaran. TODO happy path devuelve 200, incluidos los POST de
  creación: nada de 201 ni de 204. DELETE /api/servicios/{id} DESACTIVA, no
  borra, y es idempotente. Los dos GET de paciente devuelven lista vacía y
  200, nunca 404.
- Middleware de excepciones en src/Contratos.WebApi/Middleware/. Captura
  DomainException y traduce Error.Type a HTTP según la tabla de §12.1:
  Validation/Failure->400, NotFound->404, Conflict->409, Problem->422.
  Formato de respuesta { codigo, mensaje }.
  Añade una rama para ArgumentException que devuelva 400 con el mismo formato:
  es la red de seguridad de INC-S2, porque Entity(Guid.Empty) del paquete
  lanza una excepción nativa que si no saldría como 500.
  El middleware va en WebApi, NO en Infrastructure, y por tanto Infrastructure
  no necesita FrameworkReference de ASP.NET.
- Program.cs como composition root: AddJsonOptions con JsonStringEnumConverter
  y camelCase (§12.11), Swagger, DbContext, repositorios, PacienteConocidoStore,
  UnitOfWork, FacturacionOptions ligado a la sección Facturacion, y MediatR
  escaneando DOS assemblies: Contratos.Application y Contratos.Infrastructure.
  Sin el segundo escaneo toda consulta responde "No handler registered".
- Los commands usan el enum TipoServicio tipado, no un string parseado con
  Enum.Parse: un Enum.Parse sin TryParse lanza ArgumentException, que el
  middleware no captura y sale como 500 en vez de 400 (desviación D-22 de §19).
- Aplicación automática de migraciones al arrancar SOLO en Development,
  resolviendo el migrador desde un scope antes del pipeline HTTP (§12.12).
```

`[TÚ-CHECK]`

```powershell
dotnet run --project src/Contratos.WebApi
```

Abre <http://localhost:5030/swagger>. Tienen que aparecer todos los endpoints de §11, los cinco controllers.

`[TÚ-CHECK]` Prueba a mano los dos casos feos, los que más se rompen:

```
POST /api/servicios  con {"tipo":"CATERING", ...}   → no debe dar 400 por el enum
GET  /api/servicios                                  → no debe dar "No handler registered"
```

Si el primero da 400 antes de llegar al handler, falta el `JsonStringEnumConverter` en `AddJsonOptions`. Si el segundo falla, falta el segundo assembly en el registro de MediatR.

`[GATE]` La API levanta · Swagger responde · crear un servicio, contratar y consultarlo funcionan contra PostgreSQL real.

---
## Fase 6 — Demo y README

> **Ficha de la fase 6**
>
> | | |
> |---|---|
> | Modo | **Normal** |
> | Antes de empezar | `/clear` |
> | Subagentes | ninguno |
> | Lo que no se delega | correr el `demo.http` entero, de arriba abajo, contra base limpia. En trozos no vale: el orden es parte de lo que demuestra |
> | Después | `/defensa`, y responder de verdad, sin leer |
> | Cierra con | la checklist de entrega de la Actividad 4, completa |

El `demo/demo.http` **es** la demostración de la defensa. Que cubra de verdad lo que dices que cubre.

`[CC]` · contexto: medio

```
Fase 6: escribe demo/demo.http con el flujo completo de
ms-contratos-facturacion, según docs/DESIGN.md §12 (flujos principales) y §17.

Estructura:
1. Crear los tres artículos del catálogo: EVALUACION, ASESORAMIENTO, CATERING
   (HU-20)
2. Crear un servicio con "moneda": "EUR" → 400 C18_MONEY_MONEDA_INVALIDA
3. Contratar para un paciente que nadie registró → 400 C1_PACIENTE_DESCONOCIDO
4. POST /api/integracion/paciente-registrado con el contacto ANIDADO, enviado
   DOS VECES para demostrar idempotencia → 200 las dos
5. Contratar CATERING todavía sin plan → 400 C2_SIN_PLAN_ACTIVO
6. POST /api/integracion/plan-asignado con duracionDias 15 y el detalle
   completo de tres niveles con fechas absolutas
7. Contratar CATERING SIN ENVIAR FECHAS (HU-19) → 200. Comprobar en la
   respuesta y en el GET que la vigencia es la del plan y que FechaFin es
   FechaInicio + 14, y en el log que FacturaEmitida lleva el Total con el
   13 por ciento aplicado
8. Contratar CATERING otra vez para el mismo paciente → 409
   C3_CATERING_YA_ACTIVO
9. POST /api/contratos/{id}/pago con un monto distinto del total → 400
   C13_PAGO_NO_COINCIDE_CON_TOTAL
10. El mismo pago con el total exacto y su misma moneda → 200. Repetirlo → 409
    C13_PAGO_YA_REGISTRADO
11. POST /api/contratos/{id}/cancelar sobre ese contrato ya pagado → 200, y el
    GET del contrato muestra CANCELADO con la factura todavía PAGADA (C8)
12. Segundo paciente: plan-asignado con duracionDias 22 y contratar CATERING →
    400 C4_DURACION_INVALIDA
13. POST /api/integracion/evaluacion-registrada tipo CONTROL para el paciente
    CON catering vigente → 200 y NO se crea control (RN-06)
14. Lo mismo tipo QUINCENAL para un paciente SIN catering → 200 y tampoco se
    crea nada
15. Lo mismo tipo CONTROL para un paciente SIN catering → 200 y se crea el
    control con su factura de origen CONTROL_ADICIONAL
16. Repetir el paso 15 con el MISMO evaluacionId → 200 y sigue habiendo UN
    solo control y UNA sola factura (C10)
17. POST /api/controles/{id}/pago → 200. Repetirlo → 409 C21_CONTROL_YA_PAGADO
18. GET /api/pacientes/{pacienteId}/facturas → las de contratación y las de
    control juntas, distinguibles por `origen` (HU-21)
19. GET /api/contratos?desde=&hasta=&estado=ACTIVO → filtra por estado
    DERIVADO (HU-23)
20. GET /api/pacientes/{pacienteId}/controles → un solo control, pagado
    (HU-22)

Usa @baseUrl = http://localhost:5030 y variables entre peticiones.
Cada bloque lleva un comentario diciendo qué demuestra y qué HU o invariante
cubre, y los pasos negativos indican el código de error y el HTTP esperados.
```

`[TÚ-CHECK]` **Corre el `demo.http` entero, de arriba abajo, contra la base limpia.** No en trozos. Tres pasos que hay que mirar con lupa porque son los que revelan los errores caros:

- Paso 7: si `vigencia_fecha_fin` es `FechaInicio + 15` en vez de `+ 14`, el `−1` de C17 no está.
- Paso 10: si un pago de monto distinto pasó en el paso 9, C13 no está implementada y tienes el descuadre de D-02.
- Paso 16: si aparecen dos controles o dos facturas, C10 no está funcionando y lo vas a descubrir ahora y no delante del docente.

`[CC]` · contexto: bajo

```
Actualiza el README.md para la entrega de la Actividad 4. Sobre lo que ya
tenía de la Actividad 3, añade:

- Arquitectura: las cuatro capas y la regla de dependencias, con un diagrama
  Mermaid.
- Los endpoints de la API, agrupados por caso de uso, con su HU.
- Los eventos de integración: los dos que publica y los tres que consume, con
  su origen o destino.
- Cómo correr la demo: docker compose up -d, dotnet run, abrir demo/demo.http.

El diagrama de clases del dominio se MANTIENE: la Actividad 4 lo sigue
pidiendo, y sigue siendo solo de la capa de dominio de este microservicio.
```

---
## La defensa

### Ensayo

`[CC]` `/defensa` — responde de verdad, no leyendo. Si una respuesta no te sale, esa parte del código todavía no es tuya.

### Las preguntas que caen, con la respuesta corta

**«¿Cuatro agregados no son demasiados?»**
Son los que el enunciado exige. El catálogo tiene ciclo de vida propio e independiente de cualquier contrato (RN-26); la factura nace de dos orígenes, uno de los cuales no tiene contrato (RN-06); y el control adicional existe precisamente cuando no hay contrato de catering. §5.2.

**«¿Por qué `Factura` es un agregado propio y no una entidad hija de `Contrato`?»**
Porque una factura nace de una contratación **o** de un control adicional, y el control adicional aparece cuando no hay catering contratado. Dentro de `Contrato` no habría agregado al que colgar la factura de un control: haría falta un contrato ficticio de importe cero. Además tiene su propio ciclo `EMITIDA → PAGADA | ANULADA`, independiente del contrato. §5.2.

**«Enséñame dónde se comprueba que un paciente no tenga dos cateringes activos.»**
En `ContratarServicioCommandHandler`, C3. No está en el agregado a propósito: es una regla sobre **el conjunto** de contratos del paciente, y ningún agregado ve más allá de sus propios límites. Y hay un segundo nivel: el índice único filtrado `(paciente_id, servicio_tipo) WHERE estado = 'ACTIVO'`, que cubre la carrera entre dos contrataciones concurrentes que el handler no puede ver. §6.1 y §14.3.

**«¿Y eso no es lógica de negocio fuera del dominio?»**
El `Error` está catalogado con el prefijo `C` porque el vocabulario del rechazo es vocabulario de dominio. Lo que está en el handler es la **orquestación**: consultar el conjunto que la regla necesita mirar. §12.1 del maestro lo admite explícitamente.

**«Modificas dos agregados en una sola transacción. ¿No rompe eso la heurística de DDD?»**
Sí, y es deliberado. RN-05 dice «facturado al momento de la contratación»: no debe existir un contrato sin factura **ni por un instante**. La regla de negocio pesa más que la heurística de diseño. Lo que sí es eventual —y por eso viaja por eventos— es la reacción de los otros bounded contexts. §5.2 y §18.

**«¿Por qué el cliente no manda el total de la factura?»**
Porque `Factura.Emitir` recibe **líneas y un porcentaje**, no importes: los tres se calculan dentro del agregado (C11, RN-25). Si el total se pudiera enviar, se podría emitir una factura cuyo total no corresponde a sus líneas, y eso se descubre cuando ya se cobró. Lo mismo con la vigencia del catering, que se deriva del plan, y con el precio, que se copia del catálogo: en los tres casos el cliente HTTP no tuvo forma de aportar el dato.

**«¿Por qué una evaluación quincenal no se cobra nunca, ni siquiera sin catering?»**
Porque una quincenal solo existe dentro de un catering vigente: BC1 lo garantiza con su invariante `P7_QUINCENAL_REQUIERE_CATERING`. Una quincenal sin catering es un dato inconsistente del upstream, no una venta. Y RN-06 se lee de forma binaria: con catering vigente en la fecha no se cobra, sin contar evaluaciones. §6.2.

**«¿Qué pasa si llega dos veces el mismo `EvaluacionRegistrada`?»**
Nada, a propósito. C10 comprueba `ExistePorEvaluacionAsync` **como primera instrucción del handler**, antes de mirar la vigencia o el catálogo, y responde 200 sin efecto; un índice único sobre `evaluacion_id` lo respalda. Si el duplicado se detectara al final, el reproceso dependería de que la vigencia no hubiera cambiado entre las dos entregas. Está en el `demo.http`, pasos 15 y 16.

**«¿Por qué los handlers de las queries están en `Infrastructure`?»**
Desviación consciente del reparto canónico, documentada en `SISTEMA.md` §12.8: necesitan LINQ y EF Core directos, y meterlos en `Application` obligaría a esa capa a conocer el ORM. El `record` de la query sí vive en `Application`, que es lo que preserva el contrato.

**«¿Cómo garantizas la consistencia al guardar?»**
`UnitOfWork.CommitAsync` recoge los eventos de dominio del `ChangeTracker`, los publica por MediatR y luego hace `SaveChangesAsync`. Las dos políticas son traductoras puras, así que el orden no las afecta. El read model `PacienteConocido` va aparte, con su propio `SaveChangesAsync`, porque no es un agregado y no emite eventos. §12.6 y §5.7.

**«¿Por qué no separaste el modelo de persistencia del de dominio, como el ejemplo?»**
→ §5.3, abajo. Es la pregunta con más filo.

**«¿Qué harías distinto con más tiempo?»**
Tests de `Infrastructure` contra PostgreSQL real. `SISTEMA.md` §13.4 lo señala como el hueco más importante, y aquí hay tres cosas que **solo** fallan contra base real: los tres índices únicos, la regla de clonado de owned types —un `Money` compartido deja columnas en `null` sin que ningún test en memoria lo note— y el error de IDs tipados en LINQ. Y el hueco funcional conocido: cancelar un catering no cancela hoy el calendario de BC4, porque ese evento de integración no existe en `SISTEMA.md` §9 y los contratos no se amplían de forma unilateral. §20.

### La pregunta del modelo de persistencia

El repositorio de ejemplo del docente separa **dos `DbContext` sobre las mismas tablas**: `PersistenceDbContext`, con POCOs planos y anotaciones, dueño del esquema y de las migraciones; y `DomainDbContext`, que mapea el dominio rico con Fluent API y es el que usan repositorios y `UnitOfWork`. Este proyecto hace lo otro: **mapeo directo**, un solo `DbContext`.

Respuesta preparada:

> Conocemos el patrón: separar el modelo de datos del modelo de dominio, que es la posición purista de DDD. Elegimos el mapeo directo porque EF Core ya nos da owned types, `ValueConverter` y backing fields, así que el dominio no necesita hacer concesiones estructurales, solo un constructor sin parámetros. La separación aporta cuando la base es legada, la comparte otro sistema o la impone un DBA: ahí el esquema no lo controlas y necesitas un modelo propio que lo refleje. Aquí controlamos ambos, y el coste de la separación —cada campo declarado dos veces, y una desincronización que falla en runtime y no en compilación— no compensa. Está registrado como decisión con su alternativa descartada en `SISTEMA.md` §12.

Lo que **no** hay que hacer: decir que no lo conocías, o que «el mapeo directo es lo correcto». Una decisión sin alternativa descartada no es una decisión: es una descripción.

### El día de la demo

`[TÚ]` Media hora antes:

```powershell
docker compose ps                                   # healthy
dotnet build                                        # sin warnings en pantalla
dotnet test                                         # verde
dotnet run --project src/Contratos.WebApi           # y dejarlo corriendo
```

- Base **limpia** o con datos coherentes. Un `demo.http` que falla en el paso 8 porque el catering ya existe de un ensayo anterior es el clásico; y con el índice único filtrado activo, un catering vencido sin cerrar te bloquea la recontratación.
- Swagger abierto en una pestaña, `demo.http` en otra, el `DESIGN.md` en una tercera, y **la consola donde salen los eventos en JSON en una cuarta**: `ServicioContratado` y `FacturaEmitida` son media demo.
- Ten a mano el archivo donde vive cada invariante. «¿Dónde está C7?» se responde abriendo el archivo, no buscándolo.

---
## Checklist de entrega — Actividad 4

- [ ] Las cuatro capas, con la regla de dependencias intacta
- [ ] `Infrastructure` sin `FrameworkReference` de ASP.NET; el middleware en `WebApi`
- [ ] Las veintidós invariantes, cada una donde §6 dice
- [ ] Tests: uno por invariante de dominio · las once de orquestación · idempotencia de los **tres** endpoints · un test de contrato por evento
- [ ] Los contratos de integración con **todos** los campos de `SISTEMA.md` §9, opcionales incluidos, con `contacto` anidado y el `detalle` del plan en sus tres niveles
- [ ] Migración aplicada, con los tres índices únicos —dos de ellos filtrados—, los seis de consulta, cada `Money` en dos columnas y sin tablas fantasma
- [ ] Serialización: enums como string en mayúsculas y camelCase, en `Program.cs` **y** en el publicador
- [ ] MediatR escanea `Application` **e** `Infrastructure`
- [ ] La API levanta en **5030** y `demo/demo.http` apunta al mismo puerto
- [ ] `demo.http` corre entero contra base limpia, casos negativos incluidos
- [ ] `README.md` con descripción, funcionalidades, diagrama de clases del dominio renderizando en GitHub y puesta en marcha
- [ ] `docs/INCOHERENCIAS.md` sin entradas ABIERTAS
- [ ] Repositorio propio, historial legible

`[GATE]` Si algo de esta lista no está, todavía no se entrega.
