# Track Connect: seguimiento de pacientes

Entrega de la prueba técnica de TBTB Global (Track Connect, Anexo A). Permite a un gestor registrar pacientes (CA-1) y registrar contactos con ellos (CA-2), y ver en una tabla de solo lectura cada paciente con su total de contactos y su último contacto.

La interfaz abre en la lista de pacientes. Registrar un paciente, registrar un contacto y ver todos los contactos de un paciente se hacen en ventanas modales, y la tabla muestra solo el último contacto de cada uno.

Documentación del ejercicio: [`01-hallazgos.md`](01-hallazgos.md) (hallazgos del PRD), [`02-plan.md`](02-plan.md) (alcance y plan) y [`03-bitacora.md`](03-bitacora.md) (trazabilidad y decisiones).

## Requisitos

| Herramienta | Versión con la que se desarrolló y probó |
| :-- | :-- |
| .NET SDK | 8.0.204 (proyectos en `net8.0`) |
| Node.js | v22.22.3 (con npm) |
| Angular | 20 (la CLI viene en las dependencias del proyecto; no hace falta instalarla global) |
| SQL Server | Express local, instancia `localhost\SQLEXPRESS`, autenticación de Windows |
| Chrome | Instalado, para las pruebas de Angular |
| `dotnet-ef` (opcional) | 8.0.31, solo si se quiere generar o revisar migraciones |

## Estructura

```
api/                         Solución .NET (TbtbPatientTracking.sln)
  src/TbtbPatientTracking.Domain          Entidades y enums
  src/TbtbPatientTracking.Application     Servicios, DTOs y validaciones
  src/TbtbPatientTracking.Infrastructure  DbContext, configuración de EF Core y migraciones
  src/TbtbPatientTracking.Api             Controladores y arranque
  tests/TbtbPatientTracking.Tests         Pruebas xUnit de la capa de servicio
web/                         Aplicación Angular 20
scripts/                     Scripts SQL (esquema y datos de prueba)
```

## 1. Base de datos

**Cadena de conexión.** Copia `api\src\TbtbPatientTracking.Api\appsettings.Example.json` como `api\src\TbtbPatientTracking.Api\appsettings.Development.json` y ajústala si tu instancia no es `localhost\SQLEXPRESS`. Ese archivo no se sube a git.

**Opción A (recomendada): migraciones automáticas.** En Development el API aplica las migraciones al arrancar y crea la base `tbtb-patient-tracking` si no existe. No hace falta hacer nada más.

**Opción B: scripts SQL.** Primero crea la base vacía y luego corre los scripts en orden. Desde una terminal, en la raíz del repositorio:

```
sqlcmd -S localhost\SQLEXPRESS -E -Q "IF DB_ID('tbtb-patient-tracking') IS NULL CREATE DATABASE [tbtb-patient-tracking]"
sqlcmd -S localhost\SQLEXPRESS -E -d tbtb-patient-tracking -I -i scripts\001_esquema.sql
sqlcmd -S localhost\SQLEXPRESS -E -d tbtb-patient-tracking -I -i scripts\002_datos_prueba.sql
```

- `001_esquema.sql` es el script idempotente generado de la migración `InitialCreate`.
- `002_datos_prueba.sql` carga datos ficticios: 9 pacientes (3 por país, entre ellos un paciente ilocalizable y uno inactivo) y 22 contactos. Se puede correr varias veces sin duplicar filas.
- La opción `-I` de `sqlcmd` es necesaria: el índice filtrado de `Contacts` exige `QUOTED_IDENTIFIER ON` y `sqlcmd` lo trae apagado. Si `sqlcmd` reclama por el certificado, agrega `-C`. También se pueden abrir los scripts en SSMS.

Si usas la opción B, el API detecta que la migración ya está aplicada y no la repite.

## 2. Levantar el API

Desde la raíz del repositorio:

```
dotnet run --project api\src\TbtbPatientTracking.Api --launch-profile https
```

- Debe usarse el perfil `https`: el API queda en `https://localhost:7155` y Angular apunta ahí.
- Swagger: `https://localhost:7155/swagger`.
- Si el navegador no confía en el certificado de desarrollo: `dotnet dev-certs https --trust`.

En Rider basta con elegir el perfil `https` y pulsar Run.

## 3. Levantar la interfaz

En otra terminal:

```
cd web
npm install
npm start
```

Abre `http://localhost:4200`. En desarrollo Angular redirige `/api` al API con un proxy (`web/proxy.conf.json`). El API además tiene una política CORS con los orígenes permitidos en `Cors:AllowedOrigins` de `appsettings.json` (hoy `http://localhost:4200`).

## 4. Pruebas

```
cd api
dotnet test
```

```
cd web
npm test -- --watch=false
```

Las de .NET son xUnit sobre la capa de servicio (SQLite en memoria); las pruebas de CA-1 llevan el prefijo `CA1_` y las de CA-2 el prefijo `CA2_`. Las de Angular usan Jasmine y Karma con el servicio simulado.

## API

| Método y ruta | Resultado |
| :-- | :-- |
| `POST /api/patients` | `201` con el paciente creado, `400` por campo inválido, `409` si ya existe el documento (país, tipo y número) |
| `POST /api/patients/{patientId}/contacts` | `201` con el contacto creado, `400` por campo inválido o paciente inactivo, `404` si el paciente no existe |
| `GET /api/patients/{patientId}/contacts` | `200` con los contactos vigentes del paciente (más recientes primero), `404` si el paciente no existe |
| `GET /api/patients?page=1&pageSize=25` | `200` con una página de pacientes (más recientes primero), su total de contactos vigentes y su último contacto. `pageSize` máximo 100; `400` si los parámetros son inválidos |

Los errores usan el formato `ProblemDetails`. Los valores de los catálogos van en inglés (`Cedula`, `Dni`, `Passport`; `CO`, `PE`, `EC`; `Call`, `WhatsApp`, `Email`; `Contacted`, `NoAnswer`, `WrongNumber`); las etiquetas en español solo existen en la interfaz.

## Alcance y limitaciones conocidas

- **Fuera de alcance:** autorregistro, corrección de contactos (CA-3), filtros del mes (CA-4), paciente ilocalizable (CA-5), reporte de adherencia (CA-6) y autenticación. No hay login: el gestor se identifica con el campo `GestorUsername`.
- **Consulta del listado sin prueba automática:** SQLite no puede ordenar por `DateTimeOffset`, así que `GET /api/patients` se verifica a mano contra SQL Server. Las pruebas cubren sus parámetros. La justificación de la consulta y de su índice está en la bitácora.
- **Aviso de `npm install`:** puede mostrar que el paquete `inflight` está obsoleto. Lo trae `karma` (ejecutor de pruebas), es solo de desarrollo y no entra al build de la aplicación.
- El texto legal del aviso de privacidad es un marcador de posición hasta que el área legal lo entregue.
