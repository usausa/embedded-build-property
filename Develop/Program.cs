#pragma warning disable CA1303
using Develop;

Console.WriteLine($"Flavor: {Variants.Flavor}");
Console.WriteLine($"Key: {Variants.Key}");
Console.WriteLine($"Code: {Variants.Code}");
Console.WriteLine($"Flag: {Variants.Flag}");

Console.WriteLine($"Value1: {EmbeddedProperty.Value1}");
Console.WriteLine($"Value2: {EmbeddedProperty.Value2}");
Console.WriteLine($"Value3: {EmbeddedProperty.Value3}");

Console.ReadLine();
