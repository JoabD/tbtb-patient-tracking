using TbtbPatientTracking.Application.Common;

namespace TbtbPatientTracking.Application.Contacts;

public interface IContactService
{
    /// <summary>CA-2: registra un contacto asociado a un paciente existente.</summary>
    Task<ServiceResult<ContactResponse>> RegisterAsync(
        Guid patientId,
        RegisterContactRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Contactos vigentes (no anulados) de un paciente, del más reciente al más antiguo.
    /// Alimenta la lista de contactos de la interfaz.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<ContactResponse>>> ListByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);
}
