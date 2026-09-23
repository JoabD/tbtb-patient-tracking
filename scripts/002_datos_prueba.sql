-- Datos de prueba para TbtbPatientTracking.
-- Requiere que el esquema ya exista (scripts/001_esquema.sql o migraciones de EF Core).
-- Es repetible: no inserta filas que ya existan (mismo Id).
-- Todos los datos son ficticios.

USE [tbtb-patient-tracking];
GO

-- El indice filtrado de Contacts exige QUOTED_IDENTIFIER ON. sqlcmd lo trae en OFF por defecto.
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
GO

-- Pacientes: 3 por pais (CO, PE, EC).
-- Incluye un paciente ilocalizable (tres "No contesta" seguidos) y uno inactivo.
INSERT INTO dbo.Patients
    (Id, FullName, DocumentType, DocumentNumber, Country, City, Phone, Email,
     TreatmentStartDate, TrackingStatus, IsActive, PrivacyConsentAt, CreatedAt, CreatedBy)
SELECT v.*
FROM (VALUES
    ('D67C0818-4F71-5E83-AB57-E2CB0F362C41', 'Ana Maria Rodriguez Lopez', 'Cedula', '1032456789', 'CO', 'Bogota', '+573001234567', 'ana.rodriguez@example.com', '2026-07-06', 'Reachable', 1, '2026-07-06T09:15:00-05:00', '2026-07-06T09:15:00-05:00', 'gestor.ana'),
    ('9A2AB0D8-FF93-5C57-A5DA-FD4FEC333DD3', 'Carlos Andres Herrera Diaz', 'Cedula', '79845123', 'CO', 'Medellin', '+573109876543', 'carlos.herrera@example.com', '2026-07-13', 'Unreachable', 1, '2026-07-13T10:40:00-05:00', '2026-07-13T10:40:00-05:00', 'gestor.ana'),
    ('8241B788-B7E6-531D-8447-F7028506E40F', 'Luisa Fernanda Ospina Castro', 'Passport', 'AB1234567', 'CO', 'Cali', '+573205551122', NULL, '2026-08-03', 'Reachable', 1, '2026-08-03T14:05:00-05:00', '2026-08-03T14:05:00-05:00', 'gestor.luis'),
    ('E488CE24-63DD-5D5B-B3CA-BF08A44E5546', 'Jose Luis Quispe Huaman', 'Dni', '45871236', 'PE', 'Lima', '+51987654321', 'jose.quispe@example.com', '2026-07-20', 'Reachable', 1, '2026-07-20T08:30:00-05:00', '2026-07-20T08:30:00-05:00', 'gestor.luis'),
    ('05F1FC88-03D1-516B-9FA3-F32C979C2EED', 'Rosa Elena Mamani Flores', 'Dni', '70123456', 'PE', 'Arequipa', '+51954321876', NULL, '2026-08-10', 'Reachable', 1, '2026-08-10T11:20:00-05:00', '2026-08-10T11:20:00-05:00', 'gestor.maria'),
    ('F2DF1454-C511-573F-B919-D660C3B55486', 'Miguel Angel Torres Vega', 'Dni', '41236789', 'PE', 'Cusco', '+51912345678', 'miguel.torres@example.com', '2026-06-29', 'Reachable', 0, '2026-06-29T16:45:00-05:00', '2026-06-29T16:45:00-05:00', 'gestor.maria'),
    ('38B670FC-C405-5A71-B424-4FB40F849E85', 'Maria Fernanda Cevallos Andrade', 'Cedula', '1712345678', 'EC', 'Quito', '+593991234567', 'maria.cevallos@example.com', '2026-07-27', 'Reachable', 1, '2026-07-27T09:00:00-05:00', '2026-07-27T09:00:00-05:00', 'gestor.ana'),
    ('24769912-3980-5DFB-83CF-39B694BEBD3E', 'Diego Alejandro Vera Salazar', 'Cedula', '0912345671', 'EC', 'Guayaquil', '+593987654321', NULL, '2026-08-17', 'Reachable', 1, '2026-08-17T15:10:00-05:00', '2026-08-17T15:10:00-05:00', 'gestor.luis'),
    ('FD92A093-367F-565B-AFFB-F2DDAFF7012C', 'Patricia Lorena Mora Jaramillo', 'Cedula', '0102345678', 'EC', 'Cuenca', '+593983344556', 'patricia.mora@example.com', '2026-09-01', 'Reachable', 1, '2026-09-01T10:25:00-05:00', '2026-09-01T10:25:00-05:00', 'gestor.maria')
) AS v (Id, FullName, DocumentType, DocumentNumber, Country, City, Phone, Email,
        TreatmentStartDate, TrackingStatus, IsActive, PrivacyConsentAt, CreatedAt, CreatedBy)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Patients p WHERE p.Id = v.Id);
