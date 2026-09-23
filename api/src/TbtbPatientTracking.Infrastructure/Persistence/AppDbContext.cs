using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Infrastructure.Persistence;

/// <summary>Contexto de EF Core. La configuración de cada entidad vive en <c>Configurations</c>.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
