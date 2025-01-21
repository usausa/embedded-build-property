namespace BunnyTail.EmbeddedBuildProperty.Generator.Models;

using Microsoft.CodeAnalysis;

internal sealed record PropertyModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    Accessibility MethodAccessibility,
    string PropertyType,
    string PropertyName,
    string PropertyKey);
