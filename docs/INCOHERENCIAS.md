# Incoherencias detectadas durante la construcción

> Protocolo en `CLAUDE.md`. **Claude Code no resuelve incoherencias: las reporta y para.**
> El estado solo lo cambio yo. El hook `SessionStart` cuenta las ABIERTAS al abrir sesión y
> `cierre-de-turno` lo recuerda al final de cada turno.

Las incoherencias del proyecto se cerraron antes de empezar: están en
`docs/DECISIONES.md`. Este archivo es para las que aparezcan
**escribiendo código**, y empieza vacío a propósito.

---

### INC-1 · Dónde se declaran las interfaces de repositorio específicas (Domain vs Application)
- **Detectada en:** fase 2, preparando el plan de construcción de `src/Contratos.Domain`.
- **Fuente A dice:** `CLAUDE.md`, sección "Joseco.DDD.Core": «`IRepository<T>` solo tiene `GetByIdAsync` y `AddAsync`. Lo demás va en `IContratoRepository`, `IFacturaRepository`, `IControlAdicionalRepository` e `IServicioCatalogoRepository`, declaradas en Domain (§14.5).»
- **Fuente B dice:** `docs/DESIGN.md` §17, Fase 3 — Application, paso 2: «Interfaces de repositorio específicas de §14.5» (listado como entregable de Application, no de Domain). `docs/GUIA.md`, apartado "Fase 2 — El dominio": ni el checklist `[TÚ]` ni el gate de cierre mencionan ninguna interfaz de repositorio entre los archivos de Domain de esta fase. `docs/DESIGN.md` §14.5 en sí mismo no fija el proyecto, solo describe los métodos.
- **Por qué no puedo continuar:** decide si el plan de `src/Contratos.Domain` de la fase 2 debe incluir `IContratoRepository`, `IFacturaRepository`, `IControlAdicionalRepository` e `IServicioCatalogoRepository` como archivos de esta fase, o si son un entregable de la fase 3 que todavía no toca.
- **Opciones:** A) Seguir `CLAUDE.md`: declararlas en `src/Contratos.Domain` ahora, en fase 2. B) Seguir `docs/DESIGN.md` §17 y `docs/GUIA.md`: dejarlas para la fase 3 (Application), sin tocarlas en esta fase. C) Declararlas "en Domain" pero no escribirlas hasta fase 3 — no resuelve la contradicción, solo la pospone y deja el mismo dilema para entonces.
- **Mi recomendación y por qué:** B. `CLAUDE.md` es una guía y el propio `CLAUDE.md` fija que "una guía no manda sobre DESIGN.md: si la contradice, es una incoherencia". Además `docs/GUIA.md` —el procedimiento operativo con su checklist de cierre explícito para esta fase— no las incluye en ningún punto de la fase 2, y la regla de dependencias (Domain ← Application ← Infrastructure) no se rompe si el "puerto" vive en Application: Application ya referencia Domain, e Infrastructure puede implementar una interfaz declarada en Application sin invertir la flecha.
- **Impacto si se elige mal:** si se elige A y la fase 3 las reescribe en Application, queda código muerto en Domain o hay que migrar tipos a media construcción, con riesgo de dos declaraciones incompatibles de la misma interfaz. Si se elige B y en realidad debían ir en Domain, la fase 4 (Infrastructure) referenciaría tipos que `CLAUDE.md` espera en otro sitio, y `verificador-invariantes` podría señalar como incompleto algo que pertenece a otra fase.
- **Resolución:** Opción B. `IContratoRepository`, `IFacturaRepository`,
  `IControlAdicionalRepository` e `IServicioCatalogoRepository` se declaran en
  `Contratos.Application` (fase 3), no en `Contratos.Domain` (fase 2).
  Propagado a `CLAUDE.md` (sección "Joseco.DDD.Core") y a `docs/DESIGN.md`
  §14.5, que ahora fija explícitamente el proyecto. `docs/GUIA.md` (fase 2 y
  fase 3) y `docs/DESIGN.md` §17 fase 3 paso 2 ya estaban escritos así; no
  necesitaron cambio.
