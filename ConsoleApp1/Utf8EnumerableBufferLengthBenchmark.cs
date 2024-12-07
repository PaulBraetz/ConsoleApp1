namespace ConsoleApp1;

using System;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[ShortRunJob]
public class Utf8EnumerableBufferLengthBenchmark
{
    [Params(32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536)]
    public Int32 BufferLength { get; set; }

    [GlobalSetup]
    public void Setup() => _input = Utf8EnumerableInputLengthBenchmark.GenerateRandomUtf8Bytes(1_000_000);
    [GlobalCleanup]
    public void Cleanup() { /*Thread.Sleep(60_000_000);*/ }

    private Byte[] _input = [];

    [Benchmark]
    public Int32 RunCharEnumerator()
    {
        var s = ( (ReadOnlySpan<Byte>)_input ).ToCharEnumerable(BufferLength);
        var result = 0;

        foreach(var c in s)
            result++;

        return result;
    }
    [Benchmark]
    public Int32 RunCharEnumeratorStackalloc()
    {
        var s = ( (ReadOnlySpan<Byte>)_input ).ToCharEnumerable(stackalloc Char[BufferLength]);
        var result = 0;

        foreach(var c in s)
            result++;

        return result;
    }
}