namespace ConsoleApp1;

using System;
using System.Threading;
using System.Buffers;

sealed class MemoryOwnerLifetime(IMemoryOwner<Byte> owner) : IMemoryOwner<Byte>
{
    private Int32 _referenceCount;
    public void Increment() => _referenceCount++;
    public Memory<Byte> Memory => owner.Memory;
    public void Dispose()
    {
        if(Interlocked.Decrement(ref _referenceCount) == 0)
            owner.Dispose();
    }
}
