# EmbeddedBuildProperty

[![NuGet Badge](https://buildstats.info/nuget/AmazonLambdaExtension)](https://www.nuget.org/packages/EmbeddedBuildProperty/)

## What is this?

Generate a method to get the properties specified in the build options.

## Usage

### Source

```cs
namespace Example;

using EmbeddedBuildProperty;

internal static partial class Secrets
{
    [BuildProperty("SecretKey1")]
    public static partial string Key();

    [BuildProperty]
    public static partial string SecretKey2();
}
```

### Build

```
dotnet build EmbeddedBuildProperty.Example.csproj /p:EmbeddedBuildProperty=\"SecretKey1=12345678,SecretKey2=00000000\"
```

### Result

```cs
Console.WriteLine($"Key1: {Secrets.Key()}");          // 12345678
Console.WriteLine($"Key2: {Secrets.SecretKey2()}");   // 00000000
```


## TODO

* Extend supported types
