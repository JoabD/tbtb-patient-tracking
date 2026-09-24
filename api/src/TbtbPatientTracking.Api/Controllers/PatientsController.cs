using Microsoft.AspNetCore.Mvc;
using TbtbPatientTracking.Api.Extensions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Application.Patients;

namespace TbtbPatientTracking.Api.Controllers;

/// <summary>El controlador solo recibe la petición, llama al servicio y traduce el resultado. No tiene lógica de negocio.</summary>
[ApiController]
[Route("api/patients")]
public class PatientsController(IPatientService patientService) : ControllerBase
{
    /// <summary>CA-1: registra un paciente nuevo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await patientService.RegisterAsync(request, cancellationToken);

        // 201 sin cabecera Location: todavía no existe un GET por id al que apuntar.
        return result.ToActionResult(this, patient => StatusCode(StatusCodes.Status201Created, patient));
    }

    /// <summary>Lista pacientes (más recientes primero) con total de contactos y último contacto.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PatientListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await patientService.ListAsync(page, pageSize, cancellationToken);
        return result.ToActionResult(this, page => Ok(page));
    }
}
