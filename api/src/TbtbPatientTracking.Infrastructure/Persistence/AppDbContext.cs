using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Application.Abstractions;
using TbtbPatientTracking.Application.Common;
using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Infrastructure.Persistence;

/// <summary>Contexto de EF Core. La configuración de cada entidad vive en <c>Configurations</c>.</summary>
public class AppDbContext : DbContext, IAppDbContext
{
    // Errores de SQL Server por violar un índice único (2601) o una restricción única (2627).
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Igual que el guardado normal, pero traduce la violación de un índice único a
    /// <see cref="UniqueConstraintViolationException"/>, para que la aplicación no dependa de SQL Server.
    /// Los demás errores de base de datos se dejan pasar sin cambios.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation };
}
