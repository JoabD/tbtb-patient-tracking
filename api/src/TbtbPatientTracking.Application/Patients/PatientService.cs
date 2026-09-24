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

    private Task<bool> DocumentExistsAsync(
        CountryCode country,
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken) =>
        db.Patients.AnyAsync(
            p => p.Country == country && p.DocumentType == documentType && p.DocumentNumber == documentNumber,
            cancellationToken);
}
