namespace EmbeddedBuildProperty;

using Microsoft.CodeAnalysis;

public sealed partial class Generator
{
    internal sealed record MethodModel(
        string Namespace,
        string ClassName,
        bool IsValueType,
        Accessibility MethodAccessibility,
        string ReturnType,
        string MethodName,
        string PropertyName);
}
