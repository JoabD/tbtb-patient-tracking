using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Application.Contacts;
using TbtbPatientTracking.Domain.Entities;
using TbtbPatientTracking.Domain.Enums;
using TbtbPatientTracking.Tests.Support;

namespace TbtbPatientTracking.Tests.Contacts;

/// <summary>Pruebas del CA-2: registro de un contacto asociado a un paciente.</summary>
public sealed class ContactServiceTests : IDisposable
{
    // El servidor fija la hora en UTC (offset cero), igual que TimeProvider.System.
    private static readonly DateTimeOffset Now = new(2026, 9, 23, 21, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database = new();
    private readonly ContactService _service;

    public ContactServiceTests()
    {
        _service = new ContactService(
            _database.CreateContext(), new FixedTimeProvider(Now), NullLogger<ContactService>.Instance);
    }

    public void Dispose() => _database.Dispose();

    private static RegisterContactRequest ValidRequest() => new()
    {
        GestorUsername = "gestor.ana",
        ContactDate = Now.AddHours(-2),
        Channel = "Call",
        ResultCode = "Contacted",
        Observations = "Confirma la toma diaria."
    };

    /// <summary>Guarda un paciente directamente en la base y devuelve su id.</summary>
    private async Task<Guid> SeedPatientAsync(bool isActive = true)
    {
        await using var db = _database.CreateContext();

        var patient = Patient.Create(
            fullName: "Ana Maria Rodriguez Lopez",
            documentType: DocumentType.Cedula,
            documentNumber: "1032456789",
            country: CountryCode.CO,
            city: "Bogota",
            phone: "+573001234567",
            email: null,
            treatmentStartDate: new DateOnly(2026, 9, 1),
            registeredAt: Now.AddDays(-10),
            createdBy: "gestor.ana");

        db.Patients.Add(patient);

        if (!isActive)
        {
            // IsActive tiene setter privado y aún no existe una operación de dominio para inactivar.
            db.Entry(patient).Property(p => p.IsActive).CurrentValue = false;
        }

        await db.SaveChangesAsync();
        return patient.Id;
    }

    [Fact]
    public async Task CA2_RegisterContact_ValidRequest_PersistsContactAssociatedToPatient()
    {
        var patientId = await SeedPatientAsync();
        var request = ValidRequest();

        var result = await _service.RegisterAsync(patientId, request);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal(patientId, result.Value.PatientId);

        // Se lee con un contexto nuevo para comprobar lo que realmente quedó guardado.
        await using var db = _database.CreateContext();
        var saved = await db.Contacts.SingleAsync();

        Assert.Equal(result.Value.Id, saved.Id);
        Assert.Equal(patientId, saved.PatientId);
        Assert.Equal("gestor.ana", saved.GestorUsername);
        Assert.Equal(request.ContactDate, saved.ContactDate);
        Assert.Equal(ContactChannel.Call, saved.Channel);
        Assert.Equal(ContactResult.Contacted, saved.ResultCode);
        Assert.Equal("Confirma la toma diaria.", saved.Observations);
        Assert.False(saved.IsDeleted);
        Assert.Null(saved.CorrectionReason);
        Assert.Equal(Now, saved.CreatedAt);
        Assert.Equal(TimeSpan.Zero, saved.CreatedAt.Offset);
    }

    [Fact]
    public async Task CA2_RegisterContact_PatientDoesNotExist_ReturnsNotFoundAndDoesNotInsert()
    {
        var result = await _service.RegisterAsync(Guid.NewGuid(), ValidRequest());

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.NotNull(result.Message);

        await using var db = _database.CreateContext();
        Assert.Equal(0, await db.Contacts.CountAsync());
    }

    [Fact]
    public async Task CA2_RegisterContact_EmptyRequest_ReturnsErrorsForEveryRequiredField()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(patientId, new RegisterContactRequest());

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        foreach (var field in new[] { "gestorUsername", "contactDate", "channel", "resultCode" })
        {
            Assert.Contains(field, result.Errors.Keys);
        }

        // Las observaciones son opcionales: no deben reportarse como faltantes.
        Assert.DoesNotContain("observations", result.Errors.Keys);
    }