GO

-- Contactos con distintos canales y resultados.
-- Los dos ultimos son un ejemplo de correccion: el primero esta anulado (IsDeleted = 1)
-- y apunta al que lo reemplaza.
INSERT INTO dbo.Contacts
    (Id, PatientId, GestorUsername, ContactDate, Channel, ResultCode, Observations,
     IsDeleted, CorrectionReason, CorrectedAt, ReplacedByContactId, CreatedAt)
SELECT v.*
FROM (VALUES
    ('4DCDE577-9A94-560C-9C1B-0977B4DEB259', 'D67C0818-4F71-5E83-AB57-E2CB0F362C41', 'gestor.ana', '2026-07-08T10:00:00-05:00', 'Call', 'Contacted', 'Inicio de tratamiento confirmado.', 0, NULL, NULL, NULL, '2026-07-08T10:00:00-05:00'),
    ('B0D55794-E4DF-58F0-BD02-A814336FD692', 'D67C0818-4F71-5E83-AB57-E2CB0F362C41', 'gestor.ana', '2026-07-22T10:30:00-05:00', 'WhatsApp', 'Contacted', 'Sin dudas sobre la dosis.', 0, NULL, NULL, NULL, '2026-07-22T10:30:00-05:00'),
    ('99D944CC-8116-5E70-85CC-D04A1805E038', 'D67C0818-4F71-5E83-AB57-E2CB0F362C41', 'gestor.ana', '2026-08-19T09:45:00-05:00', 'Call', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-08-19T09:45:00-05:00'),
    ('FFE3F27A-4A8C-535A-8AD4-8BA467DBB105', '9A2AB0D8-FF93-5C57-A5DA-FD4FEC333DD3', 'gestor.ana', '2026-07-15T11:00:00-05:00', 'Call', 'NoAnswer', NULL, 0, NULL, NULL, NULL, '2026-07-15T11:00:00-05:00'),
    ('2524AD51-14C1-5066-8A1C-CC747218A515', '9A2AB0D8-FF93-5C57-A5DA-FD4FEC333DD3', 'gestor.ana', '2026-07-22T15:00:00-05:00', 'Call', 'NoAnswer', NULL, 0, NULL, NULL, NULL, '2026-07-22T15:00:00-05:00'),
    ('8EA113F0-115E-594E-9716-25B5C66E4F0B', '9A2AB0D8-FF93-5C57-A5DA-FD4FEC333DD3', 'gestor.luis', '2026-07-29T09:30:00-05:00', 'WhatsApp', 'NoAnswer', 'Mensaje entregado, sin respuesta.', 0, NULL, NULL, NULL, '2026-07-29T09:30:00-05:00'),
    ('9F481606-E444-5258-9CBF-6CA09A342BA0', '8241B788-B7E6-531D-8447-F7028506E40F', 'gestor.luis', '2026-08-05T13:00:00-05:00', 'Email', 'Contacted', 'Respondio por correo.', 0, NULL, NULL, NULL, '2026-08-05T13:00:00-05:00'),
    ('18DF9367-E332-5A35-83CE-A4438BB33A68', 'E488CE24-63DD-5D5B-B3CA-BF08A44E5546', 'gestor.luis', '2026-07-22T08:45:00-05:00', 'Call', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-07-22T08:45:00-05:00'),
    ('02974C3E-BE8E-5ABE-A7B5-535EF4DE254D', 'E488CE24-63DD-5D5B-B3CA-BF08A44E5546', 'gestor.luis', '2026-08-05T08:50:00-05:00', 'Call', 'NoAnswer', NULL, 0, NULL, NULL, NULL, '2026-08-05T08:50:00-05:00'),
    ('B61F87EB-3045-5250-A2BA-D432FE41F578', 'E488CE24-63DD-5D5B-B3CA-BF08A44E5546', 'gestor.maria', '2026-08-12T17:00:00-05:00', 'WhatsApp', 'Contacted', 'Confirma toma diaria.', 0, NULL, NULL, NULL, '2026-08-12T17:00:00-05:00'),
    ('7DB4A4DA-EDE2-5A8E-9BB7-CCEAD0F31769', '05F1FC88-03D1-516B-9FA3-F32C979C2EED', 'gestor.maria', '2026-08-12T10:15:00-05:00', 'Call', 'WrongNumber', 'El numero corresponde a otra persona.', 0, NULL, NULL, NULL, '2026-08-12T10:15:00-05:00'),
    ('322DD924-139B-5683-8570-E1A0F9CA5E95', '05F1FC88-03D1-516B-9FA3-F32C979C2EED', 'gestor.maria', '2026-08-14T10:20:00-05:00', 'Email', 'Contacted', 'Proporciono un numero corregido por correo.', 0, NULL, NULL, NULL, '2026-08-14T10:20:00-05:00'),
    ('4B5B1A54-F280-559E-93FA-6C2BC6A29C88', 'F2DF1454-C511-573F-B919-D660C3B55486', 'gestor.maria', '2026-07-01T12:00:00-05:00', 'Call', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-07-01T12:00:00-05:00'),
    ('68A8A585-DE3B-5E37-B1F4-A1D28D66ABC5', 'F2DF1454-C511-573F-B919-D660C3B55486', 'gestor.maria', '2026-07-15T12:10:00-05:00', 'Call', 'Contacted', 'Paciente informa abandono del tratamiento.', 0, NULL, NULL, NULL, '2026-07-15T12:10:00-05:00'),
    ('97EDD465-01FB-54C0-9229-A4B527507BE3', '38B670FC-C405-5A71-B424-4FB40F849E85', 'gestor.ana', '2026-07-29T09:20:00-05:00', 'WhatsApp', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-07-29T09:20:00-05:00'),
    ('EE29A6B5-6120-5D14-88ED-8A3A63D4C393', '38B670FC-C405-5A71-B424-4FB40F849E85', 'gestor.ana', '2026-08-26T09:25:00-05:00', 'Call', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-08-26T09:25:00-05:00'),
    ('D7FCE559-917C-5F15-8B04-3FB74898B493', '24769912-3980-5DFB-83CF-39B694BEBD3E', 'gestor.luis', '2026-08-19T16:00:00-05:00', 'Call', 'NoAnswer', NULL, 0, NULL, NULL, NULL, '2026-08-19T16:00:00-05:00'),
    ('60C089A2-6406-5FCF-90F1-B9012440D9C5', '24769912-3980-5DFB-83CF-39B694BEBD3E', 'gestor.luis', '2026-08-21T16:05:00-05:00', 'Call', 'Contacted', 'Reagendado para seguimiento.', 0, NULL, NULL, NULL, '2026-08-21T16:05:00-05:00'),
    ('BC5A5FD8-9A1D-51E6-9E55-177C1824B5A1', 'FD92A093-367F-565B-AFFB-F2DDAFF7012C', 'gestor.maria', '2026-09-03T10:30:00-05:00', 'Call', 'Contacted', NULL, 0, NULL, NULL, NULL, '2026-09-03T10:30:00-05:00'),
    ('92EBDC60-7978-5862-BEF9-9BC710B4F48A', 'FD92A093-367F-565B-AFFB-F2DDAFF7012C', 'gestor.maria', '2026-09-10T10:35:00-05:00', 'WhatsApp', 'NoAnswer', NULL, 0, NULL, NULL, NULL, '2026-09-10T10:35:00-05:00'),
    ('0D0BE604-0007-52C3-BAB0-6287EE8B5463', 'FD92A093-367F-565B-AFFB-F2DDAFF7012C', 'gestor.maria', '2026-09-15T11:00:00-05:00', 'Call', 'Contacted', 'Registrado por error de canal.', 1, 'Canal equivocado: fue WhatsApp', '2026-09-15T12:00:00-05:00', '11757D75-2248-5CD6-8B54-A8EB2581C7D8', '2026-09-15T11:00:00-05:00'),
    ('11757D75-2248-5CD6-8B54-A8EB2581C7D8', 'FD92A093-367F-565B-AFFB-F2DDAFF7012C', 'gestor.maria', '2026-09-15T11:00:00-05:00', 'WhatsApp', 'Contacted', 'Corrige el registro anulado.', 0, NULL, NULL, NULL, '2026-09-15T12:00:00-05:00')
) AS v (Id, PatientId, GestorUsername, ContactDate, Channel, ResultCode, Observations,
        IsDeleted, CorrectionReason, CorrectedAt, ReplacedByContactId, CreatedAt)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Contacts c WHERE c.Id = v.Id);
GO
