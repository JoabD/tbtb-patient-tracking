namespace TbtbPatientTracking.Domain.Enums;

/// <summary>
/// Resultado de un contacto. Catálogo cerrado para poder calcular reglas como
/// "no contesta tres veces consecutivas" sin depender de texto libre.
/// </summary>
public enum ContactResult
{
    Contacted = 1,
    NoAnswer = 2,
    WrongNumber = 3
}
