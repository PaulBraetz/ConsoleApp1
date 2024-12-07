#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Collections.Generic;
using System.Collections;

public ref partial struct BufferedUtf8CharEnumerable<TSource, TLength, TOperators>
{
    /// <summary>
    /// Enumerates chars by wrapping a <see cref="ByteSpanUtf8CharEnumerator"/> and using a rented buffer.
    /// </summary>
    public ref struct Enumerator : IEnumerator<Char>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="source">The byte source to enumerate chars from.</param>
        /// <param name="buffer">The buffer to use when storing intermediate results.</param>
        public Enumerator(TSource source, RentedArray<Char> buffer)
        {
            _buffer = buffer;
            _enumerator = new(source, buffer.Values);
        }

        private RentedArray<Char> _buffer;
        private InMemoryUtf8CharEnumerable<TSource, TLength, TOperators> _enumerator;

        /// <inheritdoc/>
        public Char Current => _enumerator.Current;
        /// <inheritdoc/>
        public Boolean MoveNext() => _enumerator.MoveNext();
        /// <inheritdoc/>
        public void Reset() => _enumerator.Reset();
        /// <inheritdoc/>
        Object IEnumerator.Current => _enumerator.Current;
        /// <inheritdoc/>
        public void Dispose()
        {
            _enumerator.Dispose();
            _buffer.Dispose();
        }
    }
}
