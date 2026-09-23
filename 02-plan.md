# Plan de Desarrollo y Cierre de Alcance

## 1. Alcance cerrado

Para abarcar el tiempo estimado (4-6 horas) y entregar una rebanada vertical con código de calidad, arquitectura en capas y pruebas unitarias, mi entrega cubrirá exclusivamente el flujo de registro primario y su visualización básica.

Criterios que se implementarán:

- **CA-1:** Registro de un paciente nuevo por parte del gestor, con validación de documento duplicado.
- **CA-2:** Registro de un contacto asociado a un paciente.
- **Mecanismo de verificación visual (Requisito implícito):** Aunque el panel de control con filtros (CA-4) queda fuera de alcance, se incluirá una tabla básica de solo lectura en la interfaz para listar los pacientes registrados y demostrar el éxito del CA-1 y CA-2 de forma visual.

## 2. Fuera de alcance

- **Autorregistro (Alcance 2):** Se omite por la falta de definición sobre cómo ligarlo de forma segura con los registros del gestor (riesgo de duplicidad de historiales al no exigir el documento de identidad).
- **Corrección de registros (CA-3):** Excede el tiempo disponible, ya que en la industria farmacéutica este tipo de sistemas requiere diseñar un esquema robusto de "soft delete" o tablas de auditoría (Audit Trail) para mantener el historial de acciones inmutable.
- **Filtros y Reportes (CA-4, CA-5, CA-6):** Se omiten para enfocar el esfuerzo en construir un backend sólido (.NET 8) y un frontend (Angular) que garanticen la persistencia correcta de la información de entrada.
- **Autenticación y roles:** El PRD no los define. El gestor se identifica con `GestorUsername`, un campo obligatorio del DTO de contacto.

## 3. Modelo de datos

Motor: SQL Server Express local (se usarán migraciones de Entity Framework Core).

- **Tabla `Patients`**:

    - `Id` (Guid, PK)
    - `FullName` (Varchar 150)
    - `DocumentType` (Varchar 50) - _Ej: 'Cédula', 'DNI', 'Pasaporte'_
    - `DocumentNumber` (Varchar 50) - _Número de identificador único del documento_
    - `Country` (Varchar 50) - _Supuesto incorporado desde los hallazgos para prefijos telefónicos_
    - `City` (Varchar 100)
    - `Phone` (Varchar 20) - _Obligatorio_
    - `Email` (Varchar 100)
    - `TreatmentStartDate` (Date)
    - `TrackingStatus` (Varchar 20) - _Valores: 'Contactable', 'Ilocalizable'. (Diseñado para materializar el estado del CA-5 sin recalcular el historial en cada consulta). Guarda solo el valor actual, no el histórico._
    - `IsActive` (Bit, default 1) - _Independiente de `TrackingStatus`: un paciente ilocalizable sigue activo hasta que un gestor lo inactive manualmente._
    - `PrivacyConsentAt` (DateTimeOffset) - _Fecha y hora de aceptación del aviso de privacidad. Obligatorio._
    - `CreatedAt` (DateTimeOffset) y `CreatedBy` (Varchar 50) - _Para trazabilidad._
    - _Índice único:_ `(Country, DocumentType, DocumentNumber)`, para impedir pacientes duplicados.
- **Tabla `Contacts`**:

    - `Id` (Guid, PK)
    - `PatientId` (Guid, FK)
    - `GestorUsername` (Varchar 50) - _Para trazabilidad_
    - `ContactDate` (DateTimeOffset)
    - `Channel` (Varchar 20) - _(Valores permitidos: Call, WhatsApp, Email)_
    - `ResultCode` (Varchar 50) - _(Enum estricto: 'Contactado', 'No contesta', 'Equivocado')_
    - `Observations` (Varchar 500) - _(Texto libre opcional para el gestor)_
    - `IsDeleted` (Bit, default 0) - _Preparación para el CA-3 a futuro_
    - `CorrectionReason` (Varchar 300), `CorrectedAt` (DateTimeOffset) y `ReplacedByContactId` (Guid, nulo) - _Preparación para el CA-3. Quedan nulos en esta entrega._
    - `CreatedAt` (DateTimeOffset)
    - _Índice:_ `(PatientId, ContactDate DESC)` filtrado por `IsDeleted = 0`, que respalda la consulta del `GET`.

_Gestión de corrección:_ Aunque el CA-3 queda fuera del código, la tabla `Contacts` incluirá un flag `IsDeleted`. Una futura corrección insertaría el nuevo registro y cambiaría a true el flag del original, conservando así la historia. El motivo, la fecha y el registro que lo reemplaza quedan en `CorrectionReason`, `CorrectedAt` y `ReplacedByContactId`.

## 4. Contrato de la interfaz (API)

Se construirá usando ASP.NET Core Web API.

- `POST /api/patients`
    - **Entrada:** DTO de Paciente.
    - **Salida:** `201 Created` con el ID generado.
    - **Errores:** `400 Bad Request` si falta información requerida (ej. Teléfono). `409 Conflict` si ya existe un paciente con el mismo País, Tipo y Número de documento.
- `POST /api/patients/{patientId}/contacts`
    - **Entrada:** DTO de Contacto.
    - **Salida:** `201 Created`.
    - **Errores:** `404 Not Found` si el paciente no existe, `400 Bad Request` por fallas de validación.
- `GET /api/patients`
    - **Salida:** `200 OK` con la lista de pacientes (incluyendo una vista resumida de sus contactos para validación visual rápida).
    - **Consulta combinada (Anexo A):** une `Patients` y `Contacts` para traer el total de contactos y el último contacto de cada paciente. Su justificación y el índice necesario se documentan en la bitácora.

## 5. Secuencia de trabajo (Estimación: 5 horas)

1. **Análisis y Planeación (1 h):** Lectura del PRD, definición de hallazgos y cierre de alcance. El commit de `01-hallazgos.md` y `02-plan.md` es el primero del repositorio, anterior a cualquier código.
2. **Configuración e Infraestructura (0.5 h):** Inicialización de solución .NET 8 con Clean Architecture ligera (Domain, Application, Infrastructure, Api), proyecto Angular 20 y dependencias.
3. **Capa de Datos y Dominio (1 h):** Creación de entidades, DbContext y Migrations de Entity Framework.
4. **Capa de Servicio (API) (1 h):** Endpoints, DTOs, lógica de negocio y Pruebas Unitarias (xUnit/NUnit).
5. **Capa de Interfaz (Web) (1 h):** Componentes de Angular, servicios inyectados, formularios reactivos y visualización de errores.
6. **Cierre (0.5 h):** Consolidación de la bitácora de IA (que se va llenando commit a commit), scripts de prueba (Seed data) y preparación del README.

## 6. Riesgos

- **Riesgo:** Incompatibilidad de entorno o fallo de arranque al ejecutar en la máquina de los evaluadores por discrepancias en las versiones de SDKs o por la base de datos.
    - **Mitigación:** Especificar claramente en el `README.md` las versiones exactas utilizadas (ej. Node v20.x, Angular CLI v20.x, .NET 8 SDK) y la cadena de conexión de ejemplo para SQL Server Express (`appsettings.Example.json`). Proveeré comandos CLI estándar (`dotnet run`, `ng serve`) y confirmaré que no existan artefactos de caché ni dependencias globales extrañas.
- **Riesgo:** Exceder el tiempo en el diseño UI/UX.
    - **Mitigación:** Se priorizará el funcionamiento y la validación de punta a punta usando librerías predeterminadas (ej. Angular Material o Bootstrap) sin invertir tiempo en detalles puramente estéticos.