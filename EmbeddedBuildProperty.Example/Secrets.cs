namespace EmbeddedBuildProperty.Example;

using EmbeddedBuildProperty;

internal static partial class Secrets
{
    [BuildProperty("SecretKey1")]
    public static partial string Key();

    [BuildProperty]
    public static partial string SecretKey2();
}
