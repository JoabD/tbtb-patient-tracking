using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Domain.Entities;

/// <summary>
/// Contacto registrado por un gestor con un paciente.
/// Los registros no se sobrescriben ni se borran: una corrección futura (CA-3) anula el original
/// con <see cref="IsDeleted"/> y agrega uno nuevo, conservando el historial.
/// Se crea únicamente con <see cref="Create"/>.
/// </summary>
public class Contact
{
    // Constructor sin parámetros: solo para que EF Core pueda materializar la entidad.
    private Contact()
    {
    }

    public Guid Id { get; private set; }

    public Guid PatientId { get; private set; }

    public Patient Patient { get; private set; } = null!;

    /// <summary>Usuario del gestor que hizo el contacto. Sin autenticación en esta entrega, viene en el DTO.</summary>
    public string GestorUsername { get; private set; } = string.Empty;

    public DateTimeOffset ContactDate { get; private set; }

    public ContactChannel Channel { get; private set; }

    public ContactResult ResultCode { get; private set; }

    /// <summary>Texto libre opcional con detalles de la gestión.</summary>
    public string? Observations { get; private set; }

    /// <summary>Anulación lógica del registro.</summary>
    public bool IsDeleted { get; private set; }

    // Los tres campos siguientes preparan el CA-3 (corrección). En esta entrega quedan nulos.
    public string? CorrectionReason { get; private set; }

    public DateTimeOffset? CorrectedAt { get; private set; }

    public Guid? ReplacedByContactId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Crea un contacto vigente (no anulado) asociado a un paciente. Las reglas de catálogo y de fecha
    /// se validan antes, en la capa de aplicación; aquí solo se protege que los datos obligatorios existan.
    /// </summary>
    public static Contact Create(
        Guid patientId,
        string gestorUsername,
        DateTimeOffset contactDate,
        ContactChannel channel,
        ContactResult resultCode,
        string? observations,
        DateTimeOffset createdAt)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("El contacto debe estar asociado a un paciente.", nameof(patientId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(gestorUsername);

        return new Contact
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            GestorUsername = gestorUsername,
            ContactDate = contactDate,
            Channel = channel,
            ResultCode = resultCode,
            Observations = string.IsNullOrWhiteSpace(observations) ? null : observations,
            IsDeleted = false,
            CreatedAt = createdAt
        };
    }
}
