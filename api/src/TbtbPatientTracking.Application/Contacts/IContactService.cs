using TbtbPatientTracking.Application.Common;

namespace TbtbPatientTracking.Application.Contacts;

public interface IContactService
{
    /// <summary>CA-2: registra un contacto asociado a un paciente existente.</summary>
    Task<ServiceResult<ContactResponse>> RegisterAsync(
        Guid patientId,
        RegisterContactRequest request,
        CancellationToken cancellationToken = default);
}
