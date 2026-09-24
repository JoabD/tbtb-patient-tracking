using Microsoft.AspNetCore.Mvc;
using TbtbPatientTracking.Application.Common;

namespace TbtbPatientTracking.Api.Extensions;

public static class ServiceResultExtensions
{
    /// <summary>
    /// Traduce el resultado de un servicio a una respuesta HTTP.
    /// Éxito: la decide el controlador. Validación: 400. Conflicto: 409. No encontrado: 404.
    /// Los errores usan el formato estándar ProblemDetails.
    /// </summary>
    public static IActionResult ToActionResult<T>(
        this ServiceResult<T> result,
        ControllerBase controller,
        Func<T, IActionResult> onSuccess)
    {
        return result.Status switch
        {
            ResultStatus.Success => onSuccess(result.Value!),

            ResultStatus.ValidationFailed => controller.ValidationProblem(
                new ValidationProblemDetails(result.Errors.ToDictionary(e => e.Key, e => e.Value))),

            ResultStatus.Conflict => controller.Problem(
                title: "Conflicto",
                detail: result.Message,
                statusCode: StatusCodes.Status409Conflict),

            ResultStatus.NotFound => controller.Problem(
                title: "No encontrado",
                detail: result.Message,
                statusCode: StatusCodes.Status404NotFound),

            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
