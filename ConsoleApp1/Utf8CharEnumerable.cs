#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

/// <summary>
/// Provides extension methods for enumerating utf8 chars from byte sequences.
/// </summary>
public static class Utf8CharEnumerable
{
    /// <summary>
    /// The minimum required length of char buffers used in enumerators.
    /// </summary>
    public const Int32 RequiredBufferLength = 32;
    /// <summary>
    /// The recommended length of buffers used in char enumerables.
    /// </summary>
    public const Int32 RecommendedBufferLength = 2048;

    private const Int32 _bufferLengthStackAllocThreshold = 32 * 1024;

    private static readonly UTF8Encoding _encoding = (UTF8Encoding)Encoding.UTF8;

    #region ToCharEnumerable
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="buffer">The buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerator wrapping the source provided.</returns>
    public static InMemoryUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlySpan<Byte> source, Span<Char> buffer) =>
        new(new(source), buffer);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlySpan<Byte> source, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferLength);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferPool">The pool to rent buffers for intermediate results from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlySpan<Byte> source, ArrayPool<Char> bufferPool, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferPool, bufferLength);

    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySequence{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="buffer">The buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerator wrapping the source provided.</returns>
    public static InMemoryUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators> ToCharEnumerable(this ReadOnlySequence<Byte> source, Span<Char> buffer) =>
        new(new(source), buffer);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySequence{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators> ToCharEnumerable(this ReadOnlySequence<Byte> source, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferLength);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlySequence{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferPool">The pool to rent buffers for intermediate results from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators> ToCharEnumerable(this ReadOnlySequence<Byte> source, ArrayPool<Char> bufferPool, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferPool, bufferLength);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlyMemory{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="buffer">The buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerator wrapping the source provided.</returns>
    public static InMemoryUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlyMemory<Byte> source, Span<Char> buffer) =>
        new(new(source), buffer);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlyMemory{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlyMemory<Byte> source, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferLength);
    /// <summary>
    /// Gets an enumerable able to enumerate the utf8 chars from a <see
    /// cref="ReadOnlyMemory{T}"/> of bytes.
    /// </summary>
    /// <param name="source">The byte source to decode chars from.</param>
    /// <param name="bufferPool">The pool to rent buffers for intermediate results from.</param>
    /// <param name="bufferLength">The length of the buffer to use when storing intermediate results.</param>
    /// <returns>A new utf8 char enumerable wrapping the source provided.</returns>
    public static BufferedUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators> ToCharEnumerable(this ReadOnlyMemory<Byte> source, ArrayPool<Char> bufferPool, Int32 bufferLength = RecommendedBufferLength) =>
        new(new(source), bufferPool, bufferLength);
    #endregion
    #region ToArray
    /// <summary>
    /// Gets an array the characters yielded by a char enumerable.
    /// </summary>
    /// <param name="enumerator">
    /// The enumerable whose chars to return as an array.
    /// </param>
    /// <returns>
    /// An array of the chars provided by <paramref name="enumerable"/>.
    /// </returns>
    public static Char[] ToArray<TEnumerable, TEnumerator, TSource, TLength, TOperators>(
        this ref TEnumerable enumerable)
        where TEnumerator : IEnumerator<Char>, allows ref struct
        where TEnumerable : struct, IUtf8CharEnumerable<TEnumerator, TSource, TLength, TOperators>, allows ref struct
        where TSource : IByteSource<TSource, TLength, TOperators>, allows ref struct
        where TOperators : IByteSourceLengthOperators<TLength>
        where TLength : unmanaged, IEqualityOperators<TLength, TLength, Boolean>
    {
        var source = enumerable.GetSource();
        Span<Byte> buffer = stackalloc Byte[RecommendedBufferLength];

        if(TSource.TryCopyTo(source, ref buffer) != default)
            throw new InvalidOperationException($"Unable to obtain array of bytes from {nameof(enumerable)}; the underlying byte source can supply more bytes than would fit.");

        var requiredResultLength = _encoding.GetCharCount(buffer);
        var result = new Char[requiredResultLength];
        var written = _encoding.GetChars(buffer, result);

        Debug.Assert(written == result.Length);

        return result;
    }

    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref InMemoryUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators> enumerable) =>
        ToArray<
            InMemoryUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators>,
            InMemoryUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators>,
            ReadOnlySpanByteSource,
            Int32,
            ByteSourceInt32LengthOperators>(ref enumerable);
    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref BufferedUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators> enumerable) =>
        ToArray<
            BufferedUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators>,
            BufferedUtf8CharEnumerable<ReadOnlySpanByteSource, Int32, ByteSourceInt32LengthOperators>.Enumerator,
            ReadOnlySpanByteSource,
            Int32,
            ByteSourceInt32LengthOperators>(ref enumerable);
    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref InMemoryUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators> enumerable) =>
        ToArray<
            InMemoryUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators>,
            InMemoryUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators>,
            ReadOnlyMemoryByteSource,
            Int32,
            ByteSourceInt32LengthOperators>(ref enumerable);
    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref BufferedUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators> enumerable) =>
        ToArray<
            BufferedUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators>,
            BufferedUtf8CharEnumerable<ReadOnlyMemoryByteSource, Int32, ByteSourceInt32LengthOperators>.Enumerator,
            ReadOnlyMemoryByteSource,
            Int32,
            ByteSourceInt32LengthOperators>(ref enumerable);
    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref InMemoryUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators> enumerable) =>
        ToArray<
            InMemoryUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators>,
            InMemoryUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators>,
            ReadOnlySequenceByteSource,
            Int64,
            ByteSourceInt64LengthOperators>(ref enumerable);
    /// <inheritdoc cref="ToArray{TEnumerator, TSource, TLength, TOperators}(ref TEnumerator)"/>
    public static Char[] ToArray(this ref BufferedUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators> enumerable) =>
        ToArray<
            BufferedUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators>,
            BufferedUtf8CharEnumerable<ReadOnlySequenceByteSource, Int64, ByteSourceInt64LengthOperators>.Enumerator,
            ReadOnlySequenceByteSource,
            Int64,
            ByteSourceInt64LengthOperators>(ref enumerable);
    #endregion
}