- **Estado:** RESUELTA — opción B

---

### INC-2 · `Handle_SegundaEvaluacionActivaElMismoDia_FallaConC3` contradice el alcance ya cerrado de C3
- **Detectada en:** fase 3, escribiendo `ContratarServicioCommandHandlerTests` según la tabla de `docs/DESIGN.md` §15.2.
- **Fuente A dice:** `docs/DESIGN.md` §6, fila C3: «Máximo **un contrato `CATERING` `ACTIVO` por paciente**... **No aplica a `EVALUACION` ni a `ASESORAMIENTO`**: RN-19 exige la unicidad solo para catering, y un paciente puede contratar varias evaluaciones (§6.1, INC-S10).» Y §13.1: «~~`C3_CONTRATO_YA_ACTIVO`~~ **Retirado (INC-S10).** C3 aplica solo a `CATERING`... El código **no se reutiliza**.» `SISTEMA.md` RN-19 (§ tabla de reglas de negocio) tampoco menciona límite alguno para `EVALUACION`: «Un paciente tiene como máximo 1 plan alimenticio activo, 1 contrato de catering activo y 1 calendario de entrega vigente a la vez.»
- **Fuente B dice:** `docs/DESIGN.md` §15.2, fila «Validaciones que cruzan agregados o repositorio»: la lista de tests obligatorios de `ContratarServicioCommandHandlerTests` incluye literalmente `Handle_SegundaEvaluacionActivaElMismoDia_FallaConC3`, con el mismo sufijo `_FallaConC<n>` que usan todos los demás tests de la fila para afirmar que el handler lanza ese error (p.ej. `Handle_PacienteDesconocido_FallaConC1`).
- **Por qué no puedo continuar:** ese nombre de test solo tiene sentido si contratar una segunda `EVALUACION` para el mismo paciente el mismo día lanza `C3_CATERING_YA_ACTIVO` (o un sucesor). Pero §6 y §13.1 ya cerraron explícitamente —citando INC-S10— que C3 no aplica a `EVALUACION`/`ASESORAMIENTO`, y el código de `ContratarServicioCommandHandler` (ya escrito) tampoco comprueba unicidad para esos tipos: solo llama a `ExisteCateringActivoParaPacienteAsync` dentro del bloque `if (TipoServicio == CATERING)`. Escribir el test tal como está nombrado exigiría o (a) inventar una invariante nueva no catalogada, o (b) reinterpretar el nombre en contra de su lectura literal. Ninguna de las dos es mía para decidir.
- **Opciones:** A) Tratar el nombre como un error de redacción en la tabla de §15.2 y no escribir ese test (o renombrarlo a un escenario real de C3, como el solape de dos cateting). B) Añadir de verdad una invariante «como máximo una `EVALUACION` activa por paciente el mismo día» a §6 y a `ContratarServicioCommandHandler`, revirtiendo la resolución de INC-S10. C) Escribir el test tal cual pero haciendo que afirme que el handler NO falla (reinterpretando `FallaConC3` como error de nomenclatura, no de comportamiento).
- **Mi recomendación y por qué:** A. INC-S10 ya se cerró con cita textual y con el retiro explícito de `C3_CONTRATO_YA_ACTIVO` «el código no se reutiliza»; revertirlo (opción B) contradice una decisión ya tomada sin que nada en el enunciado del cliente lo pida, y `SISTEMA.md` RN-19 tampoco limita las evaluaciones. La opción C falsearía el nombre del test sin corregir la tabla, dejando la incoherencia agazapada para el próximo lector.
- **Impacto si se elige mal:** si se elige B sin querer, se añade una restricción comercial que el enunciado no exige («un paciente puede contratar varias evaluaciones») y que rompe HU-19 tal como está probada hoy. Si se elige C, el nombre del test miente sobre lo que prueba y cualquiera que lo lea de pasada (o el `auditor-contratos`) puede reintroducir la lógica de bloqueo por error.
- **Impacto de dejarlo aislado:** el resto de `ContratarServicioCommandHandlerTests` (C1, C2, C4, C9, catálogo mal configurado, C12+C3 de catering, RN-01b/RN-26, C5, tasa configurable) no depende de este punto y se escribe igual.
- **Resolución:** Opción A. `docs/DESIGN.md` §15.2 reemplaza
  `Handle_SegundaEvaluacionActivaElMismoDia_FallaConC3` por
  `Handle_SegundoCateringConVigenciaQueSolapaEnFechaDistinta_FallaConC3`, un
  escenario real de C3: dos contratos de `CATERING` cuya vigencia solapa
  aunque no compartan `FechaInicio`. El test se agregó a
  `ContratarServicioCommandHandlerTests`.
