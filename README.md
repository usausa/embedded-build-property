# EmbeddedBuildProperty

[![NuGet](https://img.shields.io/nuget/v/EmbeddedBuildProperty.svg)](https://www.nuget.org/packages/EmbeddedBuildProperty)

## What is this?

Generate a method to get the properties specified in the build options.

## Usage

### Source

```cs
namespace Example;

using EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string Flavor();

    [BuildProperty("SecretKey")]
    public static partial string Key();
}
```

### Build

```
dotnet build Example.csproj /p:EmbeddedBuildProperty=\"Flavor=Free,SecretKey=12345678\"
```

### Result

```cs
Console.WriteLine($"Flavor: {Variants.Flavor()}"); // Free
Console.WriteLine($"Key: {Variants.Key()}");       // 12345678
```

## TODO

* Extend supported types
* Secret file support?
