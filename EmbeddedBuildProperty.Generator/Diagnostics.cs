namespace EmbeddedBuildProperty.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition => new(
        id: "RTBP0001",
        title: "Invalid property definition",
        messageFormat: "Property must be static partial and has getter.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnsupportedPropertyType => new(
        id: "RTBP0002",
        title: "Unsupported property type",
        messageFormat: "Property type {0} is unsupported.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
