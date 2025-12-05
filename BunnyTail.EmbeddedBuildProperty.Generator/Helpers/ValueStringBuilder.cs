namespace BunnyTail.EmbeddedBuildProperty.Generator.Helpers;

using System.Buffers;

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

    public void Dispose()
    {
        if (arrayFromPool != null)
        {
            ArrayPool<char>.Shared.Return(arrayFromPool);
        }
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
        var newArray = ArrayPool<char>.Shared.Rent(newSize);
        span.CopyTo(newArray);

        if (arrayFromPool != null)
        {
            ArrayPool<char>.Shared.Return(arrayFromPool);
        }

        arrayFromPool = newArray;
        span = arrayFromPool.AsSpan(0, newSize);
    }

    public string ToTrimString()
    {
        return new string(span.Slice(0, pos).Trim().ToArray());
    }
}
