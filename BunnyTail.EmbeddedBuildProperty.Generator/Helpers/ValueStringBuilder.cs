namespace BunnyTail.EmbeddedBuildProperty.Generator.Helpers;

internal ref struct ValueStringBuilder
{
    private char[]? arrayFromPool;
    private Span<char> span;
    private int pos;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        arrayFromPool = null;
        span = initialBuffer;
        pos = 0;
    }

    public void Append(char c)
    {
        if (pos >= span.Length)
        {
            Grow(1);
        }
        span[pos++] = c;
    }

    private void Grow(int additional)
    {
        var newSize = Math.Max(span.Length * 2, pos + additional);
        var newArray = new char[newSize];
        span.CopyTo(newArray);
        arrayFromPool = newArray;
        span = arrayFromPool;
    }

    public string ToTrimString()
    {
        return new string(span.Slice(0, pos).Trim().ToArray());
    }
}
