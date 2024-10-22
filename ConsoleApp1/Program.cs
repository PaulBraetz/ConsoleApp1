namespace ConsoleApp1;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.CodeAnalysis;
using DotNetIsolator;
using ConsoleApp1;
using Wasmtime;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Spectre.Console;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using System.IO.MemoryMappedFiles;
using System.Globalization;

internal class Program
{
    private static async Task Main() => RunCsvReaderBenchmark();
    static void RunSlidingWindowCsvReader()
    {
        //var path = Path.GetTempFileName();
        //File.WriteAllLines(
        //    path,
        //    Enumerable.Range(1, 1_000_000)
        //        .Select(i => "0000021000,2024-10-21T20:46:18.1937080+00:00,6.634817514544766,kWh")
        //        .Prepend("id,timestamp,measurement,unit"));

        //using var fileManager = MappedFileManager.Create(path);

        //const Int32 size = 32;

        //using var viewManager1 = fileManager.CreateView(offset:0, size:size);
        //var reader1 = CsvSequenceReader.Create(viewManager1);
        //var enumerator1 = new RecordEnumerator(reader1);
        //foreach(var record in enumerator1)
        //    Console.WriteLine(record);

        //viewManager1.

        //File.Delete(path);
    }
    static void RunCsvReaderBenchmark()
    {
        Console.WriteLine($"reference: {( 6.634817514544766 * 1e6 ).ToString(CultureInfo.InvariantCulture)}");
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
            Console.WriteLine($"{t.Item1,-16}: {sum}");
        });

        var summary = BenchmarkRunner.Run<CsvReaderBenchmark>();
        Process.Start("explorer", summary.LogFilePath).WaitForExit();
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
        private static Task<String?>? _inFlightReadLine;
        private static Task<String?> ReadLineTask
        {
            get
            {
                var inFlightReadLine = _inFlightReadLine;

                if(inFlightReadLine != null)
                    return inFlightReadLine;

                lock(_inFlightLock)
                {
                    if(_inFlightReadLine != null)
                        return _inFlightReadLine;

                    inFlightReadLine = Task.Run(ReadLine);
                    _inFlightReadLine = inFlightReadLine;

                    return inFlightReadLine;
                }
            }
        }
        private static String? ReadLine()
        {
            var result = Console.ReadLine();
            _inFlightReadLine = null;
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