    [Theory]
    [InlineData("Telegram")]
    [InlineData("1")]             // número: no se acepta aunque exista ese valor en el enum
    [InlineData("Call,Email")]    // lista: Enum.TryParse la convertiría en otro valor
    [InlineData("")]
    [InlineData(null)]
    public async Task CA2_RegisterContact_InvalidChannel_ReturnsValidationErrorOnChannel(string? channel)
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(patientId, ValidRequest() with { Channel = channel });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("channel", result.Errors.Keys);
    }

    [Theory]
    [InlineData("Perdido")]
    [InlineData("2")]
    [InlineData("NoAnswer,Contacted")]
    [InlineData("")]
    [InlineData(null)]
    public async Task CA2_RegisterContact_InvalidResult_ReturnsValidationErrorOnResultCode(string? resultCode)
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(patientId, ValidRequest() with { ResultCode = resultCode });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("resultCode", result.Errors.Keys);
    }

    [Fact]
    public async Task CA2_RegisterContact_FutureDate_ReturnsValidationErrorOnContactDate()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(
            patientId, ValidRequest() with { ContactDate = Now.AddMinutes(1) });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("contactDate", result.Errors.Keys);
    }

    [Fact]
    public async Task CA2_RegisterContact_DateEqualToNow_Succeeds()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(patientId, ValidRequest() with { ContactDate = Now });

        Assert.Equal(ResultStatus.Success, result.Status);
    }

    [Fact]
    public async Task CA2_RegisterContact_GestorUsernameTooLong_ReturnsValidationError()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(
            patientId, ValidRequest() with { GestorUsername = new string('a', 51) });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("gestorUsername", result.Errors.Keys);
    }

    [Fact]
    public async Task CA2_RegisterContact_ObservationsTooLong_ReturnsValidationError()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.RegisterAsync(
            patientId, ValidRequest() with { Observations = new string('x', 501) });

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("observations", result.Errors.Keys);
    }

    [Fact]
    public async Task CA2_RegisterContact_ObservationsAtLimitOrOmitted_Succeeds()
    {
        var patientId = await SeedPatientAsync();

        var atLimit = await _service.RegisterAsync(
            patientId, ValidRequest() with { Observations = new string('x', 500) });
        var omitted = await _service.RegisterAsync(
            patientId, ValidRequest() with { Observations = null });

        Assert.Equal(ResultStatus.Success, atLimit.Status);
        Assert.Equal(ResultStatus.Success, omitted.Status);
        Assert.Null(omitted.Value!.Observations);
    }

    [Fact]
    public async Task CA2_RegisterContact_InactivePatient_IsRejectedAndDoesNotInsert()
    {
        // Regla: no se registran contactos de un paciente inactivo. Esta prueba documenta esa regla.
        var patientId = await SeedPatientAsync(isActive: false);

        var result = await _service.RegisterAsync(patientId, ValidRequest());

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(string.Empty, result.Errors.Keys);

        await using var db = _database.CreateContext();
        Assert.Equal(0, await db.Contacts.CountAsync());
    }

    [Fact]
    public async Task CA2_RegisterContact_TwoContactsForSamePatient_BothAssociatedToPatient()
    {
        var patientId = await SeedPatientAsync();

        await _service.RegisterAsync(patientId, ValidRequest() with { Channel = "Call" });
        await _service.RegisterAsync(patientId, ValidRequest() with { Channel = "WhatsApp" });

        await using var db = _database.CreateContext();
        var contacts = await db.Contacts.ToListAsync();

        Assert.Equal(2, contacts.Count);
        Assert.All(contacts, c => Assert.Equal(patientId, c.PatientId));
    }

    [Fact]
    public async Task CA2_RegisterContact_NoAnswerContacts_DoNotChangePatientTrackingStatus()
    {
        // La marca de "ilocalizable" es el CA-5 y está fuera de alcance: registrar contactos no la modifica.
        var patientId = await SeedPatientAsync();

        for (var i = 0; i < 3; i++)
        {
            await _service.RegisterAsync(patientId, ValidRequest() with { ResultCode = "NoAnswer" });
        }

        await using var db = _database.CreateContext();
        var patient = await db.Patients.SingleAsync();

        Assert.Equal(TrackingStatus.Reachable, patient.TrackingStatus);
    }
}
