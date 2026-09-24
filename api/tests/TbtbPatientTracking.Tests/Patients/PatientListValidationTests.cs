using Microsoft.Extensions.Logging.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Application.Patients;
using TbtbPatientTracking.Tests.Support;

namespace TbtbPatientTracking.Tests.Patients;

/// <summary>
/// Pruebas de los parámetros del listado (GET /api/patients).
/// La consulta en sí no se prueba con SQLite: ese proveedor no puede ordenar por DateTimeOffset.
/// Se verifica contra SQL Server con los datos de scripts/002_datos_prueba.sql (ver bitácora).
/// </summary>
public sealed class PatientListValidationTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly PatientService _service;

    public PatientListValidationTests()
    {
        _service = new PatientService(
            _database.CreateContext(),
            new FixedTimeProvider(new DateTimeOffset(2026, 9, 23, 21, 0, 0, TimeSpan.Zero)),
            NullLogger<PatientService>.Instance);
    }

    public void Dispose() => _database.Dispose();

    [Theory]
    [InlineData(0, 25, "page")]
    [InlineData(-1, 25, "page")]
    [InlineData(Pagination.MaxPage + 1, 25, "page")]
    [InlineData(1, 0, "pageSize")]
    [InlineData(1, -5, "pageSize")]
    [InlineData(1, Pagination.MaxPageSize + 1, "pageSize")]
    public async Task List_InvalidPagingParameters_ReturnsValidationFailedOnThatField(
        int page, int pageSize, string expectedField)
    {
        var result = await _service.ListAsync(page, pageSize);

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(expectedField, result.Errors.Keys);
    }

    [Fact]
    public async Task List_BothParametersInvalid_ReportsBothFields()
    {
        var result = await _service.ListAsync(0, 0);

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains("page", result.Errors.Keys);
        Assert.Contains("pageSize", result.Errors.Keys);
    }
}
