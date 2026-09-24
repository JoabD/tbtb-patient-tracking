namespace TbtbPatientTracking.Application.Patients;

/// <summary>Último contacto vigente (no anulado) de un paciente.</summary>
public sealed record LastContactResponse(
    DateTimeOffset ContactDate,
    string Channel,
    string ResultCode,
    string GestorUsername);

/// <summary>Fila del listado de pacientes: datos del paciente más el resumen de sus contactos.</summary>
public sealed record PatientListItemResponse(
    Guid Id,
    string FullName,
    string DocumentType,
    string DocumentNumber,
    string Country,
    string City,
    string Phone,
    string? Email,
    DateOnly TreatmentStartDate,
    string TrackingStatus,
    bool IsActive,
    DateTimeOffset CreatedAt,
    int ContactCount,
    LastContactResponse? LastContact);
