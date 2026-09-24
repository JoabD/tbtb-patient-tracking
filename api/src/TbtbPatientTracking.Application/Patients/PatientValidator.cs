using System.Text.RegularExpressions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Application.Patients;

/// <summary>
/// Reglas de validación del registro de paciente. Devuelve los errores por campo; si está vacío, la solicitud es válida.
/// Los nombres de campo van en camelCase para coincidir con el JSON del API y con los controles del formulario.
/// </summary>
public static class PatientValidator
{
    // Formato simple decidido para esta entrega: dígitos con un "+" opcional al inicio, de 7 a 15 dígitos.
    // No se normaliza por país (ver bitácora).
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    // Después de normalizar, el documento solo puede tener letras y números (el pasaporte es alfanumérico).
    private static readonly Regex DocumentNumberRegex = new(@"^[A-Z0-9]+$", RegexOptions.Compiled);

    /// <summary>Recorta, pasa a mayúsculas y quita espacios, para que "ab 123" y "AB123" sean el mismo documento.</summary>
    public static string NormalizeDocumentNumber(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty);

    public static IReadOnlyDictionary<string, string[]> Validate(RegisterPatientRequest request)
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

        // Nombre
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            Add("fullName", "El nombre completo es obligatorio.");
        }
        else if (request.FullName.Trim().Length > 150)
        {
            Add("fullName", "El nombre completo no puede superar 150 caracteres.");
        }

        // Documento: tipo y número
        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            Add("documentType", "El tipo de documento es obligatorio.");
        }
        else if (!CatalogParser.TryParse<DocumentType>(request.DocumentType, out _))
        {
            Add("documentType", "El tipo de documento no es válido. Use Cedula, Dni o Passport.");
        }

        var documentNumber = NormalizeDocumentNumber(request.DocumentNumber);
        if (documentNumber.Length == 0)
        {
            Add("documentNumber", "El número de documento es obligatorio.");
        }
        else if (documentNumber.Length > 50)
        {
            Add("documentNumber", "El número de documento no puede superar 50 caracteres.");
        }
        else if (!DocumentNumberRegex.IsMatch(documentNumber))
        {
            Add("documentNumber", "El número de documento solo puede tener letras y números.");
        }

        // País
        if (string.IsNullOrWhiteSpace(request.Country))
        {
            Add("country", "El país es obligatorio.");
        }
        else if (!CatalogParser.TryParse<CountryCode>(request.Country, out _))
        {
            Add("country", "El país no es válido. Use CO, PE o EC.");
        }

        // Ciudad
        if (string.IsNullOrWhiteSpace(request.City))
        {
            Add("city", "La ciudad es obligatoria.");
        }
        else if (request.City.Trim().Length > 100)
        {
            Add("city", "La ciudad no puede superar 100 caracteres.");
        }

        // Teléfono: obligatorio porque es el medio principal de contacto
        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            Add("phone", "El teléfono es obligatorio.");
        }
        else if (!PhoneRegex.IsMatch(request.Phone.Trim()))
        {
            Add("phone", "El teléfono debe tener entre 7 y 15 dígitos, con un \"+\" opcional al inicio.");
        }

        // Correo: opcional, pero si viene debe ser válido
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            if (email.Length > 100)
            {
                Add("email", "El correo no puede superar 100 caracteres.");
            }
            else if (!EmailRegex.IsMatch(email))
            {
                Add("email", "El correo no tiene un formato válido.");
            }
        }

        // Fecha de inicio de tratamiento
        if (request.TreatmentStartDate is null)
        {
            Add("treatmentStartDate", "La fecha de inicio de tratamiento es obligatoria.");
        }

        // Consentimiento: el gestor debe confirmar que el paciente aceptó el aviso de privacidad
        if (request.PrivacyAccepted != true)
        {
            Add("privacyAccepted", "Debe confirmarse la aceptación del aviso de privacidad.");
        }

        // Gestor
        if (string.IsNullOrWhiteSpace(request.GestorUsername))
        {
            Add("gestorUsername", "El usuario del gestor es obligatorio.");
        }
        else if (request.GestorUsername.Trim().Length > 50)
        {
            Add("gestorUsername", "El usuario del gestor no puede superar 50 caracteres.");
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }
}
