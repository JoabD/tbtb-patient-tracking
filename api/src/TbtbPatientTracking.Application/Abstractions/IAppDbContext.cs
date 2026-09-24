using Microsoft.EntityFrameworkCore;
using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Application.Abstractions;

/// <summary>
/// Acceso a datos que necesita la capa de aplicación. Los servicios usan el DbContext directamente
/// (sin repositorios intermedios) a través de esta interfaz, para no depender de la capa de infraestructura.
/// </summary>
public interface IAppDbContext
{
    DbSet<Patient> Patients { get; }

    DbSet<Contact> Contacts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
