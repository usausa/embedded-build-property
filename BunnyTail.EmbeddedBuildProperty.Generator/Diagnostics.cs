namespace BunnyTail.EmbeddedBuildProperty.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidPropertyDefinition => new(
        id: "BTBP0001",
        title: "Invalid property definition",
        messageFormat: "Property must be static partial and has getter. property=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnsupportedPropertyType => new(
        id: "BTBP0002",
        title: "Unsupported property type",
        messageFormat: "Unsupported property type. type=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
