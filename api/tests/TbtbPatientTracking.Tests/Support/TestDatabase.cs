using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Infrastructure.Persistence;

namespace TbtbPatientTracking.Tests.Support;

/// <summary>
/// Base de datos SQLite en memoria para las pruebas. Se usa SQLite (y no la base en memoria de EF)
/// porque respeta los índices únicos, que es lo que hace falta para probar los duplicados.
/// La conexión debe mantenerse abierta: al cerrarla, la base en memoria desaparece.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public DbContextOptions<AppDbContext> Options { get; }

    /// <summary>Crea un contexto nuevo sobre la misma base. Útil para comprobar lo que realmente quedó guardado.</summary>
    public AppDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
