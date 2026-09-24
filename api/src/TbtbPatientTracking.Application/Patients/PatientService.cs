using Microsoft.Extensions.Logging;
using TbtbPatientTracking.Application.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Domain.Entities;
using TbtbPatientTracking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace TbtbPatientTracking.Application.Patients;

public class PatientService(
    IAppDbContext db,
    TimeProvider timeProvider,
    ILogger<PatientService> logger) : IPatientService
{
    private const string DuplicateMessage =
        "Ya existe un paciente registrado con ese país, tipo y número de documento.";

    public async Task<ServiceResult<PatientResponse>> RegisterAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = PatientValidator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<PatientResponse>.Invalid(errors);
        }

        // Red de seguridad: si el parseo fallara, los valores por defecto del enum pasarían como datos válidos.
        // Tras la validación no debería ocurrir, pero no se deja al azar.
        if (!CatalogParser.TryParse(request.DocumentType, out DocumentType documentType) ||
            !CatalogParser.TryParse(request.Country, out CountryCode country))
        {
            return ServiceResult<PatientResponse>.Invalid("Los códigos de catálogo proporcionados no son válidos.");
        }

        var documentNumber = PatientValidator.NormalizeDocumentNumber(request.DocumentNumber);

        if (await DocumentExistsAsync(country, documentType, documentNumber, cancellationToken))
        {
            logger.LogWarning(
                "Intento de registro duplicado rechazado para el documento {DocumentNumber}", documentNumber);
            return ServiceResult<PatientResponse>.Conflict(DuplicateMessage);
        }

        // La hora la fija el servidor (UTC), no el cliente.
        var now = timeProvider.GetUtcNow();

        // El método de fábrica garantiza que el paciente nace en un estado válido.
        var patient = Patient.Create(
            fullName: request.FullName!.Trim(),
            documentType: documentType,
            documentNumber: documentNumber,
            country: country,
            city: request.City!.Trim(),
            phone: request.Phone!.Trim(),
            email: request.Email?.Trim(),
            treatmentStartDate: request.TreatmentStartDate!.Value,
            registeredAt: now,
            createdBy: request.GestorUsername!.Trim());

        db.Patients.Add(patient);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Paciente {PatientId} registrado exitosamente por {Gestor}", patient.Id, patient.CreatedBy);
        }
        catch (UniqueConstraintViolationException ex)
        {
            // Condición de carrera: dos gestores registraron el mismo documento a la vez y el índice único
            // de la base rechazó al segundo. La infraestructura ya identificó la causa (SQL Server 2601 o 2627),
            // así que no hace falta volver a consultar. Cualquier otro error de base de datos NO se captura aquí:
            // se propaga, para no disfrazar un fallo real de "duplicado".
            logger.LogWarning(
                ex, "Condición de carrera: violación de índice único al registrar el documento {DocumentNumber}.", documentNumber);
            return ServiceResult<PatientResponse>.Conflict(DuplicateMessage);
        }

        return ServiceResult<PatientResponse>.Ok(PatientResponse.FromEntity(patient));
    }

    public async Task<ServiceResult<PagedResponse<PatientListItemResponse>>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();
        if (page < 1 || page > Pagination.MaxPage)
        {
            errors["page"] = [$"La página debe estar entre 1 y {Pagination.MaxPage}."];
        }

        if (pageSize < 1 || pageSize > Pagination.MaxPageSize)
        {
            errors["pageSize"] = [$"El tamaño de página debe estar entre 1 y {Pagination.MaxPageSize}."];
        }

        if (errors.Count > 0)
        {
            return ServiceResult<PagedResponse<PatientListItemResponse>>.Invalid(errors);
        }

        // 1. Consulta combinada de Patients y Contacts (Anexo A). Primero se pagina Patients y, solo para las filas
        // de esa página, se calculan dos subconsultas correlacionadas por PatientId: el total de contactos vigentes
        // y el último contacto. Ambas se apoyan en IX_Contacts_PatientId_ContactDate (filtrado por IsDeleted = 0).
        // Los contactos anulados se excluyen. El detalle de la justificación está en la bitácora.
        var rows = await db.Patients
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id) // desempate para que la paginación sea estable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientListRow(
                p.Id,
                p.FullName,
                p.DocumentType,
                p.DocumentNumber,
                p.Country,
                p.City,
                p.Phone,
                p.Email,
                p.TreatmentStartDate,
                p.TrackingStatus,
                p.IsActive,
                p.CreatedAt,
                p.Contacts.Count(c => !c.IsDeleted),
                p.Contacts
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.ContactDate)
                    .ThenByDescending(c => c.CreatedAt)
                    .Select(c => new LastContactRow(c.ContactDate, c.Channel, c.ResultCode, c.GestorUsername))
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        // 2. El total se calcula después de traer la página, y solo si hace falta: si la primera página no se llenó,
        // el total es lo que llegó y se ahorra un COUNT a la tabla. En cualquier otro caso sí se cuenta.
        int totalCount;
        if (page == 1 && rows.Count < pageSize)
        {
            totalCount = rows.Count;
        }
        else
        {
            totalCount = await db.Patients.CountAsync(cancellationToken);
        }

        // Los enums se pasan a texto en memoria, no en SQL, para no depender de cómo los traduzca el proveedor.
        var items = rows
            .Select(r => new PatientListItemResponse(
                r.Id,
                r.FullName,
                r.DocumentType.ToString(),
                r.DocumentNumber,
                r.Country.ToString(),
                r.City,
                r.Phone,
                r.Email,
                r.TreatmentStartDate,
                r.TrackingStatus.ToString(),
                r.IsActive,
                r.CreatedAt,
                r.ContactCount,
                r.LastContact is null
                    ? null
                    : new LastContactResponse(
                        r.LastContact.ContactDate,
                        r.LastContact.Channel.ToString(),
                        r.LastContact.ResultCode.ToString(),
                        r.LastContact.GestorUsername)))
            .ToList();

        return ServiceResult<PagedResponse<PatientListItemResponse>>.Ok(
            new PagedResponse<PatientListItemResponse>(items, page, pageSize, totalCount));
    }

    // Filas intermedias de la proyección. Son privadas: solo existen para que EF traiga los datos en una consulta.
    private sealed record LastContactRow(
        DateTimeOffset ContactDate,
        ContactChannel Channel,
        ContactResult ResultCode,
        string GestorUsername);

    private sealed record PatientListRow(
        Guid Id,
        string FullName,
        DocumentType DocumentType,
        string DocumentNumber,
        CountryCode Country,
        string City,
        string Phone,
        string? Email,
        DateOnly TreatmentStartDate,
        TrackingStatus TrackingStatus,
        bool IsActive,
        DateTimeOffset CreatedAt,
        int ContactCount,
        LastContactRow? LastContact);

    private Task<bool> DocumentExistsAsync(
        CountryCode country,
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken) =>
        db.Patients.AnyAsync(
            p => p.Country == country && p.DocumentType == documentType && p.DocumentNumber == documentNumber,
            cancellationToken);
}
