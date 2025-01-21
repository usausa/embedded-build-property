namespace EmbeddedBuildProperty.Generator.Helpers;

using Microsoft.CodeAnalysis;

internal static class DiagnosticExtensions
{
    public static void ReportDiagnostic(this SourceProductionContext context, DiagnosticInfo info)
    {
        var diagnostic = Diagnostic.Create(
            info.Descriptor,
            info.Location?.ToLocation(),
            messageArgs: info.Properties,
            properties: info.Properties);
        context.ReportDiagnostic(diagnostic);
    }
}