- **Estado:** RESUELTA — opción A

---

### INC-3 · `Moneda` es enum en SISTEMA.md §8 pero `FacturaEmitida.moneda` se tipa `string` en SISTEMA.md §9.3
- **Detectada en:** fase 3, auditoría del subagente `auditor-contratos` sobre los eventos de integración al cerrar los tests de Application.
- **Fuente A dice:** `docs/SISTEMA.md` §8 (tipos compartidos): cataloga `Moneda` como enumeración cerrada `{BOB, USD}`, propiedad de BC3.
- **Fuente B dice:** `docs/SISTEMA.md` §9.3, tabla de campos de `FacturaEmitida`: «`moneda` | `string` | `BOB` o `USD`» — tipa el campo explícitamente como `string`, no como el enum `Moneda` de §8.
- **Por qué no puedo continuar (parcial):** no bloquea código propio de este microservicio, porque `docs/DESIGN.md` §9.1 y el DTO `FacturaEmitidaIntegrationEvent.Moneda` (`string`) ya siguen fielmente la tabla literal de §9.3, que es la fuente más específica y la que de hecho define el JSON que viaja por el bus. No hay una decisión de este repositorio que dependa de resolver esta contradicción. Se registra igualmente porque el protocolo de `CLAUDE.md` exige registrar toda contradicción detectada entre fuentes, y esta vive dentro del propio `SISTEMA.md`, el maestro, que «no se edita aquí».
- **Opciones:** A) Dejar constancia aquí y no tocar nada: `DESIGN.md` y el código siguen la tabla específica de §9.3, que es la que un consumidor real deserializa. B) Proponer en el maestro (fuera de este repositorio) que §9.3 use el enum `Moneda` de §8 en vez de `string`, por consistencia de tipos. C) Proponer que §8 aclare que `Moneda` es un enum de dominio interno y que en los contratos de evento viaja siempre como `string` (como ya ocurre con otros enums, vía `JsonStringEnumConverter`, aunque aquí el campo nunca fue tipado como enum en el contrato).
- **Mi recomendación y por qué:** A, para este repositorio: `docs/SISTEMA.md` es una copia literal que «no se edita aquí» (regla del propio `CLAUDE.md`), así que la reconciliación de §8 vs §9.3 le corresponde a quien mantiene el maestro, no a este microservicio. El código ya está alineado con la fuente más específica.
- **Impacto si se elige mal:** ninguno inmediato sobre este repositorio — es una anotación informativa. El riesgo real es documental: si en el futuro alguien "corrige" `FacturaEmitidaIntegrationEvent.Moneda` a tipo enum guiándose por §8 en vez de por §9.3, rompería el contrato de wire actual sin que el maestro lo haya pedido.
- **Resolución:** Opción A. No se toca `SISTEMA.md` (copia literal, no se
  edita aquí) ni el código, que ya sigue la tabla específica de §9.3. Se
  agregó una nota en `docs/DESIGN.md` §9.1 (descripción de `FacturaEmitida`)
  explicando por qué `moneda` es `string` y no el enum `Moneda` de §5.5, para
  que el próximo lector no lo "corrija" por error.
- **Estado:** RESUELTA — opción A

---

