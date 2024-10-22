namespace ConsoleApp1;

using System;
using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

/// <summary>
/// A <see cref="MemoryManager{Byte}"/> over a region of a memory mapped file. This <see cref="MemoryManager{Byte}"/>
/// becomes owner of the file and view, disposing it upon disposal of itself.
/// </summary>
public sealed unsafe class MappedFileMemoryManager : MemoryManager<Byte>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="view"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MappedFileMemoryManager(MemoryMappedViewAccessor view, MappedFileManager fileManager)
    {
        if(view.SafeMemoryMappedViewHandle.IsClosed || view.SafeMemoryMappedViewHandle.IsInvalid)
            throw new InvalidOperationException();

        _view = view;
        _fileManager = fileManager;
    }

    readonly MemoryMappedViewAccessor _view;
    readonly MappedFileManager _fileManager;

    /// <inheritdoc />
    public override Span<Byte> GetSpan()
    {
        Byte* ptr = null;
        _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        var len = _view.SafeMemoryMappedViewHandle.ByteLength;
        return new Span<Byte>(ptr, checked((Int32)len));
    }

    /// <inheritdoc />
    public override MemoryHandle Pin(Int32 elementIndex = 0)
    {
        if(elementIndex < 0 || elementIndex >= checked((Int32)_view.SafeMemoryMappedViewHandle.ByteLength))
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        Byte* ptr = null;
        _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        return new MemoryHandle(Unsafe.Add<Byte>(ptr, elementIndex));
    }

    /// <inheritdoc />
    public override void Unpin()
    {

    }

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing) => _view.Dispose();
}
