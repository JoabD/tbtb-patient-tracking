namespace TbtbPatientTracking.Application.Common;

public enum ResultStatus
{
    Success,
    ValidationFailed,
    Conflict,
    NotFound
}

/// <summary>
/// Resultado de una operación de servicio. Los errores esperables (validación, duplicado, no encontrado)
/// se devuelven como valor y no como excepciones; el controlador los traduce a códigos HTTP.
/// </summary>
public sealed class ServiceResult<T>
{
    private ServiceResult(
        ResultStatus status,
        T? value,
        IReadOnlyDictionary<string, string[]>? errors,
        string? message)
    {
        Status = status;
        Value = value;
        Errors = errors ?? new Dictionary<string, string[]>();
        Message = message;
    }

    public ResultStatus Status { get; }

    public T? Value { get; }

    /// <summary>Errores por campo. Solo tiene contenido cuando el estado es <see cref="ResultStatus.ValidationFailed"/>.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public string? Message { get; }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static ServiceResult<T> Ok(T value) =>
        new(ResultStatus.Success, value, null, null);

    public static ServiceResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(ResultStatus.ValidationFailed, default, errors, null);

    /// <summary>Error general que no pertenece a un campo concreto (clave vacía, como ASP.NET Core).</summary>
    public static ServiceResult<T> Invalid(string message) =>
        Invalid(new Dictionary<string, string[]> { [string.Empty] = new[] { message } });

    public static ServiceResult<T> Conflict(string message) =>
        new(ResultStatus.Conflict, default, null, message);

    public static ServiceResult<T> NotFound(string message) =>
        new(ResultStatus.NotFound, default, null, message);
}
