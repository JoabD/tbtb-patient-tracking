# Bitácora

## 1. Herramientas de IA usadas

| Herramienta | En qué partes |
| :-- | :-- |
| Claude Code | Análisis del PRD y hallazgos (revisión y corrección de supuestos), plan, y construcción del código fase a fase (API, base de datos, interfaz y pruebas). Cada fase se propuso y se aprobó antes de escribir código. |

## 2. Matriz de trazabilidad

| Criterio | Commit o archivos | Prueba que lo verifica | Estado |
| :-- | :-- | :-- | :-- |
| CA-1: registro de paciente | `api/src/TbtbPatientTracking.Application/Patients/*` (servicio y validaciones), `api/src/TbtbPatientTracking.Domain/Entities/Patient.cs` (`Patient.Create`), `api/src/TbtbPatientTracking.Api/Controllers/PatientsController.cs` | `api/tests/TbtbPatientTracking.Tests/Patients/PatientServiceTests.cs` (pruebas con prefijo `CA1_`) | Parcial (servicio y API cubiertos; falta la interfaz, Fase 5) |
| CA-2: registro de contacto | `api/src/TbtbPatientTracking.Application/Contacts/*` (servicio y validaciones), `api/src/TbtbPatientTracking.Domain/Entities/Contact.cs` (`Contact.Create`), `api/src/TbtbPatientTracking.Api/Controllers/ContactsController.cs` | `api/tests/TbtbPatientTracking.Tests/Contacts/ContactServiceTests.cs` (pruebas con prefijo `CA2_`) | Parcial (servicio y API cubiertos; falta la interfaz, Fase 5) |
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
- 2026-09-23 · **Cambio posterior al plan (valores):** los valores de los catálogos se guardan en inglés en la base y el código: `Cedula`/`Dni`/`Passport`, `Reachable`/`Unreachable`, `Contacted`/`NoAnswer`/`WrongNumber`, y `Call`/`WhatsApp`/`Email` para el canal. Las etiquetas en español se muestran solo en la interfaz · El plan tenía textos en español con espacios y tildes ('No contesta', 'Cédula'), incómodos como valores de base de datos y de código · asistente propuso, yo decidí el alcance (solo esos valores).
- 2026-09-23 · **Rechazo:** el asistente sugirió `nvarchar` para nombres, ciudad, correo y observaciones. Se mantiene `varchar` en todo el texto, como en el plan · Por tiempo, y porque la collation por defecto de SQL Server Express admite tildes y "ñ" en `varchar` · asistente propuso, yo rechacé.
- 2026-09-23 · Nombre de la solución y proyectos `TbtbPatientTracking.*` y base de datos `tbtb-patient-tracking` en `localhost\SQLEXPRESS`. La cadena `emissions-advisor-apps` era solo un ejemplo · Coincide con el nombre del repositorio · yo.
- 2026-09-23 · Los campos `IsActive` e `IsDeleted` se configuran con valor por defecto en la base y `ValueGeneratedNever()` · Sin eso, EF Core omite el valor `false` al insertar y la base aplicaría el default `true` en `IsActive`, dejando activo a un paciente que se quiso inactivo · asistente propuso, yo aprobé tras entender la razón.
- 2026-09-23 · Datos de prueba solo por script SQL (`scripts/002_datos_prueba.sql`), sin `HasData` en las migraciones · La prueba pide un script de carga y así el esquema y los datos quedan separados · asistente propuso, yo aprobé.
- 2026-09-23 · La solución de .NET vive en `api/` (con `src/` y `tests/`), el proyecto Angular irá en `web/`, y `scripts/` queda en la raíz · Estructura más limpia y coincide con la que pide la prueba · yo.
- 2026-09-23 · Herramienta `dotnet-ef` fijada en 8.0.31 (había una 9.0.2 instalada) · Alinea las herramientas con EF Core 8 y evita diferencias en la migración y el script · asistente propuso, yo ejecuté.
- 2026-09-23 · `scripts/002_datos_prueba.sql` incluye `SET QUOTED_IDENTIFIER ON` · Al probarlo con `sqlcmd` falló el insert en `Contacts`: SQL Server exige esa opción para tablas con índice filtrado y `sqlcmd` la trae apagada. Lo detecté al ejecutar el script sobre la base real · error del asistente, encontrado en la verificación.
- 2026-09-23 · `IAppDbContext` en `Application`: los servicios usan el `DbContext` directo a través de una interfaz pequeña, sin repositorios · Menos código y menos mantenimiento; la interfaz evita que `Application` dependa de `Infrastructure` (inversión de dependencias) · asistente propuso, yo aprobé (coincide con lo que yo iba a sugerir).
- 2026-09-23 · Pruebas del servicio con SQLite en memoria y no con la base en memoria de EF · SQLite respeta el índice único, necesario para probar los duplicados · asistente propuso, yo aprobé.
- 2026-09-23 · `GestorUsername` se agrega al DTO de registro de paciente (alimenta `CreatedBy`) · No hay autenticación; misma decisión que para los contactos · asistente propuso, yo aprobé.
- 2026-09-23 · El consentimiento llega como booleano `PrivacyAccepted` y la fecha de aceptación la fija el servidor en UTC · Es lo más simple y evita que el cliente falsifique la fecha · yo.
- 2026-09-23 · Las propiedades de texto del DTO son `string?` · Si fueran no nulas, ASP.NET rechazaría el campo faltante con su propio mensaje en inglés antes de llegar a mi validación en español · asistente propuso, yo aprobé.
- 2026-09-23 · `CatalogParser` compara solo por nombre exacto y no usa `Enum.TryParse` · `Enum.TryParse` acepta números ("1") y listas ("Cedula,Dni") y las convertiría en otro valor sin avisar · asistente propuso, yo aprobé.
- 2026-09-23 · **Corrección mía sobre la propuesta del asistente (`PatientService`):** el asistente propuso crear el paciente con `new Patient { ... }`, sin logging y volver a consultar la base dentro del `catch` de la condición de carrera. Lo corregí: (1) inyectar `ILogger` para dejar traza de duplicados y carreras, (2) crear el paciente con un método de fábrica `Patient.Create(...)` para que nazca en estado válido (evitar un modelo anémico), (3) no hacer una segunda consulta en el `catch`, y (4) validar que el parseo de catálogos no falle · Observabilidad en un sistema de salud, integridad de la entidad y un viaje menos a la base · yo corregí, asistente propuso.
- 2026-09-23 · **Ajuste del asistente a mi corrección:** mi versión atrapaba cualquier `DbUpdateException` como duplicado, lo que disfrazaría otros fallos de base de datos (llave foránea, texto demasiado largo, conexión) como un 409. Se traduce solo la violación de índice único (errores 2601 y 2627 de SQL Server) en `AppDbContext`, y el servicio captura esa excepción específica. Mi llamada a `Patient.Create` también estaba incompleta y se completó con todos los campos · Corrección de un defecto en mi propio código · asistente propuso, yo aprobé.
- 2026-09-23 · **Riesgo aceptado por tiempo:** los logs registran el número de documento del paciente (`{DocumentNumber}`). El asistente advirtió que es un dato personal y que los logs suelen circular y retenerse más que la base de datos; se deja así por tiempo. Mejora futura: registrar solo país y tipo de documento, o el número enmascarado · yo decidí, asistente advirtió.
- 2026-09-23 · Se permite registrar contactos con pacientes inactivos (`IsActive = false`) · Registrar el contacto puede ser precisamente el mecanismo para intentar llamar de nuevo al paciente: reactivarlo, ofrecerle un nuevo servicio o contrato, o verificar su situación. Rechazarlo cerraría esa vía · el asistente propuso permitirlo y rechazarlo como alternativa; yo confirmé permitirlo con estas razones.
- 2026-09-23 · Registrar un contacto "No contesta" no modifica `TrackingStatus` · La marca de ilocalizable es el CA-5 y está fuera de alcance; una prueba lo documenta · asistente propuso, yo aprobé.
- 2026-09-23 · Contacto con fecha futura se rechaza; la fecha igual a la hora del servidor se acepta. La validación del cuerpo va antes de comprobar que el paciente exista (400 antes que 404) · Evita una consulta a la base si la solicitud ya es inválida · asistente propuso, yo aprobé.
- 2026-09-23 · `Contact.Create` con setters privados, como `Patient.Create` · Consistencia con la corrección que hice en la Fase 2 (evitar modelo anémico) · yo, aplicado por el asistente.

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

