namespace ConsoleApp1;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.CodeAnalysis;
using DotNetIsolator;
using Wasmtime;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Spectre.Console;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using System.Buffers;
using System.Text;

internal class Program
{
    private static async Task Main(String[] args) => RunDynamicallyAllocatedBufferBenchmark();

    static void RunDynamicallyAllocatedBufferBenchmark()
    {
#if DEBUG
        foreach(var data in DynamicallyAllocatedBufferBenchmark.Data)
        {
            Debug.Assert(new DynamicallyAllocatedBufferBenchmark().Run((Int32)data[0], (Int32)data[1]) == (Int32)data[1]);
        }
#else
        var summaries = BenchmarkRunner.Run(
        [
            typeof(DynamicallyAllocatedBufferBenchmark)
        ]);
        foreach(var summary in summaries)
        {
            _ = Process.Start("explorer", summary.LogFilePath);
        }
#endif
    }

    record struct Record(Int32 Id, Double Value, String Day);

    static void ParseCsv()
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(
            path,
            Enumerable.Range(1, 10)
                .Select(i => $"{i:D10},2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh")
                .Prepend("id,timestamp,measurement,unit"));
        {
            using var fileManager = MappedFileManager.Create(path);

            const Int32 requestedPageSize = 4096;
            var viewIndex = 0;

            for(var remainder = fileManager.Length; remainder > 0; remainder -= requestedPageSize)
            {
                var pageSize = Math.Min(remainder, requestedPageSize);
                var offset = fileManager.Length - remainder;
                var viewManager = fileManager.CreateView(offset, pageSize);
                //TODO
            }
        }

