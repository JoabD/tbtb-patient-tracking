namespace TbtbPatientTracking.Application.Common;

/// <summary>Convierte texto en un valor de un catálogo cerrado (enum) de forma estricta.</summary>
public static class CatalogParser
{
    /// <summary>
    /// Solo acepta el nombre exacto del valor, sin distinguir mayúsculas. No usa <c>Enum.TryParse</c> porque
    /// ese método acepta números ("1") y listas separadas por coma ("Cedula,Dni"), que se convertirían
    /// en un valor distinto sin avisar.
    /// </summary>
    public static bool TryParse<TEnum>(string? text, out TEnum value) where TEnum : struct, Enum
    {
        value = default;

        var candidate = text?.Trim();
        if (string.IsNullOrEmpty(candidate))
        {
            return false;
        }

        var name = Enum.GetNames<TEnum>()
            .FirstOrDefault(n => string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase));
        if (name is null)
        {
            return false;
        }

        value = Enum.Parse<TEnum>(name);
        return true;
    }
}
