namespace TbtbPatientTracking.Application.Common;

/// <summary>Límites de paginación de los listados.</summary>
public static class Pagination
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    // Evita que (page - 1) * pageSize desborde un int al calcular el OFFSET.
    public const int MaxPage = 100_000;
}
