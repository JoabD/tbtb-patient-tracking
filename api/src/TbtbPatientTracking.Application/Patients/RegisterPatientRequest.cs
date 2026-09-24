namespace TbtbPatientTracking.Application.Patients;

/// <summary>
/// Datos de entrada para registrar un paciente (CA-1).
/// Las propiedades de texto son <c>string?</c> a propósito: así ASP.NET no rechaza el campo faltante por su cuenta
/// y toda la validación (con mensajes en español) ocurre en el servicio.
/// </summary>
public sealed record RegisterPatientRequest
{
    public string? FullName { get; init; }

    /// <summary>Catálogo: Cedula, Dni o Passport.</summary>
    public string? DocumentType { get; init; }

    public string? DocumentNumber { get; init; }

    /// <summary>Código ISO: CO, PE o EC.</summary>
    public string? Country { get; init; }

    public string? City { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public DateOnly? TreatmentStartDate { get; init; }

    /// <summary>El gestor confirma que el paciente aceptó el aviso de privacidad.</summary>
    public bool? PrivacyAccepted { get; init; }

    /// <summary>Usuario del gestor que registra. No hay autenticación en esta entrega.</summary>
    public string? GestorUsername { get; init; }
}
