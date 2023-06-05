namespace EmbeddedBuildProperty.Example;

using EmbeddedBuildProperty;

internal static partial class Secrets
{
    [BuildProperty("SecretKey")]
    public static partial string Key();

    [BuildProperty]
    public static partial string Vector();
}
