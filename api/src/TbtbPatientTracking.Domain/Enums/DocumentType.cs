namespace TbtbPatientTracking.Domain.Enums;

/// <summary>
/// Tipo de documento de identidad del paciente. Catálogo cerrado.
/// En la interfaz se muestran etiquetas en español: Cédula, DNI y Pasaporte.
/// </summary>
public enum DocumentType
{
    Cedula = 1,
    Dni = 2,
    Passport = 3
}
