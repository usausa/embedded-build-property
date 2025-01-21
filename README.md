# BunnyTail.EmbeddedBuildProperty

[![NuGet](https://img.shields.io/nuget/v/BunnyTail.EmbeddedBuildProperty.svg)](https://www.nuget.org/packages/BunnyTail.EmbeddedBuildProperty)

## What is this?

Generate property to get build options.

## Usage

### Source

```cs
using BunnyTail.EmbeddedBuildProperty;

internal static partial class Variants
{
    [BuildProperty]
    public static partial string Flavor { get; }

    [BuildProperty("SecretKey")]
    public static partial string? Key { get; }
}
```

### Build

```
dotnet build Example.csproj /p:EmbeddedBuildProperty=\"Flavor=Free,SecretKey=12345678\"
```

### Result

```cs
Console.WriteLine($"Flavor: {Variants.Flavor}");    // Free
Console.WriteLine($"Key: {Variants.Key}");          // 12345678
```