        File.Delete(path);
    }
    static void RunUtf8Benchmark()
    {
        var summaries = BenchmarkRunner.Run(
        [
            typeof(Utf8EnumerableBufferLengthBenchmark),
            typeof(Utf8EnumerableInputLengthBenchmark)
        ]);
        foreach(var summary in summaries)
        {
            _ = Process.Start("explorer", summary.LogFilePath);
        }
    }

    static void RunUtf8Decoder()
    {
        var encoding = Encoding.UTF8;
        var inputString = String.Join(' ', Enumerable.Range(0, 10).Select(i => $"Hello, World {i}!"));
        ReadOnlySpan<Byte> source = encoding.GetBytes(inputString);
        Span<Char> buffer = stackalloc Char[Utf8CharEnumerable.RequiredBufferLength];

        foreach(var c in source.ToCharEnumerable(buffer))
            Console.Write(c);
    }
    static void RunSlidingWindowCsvReader()
    {
        var path = Path.GetTempFileName();
        File.WriteAllLines(
            path,
            Enumerable.Range(1, 10)
                .Select(i => $"{i:D10},2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh")
                .Prepend("id,timestamp,measurement,unit"));
        {
            using var fileManager = MappedFileManager.Create(path);

            const Int32 size = 4096;
            var viewIndex = 0;

            List<(Memory<Byte> memory, IMemoryOwner<Byte>? owner)> head = [];

            for(var remainder = fileManager.Length; remainder > 0; remainder -= size)
            {
                var pageSize = Math.Min(remainder, size);
                var offset = fileManager.Length - remainder;
                var viewManager = fileManager.CreateView(offset, pageSize);

                const Int32 tailScanSequenceSize = 512;
                Byte[] newLine = [(Byte)'\r', (Byte)'\n'];

                var tailReader = new SequenceReader<Byte>(new(viewManager.Memory));
                tailReader.AdvanceToEnd();
                while(true)
                {
                    var scanSequenceSize = checked((Int32)Math.Min(tailReader.Consumed, tailScanSequenceSize));
                    if(scanSequenceSize == 0)
                        break; //no newline in entire window

                    tailReader.Rewind(scanSequenceSize);
                    if(!tailReader.TryReadExact(scanSequenceSize, out var scanSequence))
                        throw new InvalidOperationException(); //we should never get here

                    tailReader.Rewind(scanSequenceSize);

                    var newLineIndex = scanSequence.FirstSpan.LastIndexOf(newLine);
                    if(newLineIndex == -1)
                        continue; //no newline in current scan sequence

                    tailReader.Advance(newLineIndex);
                    tailReader.Advance(newLine.Length);

                    break;
                }

                var tailBegin = checked((Int32)tailReader.Consumed);
                var tailLength = viewManager.Memory.Length - tailBegin;
                var bodyBegin = 0;
                var bodyLength = tailBegin;

                (Memory<Byte> memory, IMemoryOwner<Byte> owner)? ourTail = null;

                if(tailLength > 0)
                    ourTail = (viewManager.Memory.Slice(tailBegin, tailLength), viewManager);

                var (ourBodyMemory, ourBodyOwner) = tailLength > 0
                    ? (viewManager.Memory.Slice(bodyBegin, bodyLength), null)
                    : (viewManager.Memory, viewManager);

                if(ourBodyMemory.Length == 0)
                    throw new InvalidOperationException("view size too small");

                var segment = new ByteSequenceSegment(ourBodyMemory, ourBodyOwner);
                var last = segment;
                var endIndex = ourBodyMemory.Length;
                for(var i = head.Count - 1; i >= 0; i--)
                {
                    var (memory, owner) = head[i];
                    segment = segment?.Prepend(memory, owner) ?? ( last = new(memory, owner) );
                    endIndex += memory.Length;
                }

                ReadOnlySequence<Byte> sequence = new(segment!, startIndex: 0, last, endIndex: ourBodyMemory.Length);
                //var channelItem = (sequence, owner:segment);

                var reader = CsvSequenceReader.Create(sequence);
                var enumerator = new RecordEnumerator(reader);

                Console.WriteLine($"page {viewIndex++},offset {offset}, pagesize {pageSize}, bodySize {ourBodyMemory.Length}+{head.Sum(t => t.memory.Length)}, tail {( ourTail.HasValue ? ourTail.Value.memory.Length : 0 )}");
                var enumeratedOnce = false;
                foreach(var record in enumerator)
                {
                    enumeratedOnce = true;
                    Console.WriteLine(record);
                }

                if(enumeratedOnce)
                {
                    segment.Dispose();
                    head.Clear();
                } else if(ourBodyMemory.Length > 0)
                {
                    head.Add((ourBodyMemory, ourBodyOwner));
                }

                if(ourTail.HasValue)
                    head.Add(ourTail.Value);
            }
        }

        File.Delete(path);
    }

    static void RunCsvReaderBenchmark()
    {
        Console.WriteLine($"{"reference",-16}: {6.634817514544766 * 1e6}");
        new (String, Func<CsvReaderBenchmark, Double>)[]
        {
            (nameof(CsvReaderBenchmark.CsvHelper),b=>b.CsvHelper()),
            (nameof(CsvReaderBenchmark.Sep),b=>b.Sep()),
            (nameof(CsvReaderBenchmark.SepParallelized),b=>b.SepParallelized()),
            (nameof(CsvReaderBenchmark.StreamReader),b=>b.StreamReader()),
            (nameof(CsvReaderBenchmark.SequenceReader),b=>b.SequenceReader())
        }.ToList()
        .ForEach(t =>
        {
            var benchmark = new CsvReaderBenchmark();
            benchmark.GlobalSetup();
            var sum = t.Item2.Invoke(benchmark);
            benchmark.GlobalCleanup();
            Console.WriteLine($"{t.Item1,-16}: {sum}");
        });

        var summary = BenchmarkRunner.Run<CsvReaderBenchmark>();
        //Process.Start("explorer", summary.LogFilePath).WaitForExit();
    }

    static async Task Run_Winter_InterviewAlgoBenchmark()
    {
        var summary = BenchmarkRunner.Run<Winter_Interview_Benchmark>();
        await Process.Start("explorer", summary.LogFilePath).WaitForExitAsync();
    }

    private static void RunIsolatedPluginMvp()
    {
        //set up source
        const String source =
            """
        namespace MyPluginPackage;
        using ConsoleApp1;
        using System;
        public class Plugin : IPlugin
        {
            public void Execute(string arg)
            {
                Console.WriteLine($"Hello from plugin! Arg: {arg}");
            }
        }
        """;

        //parse using roslyn
        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(
                LanguageVersion.Latest,
                DocumentationMode.None,
                SourceCodeKind.Regular),
            cancellationToken: CancellationToken.None);

        //compile using roslyn
        using var peStream = new MemoryStream();
        var pluginAssemblyName = "PluginAssembly";
        var emitResult = CSharpCompilation.Create(
            assemblyName: pluginAssemblyName,
            new SyntaxTree[] { tree },
            [
                MetadataReference.CreateFromFile(typeof(IPlugin).Assembly.Location),
            ..Basic.Reference.Assemblies.NetStandard20.References.All
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)
                .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release))
            .Emit(peStream);

        if(!emitResult.Success)
        {
            foreach(var diagnostic in emitResult.Diagnostics)
            {
                Console.WriteLine(diagnostic);
            }

            return;
        }

        //retrieve plugin
        var pluginAssemblyBytes = peStream.ToArray();
        var pluginAssembly = Assembly.Load(pluginAssemblyBytes);
        var pluginType = pluginAssembly.GetTypes().Single(t => t.IsAssignableTo(typeof(IPlugin)));

        //set up runtime
        using var host = new IsolatedRuntimeHost()
            .WithBinDirectoryAssemblyLoader()
            .WithAssemblyLoader(s => s == pluginAssemblyName ? pluginAssemblyBytes : null)
            .WithWasiConfiguration(new WasiConfiguration()
                .WithInheritedStandardOutput());
        using var runtime = new IsolatedRuntime(host);

        //execute plugin
        var arg = "Arg from outside the sandbox.";
        runtime.CreateObject(
            assemblyName: pluginAssemblyName,
            @namespace: pluginType.Namespace,
            className: pluginType.Name)
            .InvokeVoid(nameof(IPlugin.Execute), arg);
    }
    public static Task MyAlgoAsync() => MyAlgoCore(periodicallyYield: true);
    public static void MyAlgo() => MyAlgoCore(periodicallyYield: false).Wait();
    private static async Task MyAlgoCore(Boolean periodicallyYield)
    {
        for(var i = 0; i < 1000000; i++)
        {
            if(periodicallyYield)
            {
                await Task.Yield();
            }
            //do some stuff
        }
    }
    static Task CancelDisplay()
    {
        ConcurrentDictionary<String, Int32> t = [];
        //t.TryUpdate("path", );

        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, args) =>
        {
            _ = tcs.TrySetResult();
            args.Cancel = true;
        };
        return AnsiConsole.Console.Status().StartAsync("running", async ctx =>
        {
            await tcs.Task;
            ctx.Status = "canceled";
            await Task.Delay(Timeout.InfiniteTimeSpan);
        });
    }
    static async Task RunSequential()
    {
        var cts = new CancellationTokenSource(5000);
        while(true)
        {
            try
            {
                Console.Write(">> ");
                var input = await AsyncConsole.ReadLineAsync(cts.Token);
                Console.WriteLine($"<< {input}");
            } catch(OperationCanceledException)
            {
                Console.WriteLine("Cancelled");
                break;
            }
        }
    }
    static async Task RunParallel()
    {
        var readGateSource = new TaskCompletionSource();
        var readGate = readGateSource.Task;
        Console.WriteLine("Initializing");
        var readTasks = Enumerable.Range(0, 3)
            .Select(async index =>
            {
                Console.WriteLine($"{index} Init");
                await readGate;
                try
                {
                    Console.WriteLine($"{index} Read");
                    var cts = new CancellationTokenSource(Random.Shared.Next(1000, 5000));
                    var line = await AsyncConsole.ReadLineAsync(cts.Token);
                    Console.WriteLine($"{index} Line: {line}");
                } catch(Exception ex)
                {
                    Console.WriteLine($"{index} Exception: {ex.Message}");
                }
            })
            .ToList();
        Console.WriteLine("Releasing");
        readGateSource.SetResult();
        await Task.WhenAll(readTasks);
    }
    static class AsyncConsole
    {
        private static readonly Object _inFlightLock = new();

        private static Task<String?> ReadLineTask
        {
            get
            {
                var inFlightReadLine = field;

                if(inFlightReadLine != null)
                    return inFlightReadLine;

                lock(_inFlightLock)
                {
                    if(field != null)
                        return field;

                    inFlightReadLine = Task.Run(ReadLine);
                    field = inFlightReadLine;

                    return inFlightReadLine;
                }
            }

            set;
        }
        private static String? ReadLine()
        {
            var result = Console.ReadLine();
            ReadLineTask = null;
            return result;
        }
        public static async Task<String?> ReadLineAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource();
            _ = cancellationToken.Register(tcs.SetResult);
            var readLineTask = ReadLineTask;
            _ = await Task.WhenAny(tcs.Task, readLineTask);
            cancellationToken.ThrowIfCancellationRequested();
            return readLineTask.Result;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    sealed class DataContractValidationAttribute : Attribute;
    [DataContract]
    sealed class DataContractFoo
    {
        [DataMember]
        public String PropertyOne { get; set; } = "FooBar";
        [DataMember]
        public Dictionary<String, String> Properties { get; set; } =
            new()
            {
                ["Key 1"] = "FooBar1",
                ["Key 2"] = "FooBar2"
            };
        [DataContractValidation]
        void Validate()
        {
            if(Properties.Any(kvp => kvp.Key.Length > 5))
                throw new InvalidOperationException();
        }
    }
}