### INC-4 · La tabla de `DESIGN.md` §9.2 tipa `EvaluacionRegistrada.tipo` como `TipoEvaluacion`, pero §5.5/§5.6 del mismo documento (y el código) lo tratan como `string`
- **Detectada en:** fase 3, segunda pasada del subagente `auditor-contratos` al cerrar `tests/Contratos.Application.Tests/`.
- **Fuente A dice:** `docs/DESIGN.md` §9.2, tabla de `EvaluacionRegistrada` (línea 940): «`tipo` | `TipoEvaluacion` | Primera condición de C7: solo `CONTROL` puede generar cobro».
- **Fuente B dice:** `docs/DESIGN.md` §5.5 (línea 539 y 543): «`TipoEvaluacion` | `INICIAL`, `QUINCENAL`, `CONTROL` | BC1 (§8 del maestro) | **Consumido, no modelado.** Llega como campo `tipo` de `EvaluacionRegistrada`...» y «**`TipoEvaluacion` no se modela como enum del dominio de BC3 a propósito**... La comparación se hace sobre el string entrante, sin distinguir mayúsculas». El código coincide con la fuente B: `EvaluacionRegistradaIntegrationEvent.Tipo` es `string`, `ProcesarEvaluacionRegistradaCommand.Tipo` es `string`, y el handler compara con `string.Equals(request.Tipo, "CONTROL", StringComparison.OrdinalIgnoreCase)` — nunca `Enum.Parse<TipoEvaluacion>`.
- **Por qué no puedo continuar (parcial):** no bloquea código de esta fase — el código, los tests (`EvaluacionRegistradaJsonTests`) y la justificación de §5.5/§5.6 son coherentes entre sí y correctos. La única pieza que no encaja es la celda de la tabla de §9.2, que es la tabla que `docs/GUIA.md` fase 3 cita como fuente para construir los contratos de integración. Corregirla yo mismo sería tocar `DESIGN.md` sin aprobación, y `CLAUDE.md` exige registrar, no resolver.
- **Opciones:** A) Corregir la celda de §9.2 de `TipoEvaluacion` a `string`, con una nota que remita a §5.5/§5.6 (alinea la tabla con la decisión ya tomada y con el código). B) Dejarla como está y confiar en que §5.5/§5.6 prevalecen por ser más específicas y explícitas. C) Revertir la decisión de §5.5/§5.6 y modelar `TipoEvaluacion` como enum en BC3 (contradice el razonamiento ya dado: crearía un segundo propietario del mismo enum).
- **Mi recomendación y por qué:** A. Es una corrección de redacción, no una decisión de diseño nueva: §5.5/§5.6 ya explican el porqué con detalle, el código y los tests ya la implementan así, y dejar la tabla de §9.2 sin corregir es la clase de discrepancia que confunde a la próxima persona que lea solo esa tabla (como estuvo a punto de pasar en esta auditoría).
- **Impacto si se elige mal:** si se elige C, se revierte una decisión ya razonada sin necesidad y se acopla BC3 al vocabulario de BC1. Si se deja sin corregir (ninguna de las tres), la tabla de §9.2 sigue siendo una fuente de confusión para quien la use como referencia rápida en una fase futura (p.ej. al escribir el controller de WebApi).
- **Resolución:** Opción A. La celda de `docs/DESIGN.md` §9.2 (`EvaluacionRegistrada.tipo`)
  se corrigió de `TipoEvaluacion` a `string`, con una nota que remite a §5.5/§5.6.
- **Estado:** RESUELTA — opción A

---

### INC-5 · `DESIGN.md` §17 etiqueta como C6 el mismo escenario que §6/§12.3 documentan como C8