### Commit 2: Fase 0: Agregado de Bitacora y .gitignore previendo la adicion del proyecto angular y .Net
- Hash: `34f2970`
- Fase: 0
- Qué cambió y por qué: base del repositorio (`.gitignore`, `.gitattributes`, `.editorconfig`) y esqueleto de la bitácora con las dos desviaciones respecto al plan ya commiteado.
- Archivos principales: `.gitignore`, `.gitattributes`, `.editorconfig`, `03-bitacora.md`
- Criterio relacionado: no aplica
- Prueba que lo verifica: no aplica
- Propuesta de: asistente, aprobada por mí (incluido `.gitattributes`)
- Rechazos o correcciones: ninguno en esta fase.

### Commit 3: Fase 1: solucion en capas, modelo de datos EF Core, migracion inicial y scripts SQL
- Hash: `d7af8e1`
- Fase: 1
- Qué cambió y por qué: solución en capas dentro de `api/` (Domain, Application, Infrastructure, Api y Tests), entidades y enums, configuración de EF Core con los índices del plan, migración `InitialCreate`, `Program.cs` que aplica migraciones al arrancar en Development, `appsettings.Example.json`, y los scripts `001_esquema.sql` (generado de la migración) y `002_datos_prueba.sql` (9 pacientes y 22 contactos). Verificado contra SQL Server Express: la base se creó y el script de datos cargó 9 pacientes y 22 contactos.
- Archivos principales: `api/src/TbtbPatientTracking.Domain/*`, `api/src/TbtbPatientTracking.Infrastructure/Persistence/*`, `api/src/TbtbPatientTracking.Infrastructure/Migrations/*`, `api/src/TbtbPatientTracking.Api/Program.cs`, `scripts/001_esquema.sql`, `scripts/002_datos_prueba.sql`
- Criterio relacionado: base de CA-1 y CA-2 (modelo de datos)
- Prueba que lo verifica: pendiente (las pruebas empiezan en la Fase 2). Verificación manual: conteos en la base.
- Propuesta de: asistente, con decisiones mías sobre valores, tipos, nombres y estructura de carpetas
- Rechazos o correcciones: rechacé `nvarchar` y mantuve `varchar` (ver registro de decisiones). Los valores de los catálogos pasaron de español a inglés por decisión mía sobre el alcance. Se corrigió un error del script de datos (`QUOTED_IDENTIFIER`).

