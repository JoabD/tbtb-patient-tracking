using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Application.Contacts;

/// <summary>Contacto tal como lo devuelve el API.</summary>
public sealed record ContactResponse(
    Guid Id,
    Guid PatientId,
    string GestorUsername,
    DateTimeOffset ContactDate,
    string Channel,
    string ResultCode,
    string? Observations,
    DateTimeOffset CreatedAt)
{
    public static ContactResponse FromEntity(Contact contact) => new(
        contact.Id,
        contact.PatientId,
        contact.GestorUsername,
        contact.ContactDate,
        contact.Channel.ToString(),
        contact.ResultCode.ToString(),
        contact.Observations,
        contact.CreatedAt);
}
