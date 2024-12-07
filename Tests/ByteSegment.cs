namespace ConsoleApp1;

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

sealed class ByteSegment : ReadOnlySequenceSegment<Byte>
{
    public ByteSegment(Memory<Byte> memory) => Memory = memory;

    [DisallowNull]
    public ByteSegment? Previous
    {
        get;
        set
        {
            field = value;
            value.Next = this;
            RunningIndex += value.RunningIndex + value.Memory.Length;
        }
    }
}
