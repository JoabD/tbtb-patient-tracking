namespace TbtbPatientTracking.Tests.Support;

/// <summary>Reloj fijo para que las pruebas no dependan de la hora real.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
