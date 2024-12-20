﻿namespace ConsoleApp1;

using System;
using System.Text;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[ShortRunJob]
public class Utf8EnumerableInputLengthBenchmark
{
    [Params(100, 1000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000)]
    public Int32 Length { get; set; }

    private static readonly String[] _strings = ["a", "Z", "1", "!", " ", "	", "\n", "\r", "\"", "'", "\\", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "=", "+", "[", "]", "{", "}", "|", ";", ":", ",", ".", "<", ">", "/", "?", "`", "~", "á", "é", "í", "ó", "ú", "ñ", "Ñ", "ç", "Ç", "ü", "ö", "ä", "ë", "ï", "✓", "✔", "✗", "✘", "“", "”", "‘", "’", "😀", "😃", "😄", "😁", "😆", "😂", "🤣", "😅", "😇", "🏳", "🌈", "🌍", "🌎", "🌏", "👨", "💻", "👩", "П", "р", "и", "в", "е", "т", "м", "こ", "ん", "に", "ち", "は", "안", "녕", "하", "세", "요", "你", "好", "م", "ر", "ح", "ب", "ا", "​", "￿", "́", " ", "𝄞", "𝄢", "𝄩", "H", "e", "l", "o", "W", "r", "d", "2", "3", "4", "5", "6", "7", "8", "9", "0", "¡", "¿", "C", "m", "s", "t", "b", "c", "è", "ê", "ē", "ė", "ę", "‍", "🐍", "🦀", "🐘", "🐿", "️", "i", "n", "g", "M", "k", "h", "S", "p", "F", "q", "u", "𐍈", "𐍉", "𐍊", "𐍋", "𐍌", "A", "B", "D", "E", "G", "I", "J", "K", "L", "N", "O", "P", "Q", "R", "T", "U", "V", "X", "Y", "f", "j", "v", "w", "x", "y", "z", "🕵", "♂", "♀"];
    [GlobalSetup]
    public void Setup() => _input = GenerateRandomUtf8Bytes(Length);
    [GlobalCleanup]
    public void Cleanup() { /*Thread.Sleep(60_000_000);*/ }

    public static Byte[] GenerateRandomUtf8Bytes(Int32 n)
    {
        var random = new Random(0);
        var sb = new StringBuilder(n);

        for(var i = 0; i < n; i++)
        {
            var randomChar = _strings[random.Next(_strings.Length)];
            _ = sb.Append(randomChar);
        }

        var inputStr = sb.ToString();
        var result = Encoding.UTF8.GetBytes(inputStr);

        return result;
    }

    private Byte[] _input = [];

    [Benchmark(Baseline = true)]
    public Int32 RunNaive()
    {
        var s = ( (UTF8Encoding)Encoding.UTF8 ).GetString(_input);
        var result = 0;
        foreach(var c in s)
            result++;

        return result;
    }
    [Benchmark]
    public Int32 RunCharEnumerator()
    {
        var s = ( (ReadOnlySpan<Byte>)_input ).ToCharEnumerable();
        var result = 0;

        foreach(var c in s)
            result++;

        return result;
    }
    [Benchmark]
    public Int32 RunCharEnumeratorStackalloc()
    {
        var s = ( (ReadOnlySpan<Byte>)_input ).ToCharEnumerable(stackalloc Char[Utf8CharEnumerable.RecommendedBufferLength]);
        var result = 0;

        foreach(var c in s)
            result++;

        return result;
    }
}
