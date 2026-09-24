using Microsoft.AspNetCore.Mvc;
using TbtbPatientTracking.Api.Extensions;
using TbtbPatientTracking.Application.Contacts;

namespace TbtbPatientTracking.Api.Controllers;

/// <summary>El controlador solo recibe la petición, llama al servicio y traduce el resultado. No tiene lógica de negocio.</summary>
[ApiController]
[Route("api/patients/{patientId:guid}/contacts")]
public class ContactsController(IContactService contactService) : ControllerBase
{
    /// <summary>CA-2: registra un contacto asociado a un paciente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        Guid patientId,
        RegisterContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await contactService.RegisterAsync(patientId, request, cancellationToken);

        // 201 sin cabecera Location: todavía no existe un GET por id al que apuntar.
        return result.ToActionResult(this, contact => StatusCode(StatusCodes.Status201Created, contact));
    }

    /// <summary>Lista los contactos vigentes de un paciente, del más reciente al más antiguo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await contactService.ListByPatientAsync(patientId, cancellationToken);
        return result.ToActionResult(this, contacts => Ok(contacts));
    }
}
