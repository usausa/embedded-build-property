using EmbeddedBuildProperty.Example;

#pragma warning disable CA1852

Console.WriteLine($"Flavor: {Variants.Flavor()}");
Console.WriteLine($"Key: {Variants.Key()}");
Console.WriteLine($"Code: {Variants.Code()}");
Console.WriteLine($"Flag: {Variants.Flag()}");
