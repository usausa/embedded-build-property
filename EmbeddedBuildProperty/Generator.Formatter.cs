namespace EmbeddedBuildProperty;

public sealed partial class Generator
{
    private static readonly Dictionary<string, Func<string, string>> Formatters = new()
    {
        { "string", FormatString },
        { "int", FormatRaw },
        { "bool", FormatRaw }
    };

    private static string FormatRaw(string value) => value;

    private static string FormatString(string value) => $"@\"{value.Replace("\"", "\"\"")}\"";
}
