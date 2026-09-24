namespace TbtbPatientTracking.Application.Common;

/// <summary>Página de resultados junto con el total, para que la interfaz pueda paginar.</summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
