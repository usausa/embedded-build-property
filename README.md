# BunnyTail.EmbeddedBuildProperty

[![NuGet](https://img.shields.io/nuget/v/BunnyTail.EmbeddedBuildProperty.svg)](https://www.nuget.org/packages/BunnyTail.EmbeddedBuildProperty)

## What is this?

Generate consts class to get build options.

## Usage

### Project

```xml
  <PropertyGroup>
    <EmbeddedFlavor>Development</EmbeddedFlavor>
    <EmbeddedSecretKey></EmbeddedFlavor>
  </PropertyGroup>

  <Import Project="..\.UserEmbeddedProperty.props" Condition="Exists('..\.UserEmbeddedProperty.props')" />

  <PropertyGroup>
    <EmbeddedPropertyValues>
      Flavor=string:$(EmbeddedFlavor),
      SecretKey=string:$(EmbeddedSecretKey)
    </EmbeddedPropertyValues>
  </PropertyGroup>
```

### Source

```cs
Console.WriteLine($"Flavor: {EmbeddedProperty.Flavor}");
Console.WriteLine($"SecretKey: {EmbeddedProperty.SecretKey}");
```

### Build

```
dotnet build Example.csproj /p:Flavor=Production /p:SecretKey=xxxx
```
