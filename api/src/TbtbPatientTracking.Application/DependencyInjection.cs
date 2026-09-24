using Microsoft.Extensions.DependencyInjection;
using TbtbPatientTracking.Application.Contacts;
using TbtbPatientTracking.Application.Patients;

namespace TbtbPatientTracking.Application;

public static class DependencyInjection
{
    /// <summary>Registra los servicios de la capa de aplicación.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // TimeProvider permite fijar la hora en las pruebas.
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IContactService, ContactService>();

        return services;
    }
}
