using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TbtbPatientTracking.Application.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Domain.Entities;
using TbtbPatientTracking.Domain.Enums;

namespace TbtbPatientTracking.Application.Contacts;

public class ContactService(
    IAppDbContext db,
    TimeProvider timeProvider,
    ILogger<ContactService> logger) : IContactService
{
    public async Task<ServiceResult<ContactResponse>> RegisterAsync(
        Guid patientId,
        RegisterContactRequest request,
        CancellationToken cancellationToken = default)
    {
        // La hora la fija el servidor (UTC), no el cliente.
        var now = timeProvider.GetUtcNow();

        var errors = ContactValidator.Validate(request, now);
        if (errors.Count > 0)
        {
            return ServiceResult<ContactResponse>.Invalid(errors);
        }

        // Red de seguridad: si el parseo fallara, los valores por defecto del enum pasarían como datos válidos.
        if (!CatalogParser.TryParse(request.Channel, out ContactChannel channel) ||
            !CatalogParser.TryParse(request.ResultCode, out ContactResult resultCode))
        {
            return ServiceResult<ContactResponse>.Invalid("Los códigos de catálogo proporcionados no son válidos.");
        }

        var patient = await db.Patients
            .Where(p => p.Id == patientId)
            .Select(p => new { p.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient is null)
        {
            logger.LogWarning("Intento de registrar un contacto para un paciente inexistente {PatientId}", patientId);
            return ServiceResult<ContactResponse>.NotFound("El paciente indicado no existe.");
        }

        // Regla: no se registran contactos de un paciente inactivo. Si está inactivo, queda a criterio del
        // gestor volver a llamarle para saber qué pasó, pero ese intento no se registra como contacto.
        if (!patient.IsActive)
        {
            logger.LogWarning("Contacto rechazado: el paciente {PatientId} está inactivo", patientId);
            return ServiceResult<ContactResponse>.Invalid(
                "No se pueden registrar contactos de un paciente inactivo.");
        }

        var contact = Contact.Create(
            patientId: patientId,
            gestorUsername: request.GestorUsername!.Trim(),
            contactDate: request.ContactDate!.Value,
            channel: channel,
            resultCode: resultCode,
            observations: request.Observations?.Trim(),
            createdAt: now);

        db.Contacts.Add(contact);
        await db.SaveChangesAsync(cancellationToken);

        // Se registran solo identificadores, sin datos personales.
        logger.LogInformation(
            "Contacto {ContactId} registrado para el paciente {PatientId} por {Gestor}",
            contact.Id, patientId, contact.GestorUsername);

        return ServiceResult<ContactResponse>.Ok(ContactResponse.FromEntity(contact));
    }

    public async Task<ServiceResult<IReadOnlyList<ContactResponse>>> ListByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Patients.AnyAsync(p => p.Id == patientId, cancellationToken))
        {
            return ServiceResult<IReadOnlyList<ContactResponse>>.NotFound("El paciente indicado no existe.");
        }

        // Solo contactos vigentes: los anulados (IsDeleted) no cuentan. La búsqueda por PatientId aprovecha
        // el índice IX_Contacts_PatientId_ContactDate.
        var contacts = await db.Contacts
            .AsNoTracking()
            .Where(c => c.PatientId == patientId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        // Se ordena en memoria: los contactos de un solo paciente son pocos, y así el orden no depende del
        // proveedor de base de datos (SQLite no puede ordenar por DateTimeOffset) y se puede probar.
        var items = contacts
            .OrderByDescending(c => c.ContactDate)
            .ThenByDescending(c => c.CreatedAt)
            .Select(ContactResponse.FromEntity)
            .ToList();

        return ServiceResult<IReadOnlyList<ContactResponse>>.Ok(items);
    }
}
