using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Domain.Entities;

/// <summary>
/// Paciente inscrito en el programa de acompañamiento.
/// Las propiedades tienen setter privado: la única forma de crear un paciente es <see cref="Create"/>,
/// que garantiza que nace en un estado válido.
/// </summary>
public class Patient
{
    // Constructor sin parámetros: solo para que EF Core pueda materializar la entidad.
    private Patient()
    {
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public DocumentType DocumentType { get; private set; }

    public string DocumentNumber { get; private set; } = string.Empty;

    public CountryCode Country { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public DateOnly TreatmentStartDate { get; private set; }

    /// <summary>Valor actual de localización. No es un histórico.</summary>
    public TrackingStatus TrackingStatus { get; private set; }

    /// <summary>Independiente de <see cref="TrackingStatus"/>. Solo el valor actual, no un histórico.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Fecha y hora en que se aceptó el aviso de privacidad.</summary>
    public DateTimeOffset PrivacyConsentAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Usuario gestor que registró al paciente.</summary>
    public string CreatedBy { get; private set; } = string.Empty;

    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();

    /// <summary>
    /// Crea un paciente nuevo: localizable, activo, con la aceptación del aviso de privacidad
    /// y el registro fechados en el momento indicado. Las reglas de formato (teléfono, correo, catálogos)
    /// se validan antes, en la capa de aplicación; aquí solo se protege que los datos obligatorios existan.
    /// </summary>
    public static Patient Create(
        string fullName,
        DocumentType documentType,
        string documentNumber,
        CountryCode country,
        string city,
        string phone,
        string? email,
        DateOnly treatmentStartDate,
        DateTimeOffset registeredAt,
        string createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        return new Patient
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            Country = country,
            City = city,
            Phone = phone,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            TreatmentStartDate = treatmentStartDate,
            TrackingStatus = TrackingStatus.Reachable,
            IsActive = true,
            // El consentimiento se confirma en el momento del registro, con la hora fijada por el servidor.
            PrivacyConsentAt = registeredAt,
            CreatedAt = registeredAt,
            CreatedBy = createdBy
        };
    }
}
