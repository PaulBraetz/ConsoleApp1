namespace ConsoleApp1;

using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;

/// <summary>
/// Represents a dynamically allocated buffer that may be initially allocated on
/// the stack and promoted to a heap allocated buffer if needed.
/// </summary>
/// <typeparam name="T">
/// The type of element managed.
/// </typeparam>
public ref struct DynamicallyAllocatedBuffer<T>
{
    /// <summary>
    /// Allocates a new instance.
    /// </summary>
    /// <param name="initialBuffer">
    /// The initial buffer to use. This may be stack-allocated by the consumer.
    /// </param>
    public DynamicallyAllocatedBuffer(Span<T> initialBuffer) => _span = initialBuffer;

    private Span<T> _span;
    private ArrayBufferWriter<T>? _heapAllocatedBuffer = null;
    private Int32 _cursor;

    /// <summary>
    /// Gets a span of the values currently in the buffer.
    /// </summary>
    public Span<T> Span => _span[.._cursor];

    /// <summary>
    /// Adds a new element to the end of the buffer, resizing it if necessary.
    /// If the length required to accommodate the new element exceeds the
    /// current buffer, it will be resized. If the buffer was previously
    /// stack-allocated but needs resizing, a new, heap-allocated buffer is
    /// created.
    /// </summary>
    /// <param name="element">
    /// The element to add to the buffer.
    /// </param>
    public void Add(T element)
    {
        EnsureLength(1);
        _span[_cursor] = element;
        _heapAllocatedBuffer?.Advance(1);
        _cursor++;
    }

    private void EnsureLength(Int32 length)
    {
        if(_cursor + length - _span.Length is > 0 and { } delta)
            Resize(delta);
    }

    private void Resize(Int32 delta)
    {
        var requiredSize = checked((Int32)BitOperations.RoundUpToPowerOf2((UInt32)( _span.Length + delta )));

        if(_heapAllocatedBuffer is null)
        {
            _heapAllocatedBuffer = new(requiredSize);
            var heapSpan = _heapAllocatedBuffer.GetSpan(requiredSize);
            _span.CopyTo(heapSpan);
            _span = heapSpan;
        } else
        {
            _span = _heapAllocatedBuffer.GetSpan(_span.Length * 2);
        }
    }

    /// <summary>
    /// Adds a new element to the end of the buffer, resizing it if necessary.
    /// If the length required to accommodate the new element exceeds the
    /// current buffer, it will be resized. If the buffer was previously
    /// stack-allocated but needs resizing, a new, heap-allocated buffer is
    /// created.
    /// </summary>
    /// <param name="element">
    /// The element to add to the buffer.
    /// </param>
    public void Add(ReadOnlySpan<T> element)
    {
        if(_cursor >= _span.Length)
        {
            if(_heapAllocatedBuffer is null)
            {
                _heapAllocatedBuffer = new(_span.Length * 2);
                var heapSpan = _heapAllocatedBuffer.GetSpan(_span.Length * 2);
                _span.CopyTo(heapSpan);
                _span = heapSpan;
            } else
            {
                _span = _heapAllocatedBuffer.GetSpan(_span.Length * 2);
            }
        }

        _span[_cursor] = element;
        _heapAllocatedBuffer?.Advance(1);
        _cursor++;
    }
}
