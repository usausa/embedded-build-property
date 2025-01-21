namespace BunnyTail.EmbeddedBuildProperty;

using System;

[AttributeUsage(AttributeTargets.Property)]
public sealed class BuildPropertyAttribute : Attribute
{
    public string Key { get; }

    public BuildPropertyAttribute()
        : this(string.Empty)
    {
    }

    public BuildPropertyAttribute(string key)
    {
        Key = key;
    }
}
