namespace EmbeddedBuildProperty.Example;

using EmbeddedBuildProperty;

internal static partial class Secrets
{
    [BuildProperty]
    public static partial string Flavor();

    [BuildProperty("SecretKey")]
    public static partial string Key();
}
