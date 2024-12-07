namespace ConsoleApp1;

using System;
using System.Text;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class Utf8EncodingBenchmark
{
    private static readonly Byte[] _input = new Byte[] { 72, 101, 108, 108 };
    private static readonly UTF8Encoding _strongEncoding = (UTF8Encoding)Encoding.UTF8;
    private static readonly Encoding _weakEncoding = Encoding.UTF8;

    [Params(1_000,1_000_000,10_000_000)]
    public Int32 Iterations { get; set; }

    [Benchmark]
    public Int32 RunGetCharsStrong()
    {
        var result = 0;
        Span<Char> buffer = stackalloc Char[32];

        for(var i = 0; i < Iterations; i++)
            result += _strongEncoding.GetChars(_input, buffer);

        return result;
    }
    [Benchmark]
    public Int32 RunGetCharsWeak()
    {
        var result = 0;
        Span<Char> buffer = stackalloc Char[32];

        for(var i = 0; i < Iterations; i++)
            result += _weakEncoding.GetChars(_input, buffer);

        return result;
    }
}
