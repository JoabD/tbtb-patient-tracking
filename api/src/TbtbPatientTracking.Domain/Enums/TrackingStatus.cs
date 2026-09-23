namespace TbtbPatientTracking.Domain.Enums;

/// <summary>
/// Indica si el paciente se ha podido localizar. Guarda solo el valor actual, no el histórico.
/// Es independiente de <c>IsActive</c>: un paciente ilocalizable sigue activo hasta que un gestor lo inactive.
/// </summary>
public enum TrackingStatus
{
    Reachable = 1,
    Unreachable = 2
}
