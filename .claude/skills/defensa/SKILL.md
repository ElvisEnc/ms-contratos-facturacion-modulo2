---
name: defensa
description: Genera preguntas de defensa sobre el codigo realmente escrito, usando el banco de preguntas de la guia, y evalua mis respuestas
disable-model-invocation: true
allowed-tools: Read Grep Glob
---

Prepárame para la defensa de este microservicio.

Primero lee, en este orden:

- `docs/GUIA.md`, el apartado «La defensa»: trae el **banco de preguntas que caen con su
  respuesta corta**, y la pregunta sobre el modelo de persistencia.
- `docs/GUIA.md`, los apartados «Dónde se gana la nota en este dominio» y «Errores ya
  cometidos en este dominio».
- `docs/DESIGN.md`, y el código que existe de verdad en este repositorio.

Después hazme **una pregunta cada vez** y espera mi respuesta antes de la siguiente.
Empieza por las del banco de la guía, que son las que el docente ya hizo, y sigue con las
tuyas basándote **solo en el código que existe aquí**.

Cubre, en este orden: por qué estos agregados y no otros · dónde vive cada invariante y por
qué ahí · cómo se garantiza la consistencia al guardar · qué pasa si llega dos veces el mismo
evento de integración · por qué los handlers de query están en Infrastructure · por qué
MediatR está pineado en 12.5.0 · qué harías distinto con más tiempo.

Después de cada respuesta mía: dime si convence, qué falta, y la versión de treinta segundos
que yo debería haber dado —contrastándola con la respuesta corta de la guía cuando la
pregunta venga de ahí. Sé exigente. Si respondo algo que el código no hace, dímelo señalando
el archivo.
