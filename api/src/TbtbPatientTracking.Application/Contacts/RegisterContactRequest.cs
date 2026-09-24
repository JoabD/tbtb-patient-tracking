namespace TbtbPatientTracking.Application.Contacts;

/// <summary>
/// Datos de entrada para registrar un contacto con un paciente (CA-2). El paciente llega en la ruta.
/// Las propiedades son nulables a propósito: toda la validación (con mensajes en español) ocurre en el servicio.
/// </summary>
public sealed record RegisterContactRequest
{
    /// <summary>Usuario del gestor que hizo el contacto. No hay autenticación en esta entrega.</summary>
    public string? GestorUsername { get; init; }

    /// <summary>Fecha y hora del contacto, con su zona horaria. No puede ser futura.</summary>
    public DateTimeOffset? ContactDate { get; init; }

    /// <summary>Catálogo: Call, WhatsApp o Email.</summary>
    public string? Channel { get; init; }

    /// <summary>Catálogo: Contacted, NoAnswer o WrongNumber.</summary>
    public string? ResultCode { get; init; }

    /// <summary>Texto libre opcional con detalles de la gestión.</summary>
    public string? Observations { get; init; }
}
