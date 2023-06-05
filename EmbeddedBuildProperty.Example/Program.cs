using EmbeddedBuildProperty.Example;

#pragma warning disable CA1852

Console.WriteLine($"Flavor: {Secrets.Flavor()}");
Console.WriteLine($"Key: {Secrets.Key()}");
