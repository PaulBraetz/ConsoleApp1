﻿#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;

/// <summary>
/// Implements a byte source wrapper around <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public ref struct ReadOnlySpanByteSource : IByteSource<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="source">The span to wrap.</param>
    public ReadOnlySpanByteSource(ReadOnlySpan<Byte> source) : this() => _source = source;

    private ReadOnlySpan<Byte> _source;
    /// <inheritdoc/>
    public static Int32 TryCopyTo(ReadOnlySpanByteSource source, ref Span<Byte> buffer, Int32 start = 0, Int32 maxLength = Int32.MaxValue)
    {
        ThrowHelpers.ThrowIfBufferLargerThanMaxLength(buffer, maxLength);

        var sourceSpan = source._source[start..];

        if(sourceSpan.Length > maxLength)
            return sourceSpan.Length - maxLength;

        if(sourceSpan.Length > buffer.Length)
        {
            buffer = sourceSpan.ToArray();
        } else
        {
            CopyToCore(sourceSpan, ref buffer);
        }

        return 0;
    }
    /// <inheritdoc/>
    public static void CopyTo(ReadOnlySpanByteSource source, ref Span<Byte> buffer, Int32 start = 0)
    {
        var sourceSpan = source._source.Slice(start, Math.Min(buffer.Length, source._source.Length - start));
        CopyToCore(sourceSpan, ref buffer);
    }

    private static void CopyToCore(ReadOnlySpan<Byte> sourceSpan, ref Span<Byte> buffer)
    {
        if(sourceSpan.Length < buffer.Length)
            buffer = buffer[..sourceSpan.Length];

        sourceSpan.CopyTo(buffer);
    }
}
