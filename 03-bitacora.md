# Bitácora

## 1. Herramientas de IA usadas

| Herramienta | En qué partes |
| :-- | :-- |
| Claude Code | Análisis del PRD y hallazgos (revisión y corrección de supuestos), plan, y construcción del código fase a fase (API, base de datos, interfaz y pruebas). Cada fase se propuso y se aprobó antes de escribir código. |

## 2. Matriz de trazabilidad

| Criterio | Commit o archivos | Prueba que lo verifica | Estado |
| :-- | :-- | :-- | :-- |
| CA-1: registro de paciente | Pendiente | Pendiente | Fuera de alcance por ahora (se construye en la Fase 2) |
| CA-2: registro de contacto | Pendiente | Pendiente | Fuera de alcance por ahora (se construye en la Fase 3) |
| CA-3: corrección de contacto | No implementado | No aplica | Fuera de alcance |
| CA-4: filtros del mes | No implementado | No aplica | Fuera de alcance |
| CA-5: paciente ilocalizable | No implementado | No aplica | Fuera de alcance |
| CA-6: reporte de adherencia | No implementado | No aplica | Fuera de alcance |

## 3. Registro de decisiones y uso de IA

Formato: fecha · decisión · motivo · propuesta de (yo / asistente).

- 2026-09-23 · Los hallazgos y el plan se commitean antes de cualquier código (commit `3a7b27e`) · La prueba exige que el plan preceda al código en el historial · yo.
- 2026-09-23 · Se revisaron los hallazgos con el asistente: se corrigieron dos supuestos que estaban cruzados (duplicados y corrección de registros) y se agregaron hallazgos que faltaban · Los supuestos no respondían a su propio hallazgo · asistente propuso, yo aprobé.
- 2026-09-23 · Se conservó el formato de tabla `Campo | Qué esperamos` y la estructura de seis secciones del plan, sin los cambios de formato que propuso el asistente · Es el formato que pide la prueba · corrección mía sobre la propuesta del asistente.
- 2026-09-23 · **Cambio posterior al plan (Country):** `Country` deja de ser `Varchar 50` de texto libre y pasa a código ISO de 2 letras (`CO`, `PE`, `EC`), validado · Es más conveniente por la cuestión de los códigos y la normalización telefónica: el código ISO se relaciona de forma directa con el prefijo telefónico de cada país y evita errores en la comunicación con el paciente. También evita variantes de escritura ("Colombia", "colombia") y hace confiable el índice único de documento · asistente propuso, yo decidí. El plan (`02-plan.md`) ya estaba commiteado y no se edita: el cambio se registra aquí.
- 2026-09-23 · **Cambio posterior al plan (teléfono):** `01-hallazgos.md` dice que el teléfono se normaliza a E.164 según el país. Se decide una validación simple (solo dígitos con `+` opcional al inicio, de 7 a 15 dígitos), sin librería y sin normalización por país · Tiempo disponible: ya van unas 2 horas en análisis y planeación · yo decidí. Queda como mejora futura.
- 2026-09-23 · Consulta combinada del Anexo A: `GET /api/patients` con total de contactos y último contacto por paciente, y no la vista de contactos del mes del Alcance 5 · El Alcance 5 (CA-4) queda fuera de alcance · yo.
- 2026-09-23 · Tipos de documento limitados a Cédula, DNI y Pasaporte, sin reglas por país · No se conoce el contexto de otros países y hay poco tiempo · yo.
- 2026-09-23 · Esquema versionado con migraciones de EF Core, más scripts SQL en `scripts/` generados de esas migraciones, y migraciones aplicadas al arrancar en Development · Cumple el requisito de `scripts/` de la prueba y permite que el evaluador arranque con `dotnet run` · asistente propuso, yo aprobé.
- 2026-09-23 · Clean Architecture ligera (Domain, Application, Infrastructure, Api), sin MediatR, CQRS ni Unit of Work propio · Cumple la separación de capas del Anexo A sin sobreingeniería en el tiempo disponible · yo, con el asistente.

## 4. Bitácora por commit

### Commit 1: docs: definicion de hallazgos criticos y cierre de alcance y Plan
- Hash: `3a7b27e`
- Fase: 0 (previa al código)
- Qué cambió y por qué: se agregan `01-hallazgos.md` y `02-plan.md`. La prueba exige que el plan sea anterior al primer commit de código.
- Archivos principales: `01-hallazgos.md`, `02-plan.md`
- Criterio relacionado: no aplica (documentación)
- Prueba que lo verifica: no aplica
- Propuesta de: yo, con revisión del asistente
- Rechazos o correcciones: se corrigieron dos supuestos cruzados en los hallazgos y se restauró el formato de tabla y de seis secciones que yo había definido.

### Commit 2: (pendiente de mensaje)
- Hash: (se completa al inicio de la siguiente fase)
- Fase: 0
- Qué cambió y por qué: base del repositorio (`.gitignore`, `.gitattributes`, `.editorconfig`) y esqueleto de la bitácora con las dos desviaciones respecto al plan ya commiteado.
- Archivos principales: `.gitignore`, `.gitattributes`, `.editorconfig`, `03-bitacora.md`
- Criterio relacionado: no aplica
- Prueba que lo verifica: no aplica
- Propuesta de: asistente, aprobada por mí (incluido `.gitattributes`)
- Rechazos o correcciones: ninguno en esta fase.
