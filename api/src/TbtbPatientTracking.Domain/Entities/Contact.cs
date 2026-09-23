using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Domain.Entities;

/// <summary>
/// Contacto registrado por un gestor con un paciente.
/// Los registros no se sobrescriben ni se borran: una corrección futura (CA-3) anula el original
/// con <see cref="IsDeleted"/> y agrega uno nuevo, conservando el historial.
/// </summary>
public class Contact
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    /// <summary>Usuario del gestor que hizo el contacto. Sin autenticación en esta entrega, viene en el DTO.</summary>
    public string GestorUsername { get; set; } = string.Empty;

    public DateTimeOffset ContactDate { get; set; }

    public ContactChannel Channel { get; set; }

    public ContactResult ResultCode { get; set; }

    /// <summary>Texto libre opcional con detalles de la gestión.</summary>
    public string? Observations { get; set; }

    /// <summary>Anulación lógica del registro.</summary>
    public bool IsDeleted { get; set; }

    // Los tres campos siguientes preparan el CA-3 (corrección). En esta entrega quedan nulos.
    public string? CorrectionReason { get; set; }

    public DateTimeOffset? CorrectedAt { get; set; }

    public Guid? ReplacedByContactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
