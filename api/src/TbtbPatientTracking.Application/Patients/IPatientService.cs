using TbtbPatientTracking.Application.Common;

namespace TbtbPatientTracking.Application.Patients;

public interface IPatientService
{
    /// <summary>CA-1: registra un paciente nuevo por parte del gestor.</summary>
    Task<ServiceResult<PatientResponse>> RegisterAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Listado de pacientes, del más reciente al más antiguo, con el total de contactos vigentes
    /// y el último contacto de cada uno.
    /// </summary>
    Task<ServiceResult<PagedResponse<PatientListItemResponse>>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