### Commit 4: Fase 2: CA-1 registro de paciente (servicio, validaciones, endpoint y pruebas)
- Hash: `3c36e45`
- Fase: 2
- Qué cambió y por qué: `POST /api/patients` (CA-1). Servicio de aplicación con validaciones en español por campo, detección de documento duplicado (`409`), control de la condición de carrera traduciendo la violación del índice único en `AppDbContext`, y creación del paciente por el método de fábrica `Patient.Create`. Controlador delgado que traduce el resultado a HTTP (`201`, `400`, `409`) con `ProblemDetails`. `IAppDbContext` permite usar el `DbContext` directo desde `Application`. Se quitó `HasColumnType("date")` de `TreatmentStartDate` (SQL Server ya usa `date` por defecto para `DateOnly`); no requiere migración.
- Archivos principales: `api/src/TbtbPatientTracking.Application/Patients/*`, `api/src/TbtbPatientTracking.Application/Common/*`, `api/src/TbtbPatientTracking.Application/Abstractions/IAppDbContext.cs`, `api/src/TbtbPatientTracking.Domain/Entities/Patient.cs`, `api/src/TbtbPatientTracking.Infrastructure/Persistence/AppDbContext.cs`, `api/src/TbtbPatientTracking.Api/Controllers/PatientsController.cs`, `api/tests/TbtbPatientTracking.Tests/*`
- Criterio relacionado: CA-1
- Prueba que lo verifica: `PatientServiceTests` (17 pruebas, 34 casos, todas con prefijo `CA1_`). Resultado: 34 superadas, 0 fallidas. `dotnet ef migrations has-pending-model-changes`: sin cambios pendientes.
- Propuesta de: asistente, con correcciones mías sobre el servicio y el dominio
- Rechazos o correcciones: corregí el `PatientService` propuesto (sin logging, `new Patient {...}`, segunda consulta en el `catch`, parseo sin control). Ver registro de decisiones.

### Commit 5: Fase 3: CA-2 registro de contacto (servicio, validaciones, endpoint y pruebas)
- Hash: (se completa al inicio de la siguiente fase)
- Fase: 3
- Qué cambió y por qué: `POST /api/patients/{patientId}/contacts` (CA-2). Servicio con validaciones en español por campo (gestor, fecha no futura, canal y resultado de catálogo cerrado, observaciones), `404` si el paciente no existe, contacto creado con `Contact.Create` y controlador delgado con `ProblemDetails`. Los logs registran solo identificadores.
- Archivos principales: `api/src/TbtbPatientTracking.Application/Contacts/*`, `api/src/TbtbPatientTracking.Domain/Entities/Contact.cs`, `api/src/TbtbPatientTracking.Api/Controllers/ContactsController.cs`, `api/tests/TbtbPatientTracking.Tests/Contacts/ContactServiceTests.cs`
- Criterio relacionado: CA-2
- Prueba que lo verifica: `ContactServiceTests` (pruebas con prefijo `CA2_`). Resultado: pendiente de ejecutar.
- Propuesta de: asistente
- Rechazos o correcciones: ninguno en esta fase.
