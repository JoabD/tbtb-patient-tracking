using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Application.Contacts;
using TbtbPatientTracking.Domain.Entities;
using TbtbPatientTracking.Domain.Enums;
using TbtbPatientTracking.Tests.Support;

namespace TbtbPatientTracking.Tests.Contacts;

/// <summary>Pruebas de la lista de contactos de un paciente (GET /api/patients/{id}/contacts).</summary>
public sealed class ContactListTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 9, 23, 21, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database = new();
    private readonly ContactService _service;

    public ContactListTests()
    {
        _service = new ContactService(
            _database.CreateContext(), new FixedTimeProvider(Now), NullLogger<ContactService>.Instance);
    }

    public void Dispose() => _database.Dispose();

    private async Task<Guid> SeedPatientAsync(string documentNumber = "1032456789")
    {
        await using var db = _database.CreateContext();

        var patient = Patient.Create(
            fullName: "Ana Maria Rodriguez Lopez",
            documentType: DocumentType.Cedula,
            documentNumber: documentNumber,
            country: CountryCode.CO,
            city: "Bogota",
            phone: "+573001234567",
            email: null,
            treatmentStartDate: new DateOnly(2026, 9, 1),
            registeredAt: Now.AddDays(-10),
            createdBy: "gestor.ana");

        db.Patients.Add(patient);
        await db.SaveChangesAsync();
        return patient.Id;
    }

    /// <summary>Guarda un contacto directamente en la base; permite fijar la fecha y marcarlo como anulado.</summary>
    private async Task<Guid> SeedContactAsync(
        Guid patientId, DateTimeOffset contactDate, string gestor = "gestor.ana", bool isDeleted = false)
    {
        await using var db = _database.CreateContext();

        var contact = Contact.Create(
            patientId: patientId,
            gestorUsername: gestor,
            contactDate: contactDate,
            channel: ContactChannel.Call,
            resultCode: ContactResult.Contacted,
            observations: null,
            createdAt: Now);

        db.Contacts.Add(contact);

        if (isDeleted)
        {
            // IsDeleted tiene setter privado y la anulación (CA-3) está fuera de alcance.
            db.Entry(contact).Property(c => c.IsDeleted).CurrentValue = true;
        }

        await db.SaveChangesAsync();
        return contact.Id;
    }

    [Fact]
    public async Task ListContacts_PatientDoesNotExist_ReturnsNotFound()
    {
        var result = await _service.ListByPatientAsync(Guid.NewGuid());

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ListContacts_PatientWithoutContacts_ReturnsEmptyList()
    {
        var patientId = await SeedPatientAsync();

        var result = await _service.ListByPatientAsync(patientId);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task ListContacts_ReturnsMostRecentFirst()
    {
        var patientId = await SeedPatientAsync();
        var oldest = await SeedContactAsync(patientId, Now.AddDays(-3));
        var newest = await SeedContactAsync(patientId, Now.AddDays(-1));
        var middle = await SeedContactAsync(patientId, Now.AddDays(-2));

        var result = await _service.ListByPatientAsync(patientId);

        Assert.Equal(new[] { newest, middle, oldest }, result.Value!.Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task ListContacts_ExcludesDeletedContacts()
    {
        var patientId = await SeedPatientAsync();
        var active = await SeedContactAsync(patientId, Now.AddDays(-2));
        await SeedContactAsync(patientId, Now.AddDays(-1), isDeleted: true);

        var result = await _service.ListByPatientAsync(patientId);

        var single = Assert.Single(result.Value!);
        Assert.Equal(active, single.Id);
    }

    [Fact]
    public async Task ListContacts_ReturnsOnlyTheContactsOfThatPatient()
    {
        var patientId = await SeedPatientAsync();
        var otherPatientId = await SeedPatientAsync(documentNumber: "79845123");
        var mine = await SeedContactAsync(patientId, Now.AddDays(-1));
        await SeedContactAsync(otherPatientId, Now.AddDays(-1), gestor: "gestor.luis");

        var result = await _service.ListByPatientAsync(patientId);

        var single = Assert.Single(result.Value!);
        Assert.Equal(mine, single.Id);
        Assert.Equal(patientId, single.PatientId);
    }
}
