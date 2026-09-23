using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TbtbPatientTracking.Infrastructure.Persistence;

namespace TbtbPatientTracking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra la capa de datos. Recibe la cadena de conexión ya resuelta por el API.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }
}
