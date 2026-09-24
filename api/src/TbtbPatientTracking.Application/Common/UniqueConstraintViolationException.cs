namespace TbtbPatientTracking.Application.Common;

/// <summary>
/// Se lanza cuando la base de datos rechaza un guardado por violar un índice único (por ejemplo, un documento duplicado).
/// La capa de infraestructura traduce el error específico del motor de base de datos a esta excepción,
/// para que la capa de aplicación no dependa de SQL Server.
/// </summary>
public class UniqueConstraintViolationException(Exception innerException)
    : Exception("Se violó un índice único de la base de datos.", innerException);
