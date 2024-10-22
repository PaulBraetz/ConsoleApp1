namespace ConsoleApp1;

using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Spectre.Console;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration.Attributes;
using BenchmarkDotNet.Jobs;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, launchCount: 10, iterationCount: 1, id: "ColdStart")]
[ShortRunJob(RuntimeMoniker.Net80)]
public class CsvReaderBenchmark
{
    [GlobalSetup]
    public void GlobalSetup()
    {
        _path = Path.GetTempFileName();
        File.WriteAllLines(
            _path,
            Enumerable.Range(1, 1_000_000)
                .Select(i => "0000021000,2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh")
                .Prepend("Id,Timestamp,Measurement,Unit"));
        _ = File.ReadAllBytes(_path);
        GC.Collect();
    }

    [GlobalCleanup]
    public void GlobalCleanup() => File.Delete(_path);

    String _path;

    [Benchmark(Baseline = true)]
    public Double Sep()
    {
        var reader = new SepReader(_path);

        var sum = 0d;
        foreach(var (_, _, measurement) in reader.Read())
            sum += measurement;

        return sum;
    }
    struct CsvHelperRecord
    {
        public Int64 Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Double Measurement { get; set; }
    }
    [Benchmark]
    public Double CsvHelper()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };
        using var reader = new StreamReader(_path);
        using var csv = new CsvReader(reader, config);
        var result = csv.GetRecords<CsvHelperRecord>().Sum(r => r.Measurement);

        return result;
    }
    [Benchmark]
    public Double StreamReader()
    {
        using var reader = new StreamReader(_path);
        Double result = 0;
        while(reader.ReadLine() is { } line)
        {
            if(line.Split(',') is [{ } idStr, { } timestampStr, { } measurementStr, ..]
            && Int64.TryParse(idStr, CultureInfo.InvariantCulture, out var id)
            && DateTimeOffset.TryParse(timestampStr, CultureInfo.InvariantCulture, out var timestamp)
            && Double.TryParse(measurementStr, CultureInfo.InvariantCulture, out var measurement))
            {
                result += measurement;
            }
        }

        return result;
    }
    [Benchmark]
    public Double SepParallelized()
    {
        var reader = new SepReader(_path);

        var sum = reader.Read().AsParallel().Sum(t => t.measurement);

        return sum;
    }
    //[Benchmark]
    public Double SequenceReader()
    {
        using var fileManager = MappedFileManager.Create(_path);
        using var viewManager = fileManager.CreateView();
        var reader = CsvSequenceReader.Create(viewManager);
        var enumerator = new RecordEnumerator(reader);

        var sum = 0d;
        foreach(var (_, _, measurement) in enumerator)
            sum += measurement;

        return sum;
    }
}
