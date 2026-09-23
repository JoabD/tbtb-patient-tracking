using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Domain.Entities;

/// <summary>Paciente inscrito en el programa de acompañamiento.</summary>
public class Patient
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DocumentType DocumentType { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public CountryCode Country { get; set; }

    public string City { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateOnly TreatmentStartDate { get; set; }

    /// <summary>Valor actual de localización. No es un histórico.</summary>
    public TrackingStatus TrackingStatus { get; set; } = TrackingStatus.Reachable;

    /// <summary>Independiente de <see cref="TrackingStatus"/>. Solo el valor actual, no un histórico.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Fecha y hora en que se aceptó el aviso de privacidad.</summary>
    public DateTimeOffset PrivacyConsentAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Usuario gestor que registró al paciente.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
