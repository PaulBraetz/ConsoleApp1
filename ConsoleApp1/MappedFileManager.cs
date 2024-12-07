namespace ConsoleApp1;

using System;
using System.IO;
using System.IO.MemoryMappedFiles;

public sealed class MappedFileManager : IDisposable
{
    MappedFileManager(MemoryMappedFile file, Int64 length)
    {
        _file = file;

        if(_file.SafeMemoryMappedFileHandle.IsClosed || _file.SafeMemoryMappedFileHandle.IsInvalid)
            throw new InvalidOperationException();

        Length = length;
    }

    readonly MemoryMappedFile _file;
    public Int64 Length { get; }
    // 0 has special meaning for view accessor
    const Int64 _defaultSize = 0;

    public static MappedFileManager Create(String path)
    {
        var rs = File.OpenRead(path);
        var file = MemoryMappedFile.CreateFromFile(
            rs,
            mapName: null,
            capacity: 0,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            leaveOpen: false);

        var result = new MappedFileManager(file, rs.Length);

        return result;
    }

    public FileView CreateView() => CreateView(0, _defaultSize);
    public FileView CreateView(Int64 offset, Int64 size) =>
        new(_file.CreateViewAccessor(offset, size, MemoryMappedFileAccess.Read), this, offset);

    public void Dispose() => _file.Dispose();
}
