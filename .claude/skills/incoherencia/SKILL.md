---
name: incoherencia
description: Registra una incoherencia entre SISTEMA.md, DESIGN.md, las guias y el codigo
argument-hint: "[descripcion breve]"
disable-model-invocation: true
---

He detectado una posible incoherencia: **$ARGUMENTS**

1. Localiza las dos afirmaciones que chocan y cítalas textualmente con su número de sección.
   Las fuentes posibles son `docs/SISTEMA.md`, `docs/DESIGN.md`, las guías de
   `docs/GUIA.md` y el código de este repositorio.
2. Di cuál manda según la jerarquía del `CLAUDE.md`: SISTEMA.md sobre DESIGN.md, DESIGN.md
   sobre el código, y una guía **nunca** sobre la especificación —la guía dice cómo, la
   especificación dice qué—.
3. Si el punto toca código ya escrito, dime qué hace el código hoy.
4. Añade la entrada a `docs/INCOHERENCIAS.md` con el formato del `CLAUDE.md`, con el
   siguiente número correlativo y estado ABIERTA.
5. Dame las opciones y tu recomendación con su motivo técnico.

No la resuelvas. No cambies código por esto.
