namespace EmbeddedBuildProperty.Generator.Helpers;

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable CA1819
public sealed record DiagnosticInfo
{
    public DiagnosticDescriptor Descriptor { get; }

    public LocationInfo? Location { get; }

    public ImmutableDictionary<string, string?>? Properties { get; }

    public object?[]? MessageArgs { get; }

    public DiagnosticInfo(
        DiagnosticDescriptor descriptor,
        Location? location)
        : this(descriptor, location, null, null)
    {
    }

    public DiagnosticInfo(
        DiagnosticDescriptor descriptor,
        Location? location,
        ImmutableDictionary<string, string?>? properties)
        : this(descriptor, location, properties, null)
    {
    }

    public DiagnosticInfo(
        DiagnosticDescriptor descriptor,
        Location? location,
        params object?[]? messageArgs)
        : this(descriptor, location, null, messageArgs)
    {
    }

    public DiagnosticInfo(
        DiagnosticDescriptor descriptor,
        Location? location,
        ImmutableDictionary<string, string?>? properties,
        params object?[]? messageArgs)
    {
        Descriptor = descriptor;
        Location = location is not null ? LocationInfo.CreateFrom(location) : null;
        Properties = properties;
        MessageArgs = messageArgs;
    }
}
#pragma warning restore CA1819

public record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() =>
        Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? CreateFrom(SyntaxNode node) =>
        CreateFrom(node.GetLocation());

    public static LocationInfo? CreateFrom(Location location) =>
        location.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
}
