using TbtbPatientTracking.Application.Common;

namespace TbtbPatientTracking.Application.Patients;

public interface IPatientService
{
    /// <summary>CA-1: registra un paciente nuevo por parte del gestor.</summary>
    Task<ServiceResult<PatientResponse>> RegisterAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken = default);
}
