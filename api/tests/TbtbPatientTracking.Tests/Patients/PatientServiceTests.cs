using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Application.Patients;
using TbtbPatientTracking.Domain.Entities;
using TbtbPatientTracking.Domain.Enums;
using TbtbPatientTracking.Tests.Support;

namespace TbtbPatientTracking.Tests.Patients;

/// <summary>Pruebas del CA-1: registro de un paciente nuevo por parte del gestor.</summary>
public sealed class PatientServiceTests : IDisposable
{
    // El servidor fija la hora en UTC (offset cero), igual que TimeProvider.System.
    private static readonly DateTimeOffset Now = new(2026, 9, 23, 21, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database = new();
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _service = CreateService(_database.CreateContext());
    }

    public void Dispose() => _database.Dispose();

    private static PatientService CreateService(TbtbPatientTracking.Application.Abstractions.IAppDbContext db) =>
        new(db, new FixedTimeProvider(Now), NullLogger<PatientService>.Instance);

    private static RegisterPatientRequest ValidRequest() => new()
    {
        FullName = "Ana Maria Rodriguez Lopez",
        DocumentType = "Cedula",
        DocumentNumber = "1032456789",
        Country = "CO",
        City = "Bogota",
        Phone = "+573001234567",
        Email = "ana.rodriguez@example.com",
        TreatmentStartDate = new DateOnly(2026, 9, 1),
        PrivacyAccepted = true,
        GestorUsername = "gestor.ana"
    };

    [Fact]
    public async Task CA1_RegisterPatient_ValidRequest_PersistsPatientAndReturnsSuccess()
    {
        var result = await _service.RegisterAsync(ValidRequest());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);

        // Se lee con un contexto nuevo para comprobar lo que realmente quedó guardado.
        await using var db = _database.CreateContext();
        var saved = await db.Patients.SingleAsync();

