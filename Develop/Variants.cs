namespace Develop;

using EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string Flavor { get; }

    [BuildProperty("SecretKey")]
    public static partial string? Key { get; }

    [BuildProperty]
    public static partial int Code { get; }

    [BuildProperty]
    public static partial bool Flag { get; }
}
