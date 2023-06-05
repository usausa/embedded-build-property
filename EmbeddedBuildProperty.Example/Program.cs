using EmbeddedBuildProperty.Example;

#pragma warning disable CA1852

Console.WriteLine($"Key1: {Secrets.Key()}");
Console.WriteLine($"Key2: {Secrets.SecretKey2()}");
