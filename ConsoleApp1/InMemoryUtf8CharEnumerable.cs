#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Numerics;

/// <summary>
/// Decodes an in-memory source of bytes using utf8 encoding. Every enumerator produced
/// will utilize the same shared buffer for result characters.
/// </summary>
public ref struct InMemoryUtf8CharEnumerable<TSource, TLength, TOperators>
    : IUtf8CharEnumerable<InMemoryUtf8CharEnumerable<TSource, TLength, TOperators>, TSource, TLength, TOperators>, IEnumerator<Char>
    where TLength : unmanaged, IEqualityOperators<TLength, TLength, Boolean>// must be unmanaged to enable "new enumerator on copy"-semantics
    where TOperators : IByteSourceLengthOperators<TLength>
    where TSource : IByteSource<TSource, TLength, TOperators>, allows ref struct
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="source">
    /// The byte source to decode characters from.
    /// </param>
    /// <param name="buffer">
    /// The buffer to use when decoding characters. Its length must be at least <see cref="RequiredBufferLength"/>.
    /// </param>
    public InMemoryUtf8CharEnumerable(TSource source, Span<Char> buffer) : this()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, Utf8CharEnumerable.RequiredBufferLength);

        _source = source;
        _resultCharsBuffer = buffer;
    }

    private TSource _source;
    private TLength _bytesConsumed;

    private Span<Char> _resultCharsBuffer;
    private Int32 _resultCharsCursor;
    private Int32 _resultCharsCursorLimit;

    private static readonly UTF8Encoding _encoding = (UTF8Encoding)Encoding.UTF8;

    /// <inheritdoc/>
    public Char Current => _resultCharsBuffer[_resultCharsCursor];

    /// <inheritdoc/>
    public void Reset()
    {
        _bytesConsumed = default;
        _resultCharsCursor = 0;
        _resultCharsCursorLimit = 0;
    }

    /// <inheritdoc/>
    Object IEnumerator.Current => Current;
    /// <inheritdoc/>
    public void Dispose() { }
    /// <inheritdoc/>
    public Boolean MoveNext()
    {
        if(_resultCharsCursor < _resultCharsCursorLimit)
        {
            _resultCharsCursor++;
            return true;
        }

        Span<Byte> buffer = stackalloc Byte[_resultCharsBuffer.Length];
        TSource.CopyTo(_source, ref buffer, _bytesConsumed);

        if(buffer.Length == 0)
            return false;

        TOperators.Increment(ref _bytesConsumed, buffer.Length);

        _resultCharsCursor = 0;
        _resultCharsCursorLimit = _encoding.GetChars(buffer, _resultCharsBuffer) - 1;

        return _resultCharsCursorLimit > -1;
    }
    /// <summary>
    /// Gets a copy of this instance.
    /// </summary>
    /// <returns>
    /// A new copy of this enumerator.
    /// </returns>
    public InMemoryUtf8CharEnumerable<TSource, TLength, TOperators> GetEnumerator() => this;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource GetSource() => _source;
}
