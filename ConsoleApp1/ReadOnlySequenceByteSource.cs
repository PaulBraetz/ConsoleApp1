#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Buffers;

/// <summary>
/// Implements a byte source wrapper around <see cref="ReadOnlySequence{T}"/>.
/// </summary>
public ref struct ReadOnlySequenceByteSource : IByteSource<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="source">The sequence to wrap.</param>
    public ReadOnlySequenceByteSource(ReadOnlySequence<Byte> source) : this() => _source = source;

    private ReadOnlySequence<Byte> _source;
    /// <inheritdoc/>
    public static Int64 TryCopyTo(ReadOnlySequenceByteSource source, ref Span<Byte> buffer, Int64 start = 0, Int32 maxLength = Int32.MaxValue)
    {
        ThrowHelpers.ThrowIfBufferLargerThanMaxLength(buffer, maxLength);

        var requiredLength = source._source.Length - start;
        if(requiredLength > maxLength)
            return requiredLength - maxLength;

        if(requiredLength > buffer.Length)
        {
            buffer = source._source.Slice(start).ToArray();
        } else
        {
            CopyToCore(source, ref buffer, start, requiredLength);
        }

        return 0;
    }
    /// <inheritdoc/>
    public static void CopyTo(ReadOnlySequenceByteSource source, ref Span<Byte> buffer, Int64 start = 0)
    {
        var requiredLength = Math.Min(source._source.Length - start, buffer.Length);

        CopyToCore(source, ref buffer, start, requiredLength);
    }
    private static void CopyToCore(ReadOnlySequenceByteSource source, ref Span<Byte> buffer, Int64 start, Int64 requiredLength)
    {
        if(requiredLength < buffer.Length)
            buffer = buffer[..(Int32)requiredLength];

        source._source.Slice(start, requiredLength).CopyTo(buffer);
    }
}

static class ThrowHelpers
{
    internal static void ThrowIfBufferLargerThanMaxLength(Span<Byte> buffer, Int32 maxLength)
    {
        if(maxLength < buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, $"{nameof(maxLength)} '{maxLength}' must be greater than or equal to the length of the buffer provided ('{buffer.Length}').");
    }
}