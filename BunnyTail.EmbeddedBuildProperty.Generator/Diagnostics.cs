namespace BunnyTail.EmbeddedBuildProperty.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    // Property

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

    // Constant

    public static DiagnosticDescriptor InvalidConstValueName => new(
        id: "BTBP1001",
        title: "Invalid const value name",
        messageFormat: "Name separator '=' is not found.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConstValueType => new(
        id: "BTBP1002",
        title: "Invalid const value type",
        messageFormat: "Type separator ':' is not found.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