- **Detectada en:** fase 6, preparando los comentarios de `demo/demo.http` según §12 y §17.
- **Fuente A dice:** `docs/DESIGN.md` §17, Fase 6, ítem 1: «`demo/demo.http` con el flujo completo y los casos negativos: C1 (paciente desconocido), C2 (catering sin plan), C3 (segundo catering), C4 (duración fuera de 15/30), **C6 (anular una factura pagada)**, C10 (reprocesar la misma evaluación) y moneda inválida (C18).»
- **Fuente B dice:** `docs/DESIGN.md` §6, fila C8: «Cancelar un contrato anula su factura **solo si está `EMITIDA`**. Una factura `PAGADA` permanece `PAGADA`.» — Handler `CancelarContratoCommandHandler`, sin error asociado («rama condicional, no error»). Y §12.3, Caso B: «`H-&gt;&gt;H: Factura.Estado es PAGADA, no es EMITIDA` — Nota: **C8** — no se llama a Anular, revertir un cobro exige nota de crédito» → `200 OK`, contrato `CANCELADO`, factura sigue `PAGADA`. Confirmado en código: `Contrato.RegistrarPago` (`src/Contratos.Domain/Contratos/Contrato.cs`) corta con C14 si el contrato no está `ACTIVO`, y `CancelarContratoCommandHandler` solo invoca `Factura.Anular()` cuando la factura está `EMITIDA`; `C6_ESTADO_FACTURA_INVALIDO` (`src/Contratos.Domain/Facturas/Factura.cs`, `MarcarPagada`/`Anular`) no es alcanzable desde ningún endpoint HTTP existente.
- **Por qué no puedo continuar (parcial):** no bloqueó el trabajo de esta fase — el flujo de 20 pasos que se me pidió ya sigue la interpretación C8 (paso 11: cancelar un contrato ya pagado → 200, factura sigue `PAGADA`), consistente con §6/§12.3. Se registra porque §17 del mismo documento nombra ese escenario con el código equivocado, y el protocolo exige registrar toda contradicción detectada aunque no bloquee.
- **Opciones:** A) Corregir la etiqueta de §17 de "C6" a "C8 (cancelar un contrato con factura ya pagada)", sin tocar comportamiento ni código — es una corrección de redacción. B) Dejar §17 como está, confiando en que §6/§12.3 prevalecen por ser más específicas. C) Implementar un escenario real que dispare C6 (por ejemplo, exponer una operación que invoque `Factura.Anular()`/`MarcarPagada()` sobre una factura no `EMITIDA`), y usarlo en el `demo.http` en vez de citar C8 en el paso 11.
- **Mi recomendación y por qué:** A. Es del mismo tipo que INC-2 e INC-4, ya resueltas con la opción A: una celda o etiqueta mal cruzada entre dos invariantes reales y ya documentadas, sin que el código ni el comportamiento deban cambiar. C6 sigue siendo una invariante de dominio legítima (defensa en profundidad en `Factura`), solo que no es la que se ejercita al cancelar un contrato ya pagado.
- **Impacto si se elige mal:** si se elige C, se inventa un caso de uso o un endpoint que el enunciado no pide solo para poder disparar C6 por HTTP, añadiendo superficie de API no solicitada. Si se deja sin corregir (ninguna de las tres), quien lea solo §17 en la próxima fase o en la defensa puede buscar —o peor, esperar del alumno— un 409 `C6_ESTADO_FACTURA_INVALIDO` donde el sistema, correctamente, responde 200.
- **Resolución:** Opción A. `docs/DESIGN.md` §17, Fase 6, ítem 1, se corrigió
  de «C6 (anular una factura pagada)» a «C8 (cancelar un contrato con
  factura ya pagada)». No se tocó comportamiento ni código: `C6_ESTADO_FACTURA_INVALIDO`
  sigue viva en `Factura.MarcarPagada`/`Anular` como invariante de dominio,
  simplemente no es la que corresponde citar en este escenario. `demo/demo.http`
  paso 11 ya citaba el código correcto (C8).
- **Estado:** RESUELTA — opción A

---

<!-- Formato de cada entrada:

### INC-1 · <título en una línea>
- **Detectada en:** <fase / archivo / momento>
- **Fuente A dice:** <cita textual + §sección>
- **Fuente B dice:** <cita textual + §sección>
- **Por qué no puedo continuar:** <qué decisión concreta bloquea>
- **Opciones:** A) … B) … C) …
- **Mi recomendación y por qué:** <una, con motivo técnico>
- **Impacto si se elige mal:** <qué se rompe después>
- **Estado:** ABIERTA

-->
