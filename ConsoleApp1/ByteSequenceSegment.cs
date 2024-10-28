namespace ConsoleApp1;

using System;
using System.Buffers;
using System.Diagnostics;

sealed class ByteSequenceSegment : ReadOnlySequenceSegment<Byte>, IDisposable
{
    public ByteSequenceSegment(Memory<Byte> memory, IMemoryOwner<Byte>? owner = null)
    {
        Debug.Assert(memory.Length > 0, "memory should not be zero-length");
        _owner = owner;
        Memory = memory;
    }

    private readonly IMemoryOwner<Byte>? _owner;

    public ByteSequenceSegment Prepend(Memory<Byte> memory, IMemoryOwner<Byte>? owner = null)
    {
        var head = new ByteSequenceSegment(memory, owner)
        {
            RunningIndex = 0,
            Next = this
        };

        var cur = this;
        while(cur != null)
        {
            cur.RunningIndex += memory.Length;
            cur = cur.Next as ByteSequenceSegment;
        }

        return head;
    }

    public void Dispose()
    {
        ( Next as IDisposable )?.Dispose();

        _owner?.Dispose();
    }
}
