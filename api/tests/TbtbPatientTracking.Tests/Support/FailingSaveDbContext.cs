using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Infrastructure.Persistence;

namespace TbtbPatientTracking.Tests.Support;

/// <summary>
/// Contexto cuyo guardado siempre falla con la excepción indicada. Sirve para simular errores de la base
/// que no se pueden provocar con SQLite, como la condición de carrera de dos gestores a la vez.
/// </summary>
public sealed class FailingSaveDbContext(DbContextOptions<AppDbContext> options, Exception failure)
    : AppDbContext(options)
{
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default) => throw failure;
}
