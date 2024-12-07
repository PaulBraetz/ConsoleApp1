#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Enumerates chars from a <see cref="ReadOnlySpan{T}"/> of bytes using rented
/// buffers. Every enumerator created will utilize its own rented buffer, to be
/// returned upon disposal of the numerator.
/// </summary>
public ref partial struct BufferedUtf8CharEnumerable<TSource, TLength, TOperators>
    : IUtf8CharEnumerable<BufferedUtf8CharEnumerable<TSource, TLength, TOperators>.Enumerator, TSource, TLength, TOperators>
    where TLength : unmanaged, IEqualityOperators<TLength, TLength, Boolean> // must be unmanaged to enable "new enumerator on copy"-semantics
    where TOperators : IByteSourceLengthOperators<TLength>
    where TSource : IByteSource<TSource, TLength, TOperators>, allows ref struct
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="source">
    /// The byte source to decode characters from.
    /// </param>
    /// <param name="bufferPool">
    /// The array pool to rent a buffer from.
    /// </param>
    /// <param name="bufferLength">
    /// The buffer length to use when decoding characters. The value provided must be at least <see cref="RequiredBufferLength"/>.
    /// </param>
    public BufferedUtf8CharEnumerable(TSource source, ArrayPool<Char> bufferPool, Int32 bufferLength = Utf8CharEnumerable.RecommendedBufferLength) : this()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferLength, Utf8CharEnumerable.RequiredBufferLength);

        _bufferLength = bufferLength;
        _bufferPool = bufferPool;
        _source = source;
    }
    /// <summary>
    /// Initializes a new instance using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="source">
    /// The byte source to decode characters from.
    /// </param>
    /// <param name="bufferLength">
    /// The buffer length to use when decoding characters. The value provided must be at least <see cref="RequiredBufferLength"/>.
    /// </param>
    public BufferedUtf8CharEnumerable(TSource source, Int32 bufferLength = Utf8CharEnumerable.RecommendedBufferLength)
        : this(source, ArrayPool<Char>.Shared, bufferLength)
    { }

    private Int32 _bufferLength;
    private ArrayPool<Char> _bufferPool;
    private TSource _source;

    /// <summary>
    /// Gets a new enumerator with a dedicated rented buffer.
    /// </summary>
    /// <returns>
    /// A new enumerator.
    /// </returns>
    public Enumerator GetEnumerator() => new(_source, RentedArray.Create(_bufferLength, _bufferPool));
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource GetSource() => _source;
}