        Assert.Equal(result.Value.Id, saved.Id);
        Assert.Equal("Ana Maria Rodriguez Lopez", saved.FullName);
        Assert.Equal(DocumentType.Cedula, saved.DocumentType);
        Assert.Equal("1032456789", saved.DocumentNumber);
        Assert.Equal(CountryCode.CO, saved.Country);
        Assert.Equal("+573001234567", saved.Phone);
        Assert.Equal(TrackingStatus.Reachable, saved.TrackingStatus);
        Assert.True(saved.IsActive);
        Assert.Equal(Now, saved.PrivacyConsentAt);
        Assert.Equal(TimeSpan.Zero, saved.PrivacyConsentAt.Offset);
        Assert.Equal(Now, saved.CreatedAt);
        Assert.Equal("gestor.ana", saved.CreatedBy);
    }

    [Fact]
    public async Task CA1_RegisterPatient_EmptyRequest_ReturnsErrorsForEveryRequiredField()
    {
        var result = await _service.RegisterAsync(new RegisterPatientRequest());

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);

        var expectedFields = new[]
        {
            "fullName", "documentType", "documentNumber", "country", "city",
            "phone", "treatmentStartDate", "privacyAccepted", "gestorUsername"
        };
        foreach (var field in expectedFields)
        {
            Assert.Contains(field, result.Errors.Keys);
        }

        // El correo es opcional: no debe reportarse como faltante.
        Assert.DoesNotContain("email", result.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CA1_RegisterPatient_MissingPhone_ReturnsValidationErrorOnPhone(string? phone)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Phone = phone });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("phone", result.Errors.Keys);
        Assert.Empty(await _database.CreateContext().Patients.ToListAsync());
    }

    [Theory]
    [InlineData("123456")]            // menos de 7 dígitos
    [InlineData("1234567890123456")]  // más de 15 dígitos
    [InlineData("abc1234567")]        // letras
    [InlineData("57+3001234567")]     // el + solo puede ir al inicio
    [InlineData("+57 300 1234567")]   // espacios
    public async Task CA1_RegisterPatient_InvalidPhoneFormat_ReturnsValidationErrorOnPhone(string phone)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Phone = phone });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("phone", result.Errors.Keys);
    }

    [Theory]
    [InlineData("1234567")]         // mínimo permitido
    [InlineData("3001234567")]
    [InlineData("+573001234567")]
    [InlineData("+123456789012345")] // máximo permitido
    public async Task CA1_RegisterPatient_ValidPhoneFormats_Succeeds(string phone)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Phone = phone });

        Assert.Equal(ResultStatus.Success, result.Status);
    }

    [Theory]
    [InlineData("Licencia")]
    [InlineData("1")]                // número: no se acepta aunque exista ese valor en el enum
    [InlineData("Cedula,Dni")]       // lista: Enum.TryParse la convertiría en otro valor
    [InlineData("")]
    [InlineData(null)]
    public async Task CA1_RegisterPatient_InvalidDocumentType_ReturnsValidationErrorOnDocumentType(string? documentType)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { DocumentType = documentType });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("documentType", result.Errors.Keys);
    }

    [Theory]
    [InlineData("MX")]
    [InlineData("Colombia")]
    [InlineData("")]
    [InlineData(null)]
    public async Task CA1_RegisterPatient_InvalidCountry_ReturnsValidationErrorOnCountry(string? country)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Country = country });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("country", result.Errors.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task CA1_RegisterPatient_PrivacyNotAccepted_ReturnsValidationError(bool? accepted)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { PrivacyAccepted = accepted });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("privacyAccepted", result.Errors.Keys);
    }

    [Theory]
    [InlineData("no-es-un-correo")]
    [InlineData("sin@punto")]
    public async Task CA1_RegisterPatient_InvalidEmail_ReturnsValidationErrorOnEmail(string email)
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Email = email });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("email", result.Errors.Keys);
    }

    [Fact]
    public async Task CA1_RegisterPatient_EmailOmitted_Succeeds()
    {
        var result = await _service.RegisterAsync(ValidRequest() with { Email = null });

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Null(result.Value!.Email);
    }

    [Fact]
    public async Task CA1_RegisterPatient_DuplicateDocument_ReturnsConflictAndDoesNotInsertTwice()
    {
        await _service.RegisterAsync(ValidRequest());

        var second = await _service.RegisterAsync(ValidRequest() with { FullName = "Otra Persona" });

        Assert.Equal(ResultStatus.Conflict, second.Status);
        Assert.NotNull(second.Message);

        await using var db = _database.CreateContext();
        Assert.Equal(1, await db.Patients.CountAsync());
    }

    [Fact]
    public async Task CA1_RegisterPatient_DuplicateDocumentWithDifferentCaseOrSpaces_ReturnsConflict()
    {
        var first = ValidRequest() with { DocumentType = "Passport", DocumentNumber = "ab 1234567" };
        var second = ValidRequest() with { DocumentType = "passport", DocumentNumber = "AB1234567" };

        await _service.RegisterAsync(first);
        var result = await _service.RegisterAsync(second);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CA1_RegisterPatient_SameDocumentInDifferentCountry_Succeeds()
    {
        var colombia = await _service.RegisterAsync(ValidRequest() with { Country = "CO" });
        var peru = await _service.RegisterAsync(ValidRequest() with { Country = "PE" });

        Assert.Equal(ResultStatus.Success, colombia.Status);
        Assert.Equal(ResultStatus.Success, peru.Status);
    }

    [Fact]
    public async Task CA1_RegisterPatient_RaceConditionOnSave_ReturnsConflict()
    {
        // Simula que otro gestor guardó el mismo documento entre la comprobación y el guardado:
        // la infraestructura traduce el error del índice único a UniqueConstraintViolationException.
        var failing = new FailingSaveDbContext(
            _database.Options, new UniqueConstraintViolationException(new Exception("índice único")));
        var service = CreateService(failing);

        var result = await service.RegisterAsync(ValidRequest());

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task CA1_RegisterPatient_OtherDatabaseError_IsNotDisguisedAsConflict()
    {
        // Un fallo de base de datos que no es un duplicado debe propagarse, no convertirse en un 409 falso.
        var failing = new FailingSaveDbContext(_database.Options, new DbUpdateException("otro error de base de datos"));
        var service = CreateService(failing);

        await Assert.ThrowsAsync<DbUpdateException>(() => service.RegisterAsync(ValidRequest()));
    }

    [Fact]
    public async Task CA1_UniqueIndex_DuplicateDocumentInsertedDirectly_ThrowsDbUpdateException()
    {
        // Prueba de la última defensa: aunque el servicio no lo detecte (dos gestores a la vez),
        // la base rechaza el duplicado por el índice único (País + Tipo + Número).
        await using var db = _database.CreateContext();

        db.Patients.Add(NewPatient());
        db.Patients.Add(NewPatient());

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        static Patient NewPatient() => Patient.Create(
            fullName: "Paciente Directo",
            documentType: DocumentType.Dni,
            documentNumber: "45871236",
            country: CountryCode.PE,
            city: "Lima",
            phone: "+51987654321",
            email: null,
            treatmentStartDate: new DateOnly(2026, 9, 1),
            registeredAt: DateTimeOffset.UtcNow,
            createdBy: "gestor.test");
    }
}
