namespace ConsoleApp1;

using System;
using System.IO;
using System.IO.MemoryMappedFiles;

public sealed class MappedFileManager : IDisposable
{
    MappedFileManager(MemoryMappedFile file)
    {
        _file = file;

        if(_file.SafeMemoryMappedFileHandle.IsClosed || _file.SafeMemoryMappedFileHandle.IsInvalid)
            throw new InvalidOperationException();
    }

    readonly MemoryMappedFile _file;

    public static MappedFileManager Create(String path)
    {
        var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open);
        var result = new MappedFileManager(file);

        return result;
    }

    //0 has special meaning for view accessor
    const Int64 _defaultSize = 0;

    public MappedFileMemoryManager CreateView() => CreateView(0, _defaultSize);
    public MappedFileMemoryManager CreateView(Int64 offset, Int64 size) =>
        new(_file.CreateViewAccessor(offset, size, MemoryMappedFileAccess.Read), this);

    public void Dispose() => _file.Dispose();
}
