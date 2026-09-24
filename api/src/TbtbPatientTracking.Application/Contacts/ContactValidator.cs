using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Application.Contacts;

/// <summary>
/// Reglas de validación del registro de contacto. Devuelve los errores por campo; si está vacío, la solicitud es válida.
/// Los nombres de campo van en camelCase para coincidir con el JSON del API.
/// </summary>
public static class ContactValidator
{
    private const int MaxGestorLength = 50;
    private const int MaxObservationsLength = 500;

    /// <param name="request">Solicitud a validar.</param>
    /// <param name="now">Hora actual del servidor, para rechazar fechas de contacto futuras.</param>
    public static IReadOnlyDictionary<string, string[]> Validate(RegisterContactRequest request, DateTimeOffset now)
    {
        var errors = new Dictionary<string, List<string>>();

        void Add(string field, string message)
        {
            if (!errors.TryGetValue(field, out var list))
            {
                list = new List<string>();
                errors[field] = list;
            }

            list.Add(message);
        }

        // Gestor
        if (string.IsNullOrWhiteSpace(request.GestorUsername))
        {
            Add("gestorUsername", "El usuario del gestor es obligatorio.");
        }
        else if (request.GestorUsername.Trim().Length > MaxGestorLength)
        {
            Add("gestorUsername", "El usuario del gestor no puede superar 50 caracteres.");
        }

        // Fecha: obligatoria y no futura
        if (request.ContactDate is null)
        {
            Add("contactDate", "La fecha del contacto es obligatoria.");
        }
        else if (request.ContactDate.Value > now)
        {
            Add("contactDate", "La fecha del contacto no puede ser futura.");
        }

        // Canal
        if (string.IsNullOrWhiteSpace(request.Channel))
        {
            Add("channel", "El canal es obligatorio.");
        }
        else if (!CatalogParser.TryParse<ContactChannel>(request.Channel, out _))
        {
            Add("channel", "El canal no es válido. Use Call, WhatsApp o Email.");
        }

        // Resultado
        if (string.IsNullOrWhiteSpace(request.ResultCode))
        {
            Add("resultCode", "El resultado del contacto es obligatorio.");
        }
        else if (!CatalogParser.TryParse<ContactResult>(request.ResultCode, out _))
        {
            Add("resultCode", "El resultado no es válido. Use Contacted, NoAnswer o WrongNumber.");
        }

        // Observaciones: opcionales
        if (request.Observations is not null && request.Observations.Trim().Length > MaxObservationsLength)
        {
            Add("observations", "Las observaciones no pueden superar 500 caracteres.");
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }
}
