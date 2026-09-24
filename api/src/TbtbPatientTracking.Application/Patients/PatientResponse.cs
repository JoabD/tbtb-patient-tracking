using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Application.Patients;

/// <summary>Paciente tal como lo devuelve el API. Las entidades de dominio no salen del API.</summary>
public sealed record PatientResponse(
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
    DateTimeOffset CreatedAt)
{
    public static PatientResponse FromEntity(Patient patient) => new(
        patient.Id,
        patient.FullName,
        patient.DocumentType.ToString(),
        patient.DocumentNumber,
        patient.Country.ToString(),
        patient.City,
        patient.Phone,
        patient.Email,
        patient.TreatmentStartDate,
        patient.TrackingStatus.ToString(),
        patient.IsActive,
        patient.CreatedAt);
}
