namespace EmbeddedBuildProperty.Example;

using EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string Flavor();

    [BuildProperty("SecretKey")]
    public static partial string? Key();

    [BuildProperty]
    public static partial int Code();

    [BuildProperty]
    public static partial bool Flag();
}
