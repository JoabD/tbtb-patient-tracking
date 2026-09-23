# Hallazgos del PRD

A continuación se listan los hallazgos encontrados en el extracto del PRD.
Esto con base a criterios específicos en los cuales considero que existen ciertos huecos en la definición.

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 1 y 2 (Registro y Autorregistro) |
| **Tipo** | Contradicción |
| **Descripción** | El registro por gestor exige teléfono (obligatorio por ser el contacto principal), pero el autorregistro solo pide nombre y correo. Un paciente autorregistrado no tendría teléfono. |
| **Impacto** | Los pacientes autorregistrados quedan fuera del medio principal de contacto, lo que afecta el seguimiento y las mediciones de adherencia. |
| **Pregunta al PO** | ¿El teléfono debe ser obligatorio también en el autorregistro, o el gestor debe contactar primero por correo para solicitarlo? |
| **Supuesto** | El teléfono es obligatorio en ambas vías de registro. En esta entrega solo se construye la vía del gestor. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 1 y 2 (Registro y Autorregistro) |
| **Tipo** | Riesgo |
| **Descripción** | El autorregistro solo pide nombre y correo, mientras que el gestor registra con documento de identidad. No existe un identificador compartido que permita conciliar perfiles. |
| **Impacto** | Pacientes duplicados que corrompen el historial de seguimiento y alteran las métricas de adherencia que se entregan al laboratorio. |
| **Pregunta al PO** | ¿Debemos exigir el documento de identidad también en el autorregistro para poder cruzar datos y evitar duplicados? ¿Qué debe pasar si el paciente ya existe (rechazar, fusionar, avisar al gestor)? |
| **Supuesto** | El documento (País + Tipo + Número) es obligatorio y funciona como llave única. Se rechaza el duplicado con `409 Conflict`. El autorregistro queda fuera de alcance hasta que se defina cómo validar la identidad. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 1 (Registro de pacientes) |
| **Tipo** | Ambigüedad |
| **Descripción** | No se especifica si basta el dato (tipo y número) o si se debe adjuntar una copia digital del documento. Un solo campo de texto además mezcla tipo y número. |
| **Impacto** | Si se requieren archivos, cambia la infraestructura (almacenamiento en la nube, manejo de streams en el API) y aparece un tema de retención de datos sensibles. |
| **Pregunta al PO** | ¿El registro requiere cargar el documento escaneado? ¿Se debe separar en "Tipo de documento" y "Número de documento"? |
| **Supuesto** | No se cargan archivos. El modelo separa `DocumentType` (catálogo cerrado) y `DocumentNumber`. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 4 y CA-3 (Corrección de registros) |
| **Tipo** | Riesgo |
| **Descripción** | "Corregir el registro" no dice si se sobrescribe. En un sistema de salud, sobrescribir o eliminar sin dejar motivo destruye la trazabilidad auditable. |
| **Impacto** | Posible incumplimiento de normativa de auditoría al alterar registros históricos sin huella, autor ni razón. |
| **Pregunta al PO** | ¿La corrección debe conservar el dato original (audit log con motivo, o anular y reinsertar) o basta con mostrar quién modificó el dato? |
| **Supuesto** | La corrección es append-only: el contacto original se anula (`IsDeleted`) y se inserta uno nuevo, con motivo, autor y fecha. El modelo lo soporta desde esta entrega, pero la funcionalidad (CA-3) queda fuera de alcance. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Contexto del programa (Colombia, Perú y Ecuador) y Alcance funcional 1 y 5 |
| **Tipo** | Límite no definido |
| **Descripción** | El programa opera en 3 países pero el registro solo pide "ciudad". Falta el país para el prefijo telefónico y la zona horaria. Además, "mes en curso" y "último día del mes" no dicen con qué zona horaria se cortan. |
| **Impacto** | Errores de marcación por prefijos incorrectos, llamadas fuera de horario y contactos que caen en el mes equivocado al filtrar o reportar. |
| **Pregunta al PO** | ¿Agregamos "País" explícito al registro o existe un catálogo ciudad→país? ¿El corte de mes se hace con la zona horaria del país del paciente o con una zona única del programa? |
| **Supuesto** | `Country` es obligatorio (código ISO: CO, PE, EC) y el teléfono se normaliza a formato E.164 según el país. Las fechas se guardan como `DateTimeOffset`. El corte de mes usará la zona horaria del país del paciente (se aplica cuando se construyan CA-4 a CA-6). |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 1 y 2 (Registro de pacientes) |
| **Tipo** | Riesgo |
| **Descripción** | Se recolectan datos personales (y de salud, por el tratamiento) en tres países sin mencionar la aceptación del aviso de privacidad o el consentimiento. |
| **Impacto** | Posible violación de las leyes de protección de datos de cada país. |
| **Pregunta al PO** | ¿El sistema debe incluir una casilla obligatoria y guardar la fecha y hora de aceptación? ¿Existen textos distintos por país? |
| **Supuesto** | El registro exige una casilla de aceptación y guarda `PrivacyConsentAt`. El texto legal es un marcador de posición hasta que legal lo entregue. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 3 (Registro de contactos) y CA-5 |
| **Tipo** | Vacío |
| **Descripción** | No hay catálogo definido para el resultado. Si es texto libre, no se puede calcular "no contesta tres veces" por variaciones de redacción. |
| **Impacto** | Imposible automatizar CA-5 y datos sucios que impiden métricas confiables para el laboratorio. |
| **Pregunta al PO** | ¿Podemos definir un catálogo cerrado de resultados (por ejemplo: contactado, no contesta, número equivocado) con un campo aparte de observaciones? |
| **Supuesto** | `ResultCode` es un enum cerrado ('Contactado', 'No contesta', 'Equivocado') validado en backend, más `Observations` de texto libre opcional. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | CA-5 |
| **Tipo** | Ambigüedad |
| **Descripción** | No se define si hay ventana de tiempo, si el canal importa, ni qué pasa si entre dos "no contesta" hay otro resultado (por ejemplo "número equivocado"). |
| **Impacto** | Distintos criterios producen distinta cantidad de ilocalizables y, por tanto, otro porcentaje de adherencia. |
| **Pregunta al PO** | ¿"Consecutivas" son los tres últimos contactos del paciente sin importar canal ni tiempo transcurrido? ¿Cualquier otro resultado reinicia la racha? |
| **Supuesto** | Se cuentan los tres contactos más recientes no anulados del paciente, en cualquier canal y sin ventana de tiempo. Cualquier resultado distinto de `NoAnswer` rompe la racha. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | CA-5 y CA-6 (Reporte de adherencia y pacientes activos) |
| **Tipo** | Ambigüedad |
| **Descripción** | El porcentaje se calcula sobre "pacientes activos", pero nunca se define qué es activo ni si un paciente "ilocalizable" pasa a inactivo, automática o manualmente. |
| **Impacto** | Si los ilocalizables cuentan en el denominador, el porcentaje reportado al laboratorio cambia de forma significativa. |
| **Pregunta al PO** | ¿"Ilocalizable" equivale a "inactivo" para CA-6? ¿El cambio es automático o lo hace el gestor con un motivo? ¿Qué otros eventos inactivan a un paciente (abandono, fin de tratamiento)? |
| **Supuesto** | "Activo" es un estado independiente (`IsActive`). Un ilocalizable sigue siendo activo hasta que un gestor lo inactive manualmente con un motivo. `TrackingStatus` solo indica si el paciente es localizable. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | CA-6 |
| **Tipo** | Vacío |
| **Descripción** | "Activos al último día del mes" requiere conocer el estado en una fecha pasada. Un campo que solo guarda el valor actual (`IsActive`, `TrackingStatus`) no lo permite. |
| **Impacto** | Al generar el reporte de un mes cerrado, el denominador saldría con el estado de hoy y no con el del cierre. |
| **Pregunta al PO** | ¿Se necesita reconstruir el estado de cualquier mes pasado? ¿Desde cuándo? |
| **Supuesto** | Sí. Cuando se construya CA-6 se necesitará una tabla de historial de estados (`PatientStatusHistory`). En esta entrega `IsActive` y `TrackingStatus` guardan solo el valor actual, y esa limitación queda declarada. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | CA-3 y CA-6 |
| **Tipo** | Contradicción |
| **Descripción** | CA-3 dice que el reporte refleja la información corregida, pero los reportes de adherencia se entregan mensualmente al laboratorio. No se dice qué pasa al corregir un contacto de un mes ya cerrado. |
| **Impacto** | El laboratorio podría tener una cifra que ya no coincide con la del sistema, sin rastro de por qué. |
| **Pregunta al PO** | ¿Los reportes cerrados se congelan (con corrección solo en el mes siguiente) o se regeneran y se versiona la entrega? |
| **Supuesto** | Los reportes de un mes cerrado se congelan y se guardan como una versión. Una corrección posterior genera una nueva versión, sin reemplazar la anterior. Queda fuera de alcance. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Reporte de adherencia (Alcance funcional 6) y Notas del documento |
| **Tipo** | Vacío |
| **Descripción** | El porcentaje es "contactados dentro del plazo previsto", pero el calendario de seguimiento está pendiente de confirmación con el área médica. |
| **Impacto** | Sin calendario no existe el numerador del reporte; construirlo con plazos inventados produciría cifras sin valor para el laboratorio. |
| **Pregunta al PO** | ¿Cuál es el calendario (frecuencia y plazos por semana de tratamiento) y cuándo se confirma? ¿Es el mismo para los tres países? |
| **Supuesto** | El calendario será configurable por dato y no fijo en código. Se guarda `TreatmentStartDate` para poder calcularlo después. El reporte queda fuera de alcance. |

| Campo | Qué esperamos |
| :--- | :--- |
| **Referencia** | Alcance funcional 3 y 5 |
| **Tipo** | Vacío |
| **Descripción** | El PRD habla de "gestor" y "coordinadora" pero no define autenticación, roles ni de dónde sale el gestor que se registra en cada contacto. |
| **Impacto** | Sin identidad no hay trazabilidad de quién registró o corrigió, y los filtros por gestor (CA-4) no tienen base. |
| **Pregunta al PO** | ¿Existe un sistema de identidad (SSO, Active Directory) o se administra en este sistema? ¿Qué puede hacer cada rol? |
| **Supuesto** | No hay autenticación en esta entrega. `GestorUsername` es un campo obligatorio del DTO de contacto, y queda declarado como limitación. |