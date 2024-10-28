namespace ConsoleApp1;

using System;
using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Diagnostics.Tracing.Parsers.AspNet;

/// <summary>
/// A <see cref="MemoryManager{Byte}"/> over a region of a memory mapped file. This <see cref="MemoryManager{Byte}"/>
/// becomes owner of the file and view, disposing it upon disposal of itself.
/// </summary>
public sealed unsafe class FileView : MemoryManager<Byte>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="view"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FileView(MemoryMappedViewAccessor view, MappedFileManager fileManager, Int64 requestedOffset)
    {
        if(view.SafeMemoryMappedViewHandle.IsClosed || view.SafeMemoryMappedViewHandle.IsInvalid)
            throw new InvalidOperationException();

        _view = view;
        _fileManager = fileManager;
        _view.SafeMemoryMappedViewHandle.AcquirePointer(ref _ptr);
        //_requestedOffset = requestedOffset;
    }

    readonly MemoryMappedViewAccessor _view;
    readonly MappedFileManager _fileManager;

    readonly Byte* _ptr;
    //readonly Int64 _requestedOffset;

    private void* AdjustedPointer
    {
        get
        {
            //view was aligned to previous page size multiple
            //essentially left shifted
            //we calculate the difference via the requested offset, which could be further right
            var requiredOffset = checked((Int32)/*(_requestedOffset - */_view.PointerOffset/*)*/);
            //realign our pointer to the requested offset
            var result = Unsafe.Add<Byte>(_ptr, requiredOffset);

            return result;
        }
    }
    private Int32 AdjustedByteLength => checked((Int32)( _view.SafeMemoryMappedViewHandle.ByteLength - (UInt64)_view.PointerOffset ));

    /// <inheritdoc />
    public override Span<Byte> GetSpan() => new(AdjustedPointer, AdjustedByteLength);

    /// <inheritdoc />
    public override MemoryHandle Pin(Int32 elementIndex = 0)
    {
        if(elementIndex < 0 || elementIndex >= AdjustedByteLength)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        var result = new MemoryHandle(Unsafe.Add<Byte>(AdjustedPointer, elementIndex));

        return result;
    }

    /// <inheritdoc />
    public override void Unpin()
    {

    }

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        _view.SafeMemoryMappedViewHandle.ReleasePointer();
        _view.Dispose();
    }
}
