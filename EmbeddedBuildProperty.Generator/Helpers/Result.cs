namespace EmbeddedBuildProperty.Generator.Helpers;

public sealed record Result<TValue>(TValue Value, EquatableArray<DiagnosticInfo> Errors)
    where TValue : IEquatable<TValue>?
{
}
