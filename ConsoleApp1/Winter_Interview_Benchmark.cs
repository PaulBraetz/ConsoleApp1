namespace ConsoleApp1;

using System;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

[ShortRunJob]
public class Winter_Interview_Benchmark
{
    [Params(1, 10, 100)]
    public Int32 DegreeOfParallelism { get; set; }
    [Params(1_000, 100_000, 1_000_000)]
    public Int32 IncrementCount { get; set; }

    [Benchmark]
    public async Task<Int32> InterviewAlgo_BrokenSample()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_BrokenSample(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }
    [Benchmark]
    public async Task<Int32> InterviewAlgo_InterlockedExchangeSimple()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_InterlockedExchangeSimple(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }
    [Benchmark]
    public async Task<Int32> InterviewAlgo_InterlockedExchange()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_InterlockedExchange(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }
    [Benchmark(Baseline = true)]
    public async Task<Int32> InterviewAlgo_InterlockedIncrement()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_InterlockedIncrement(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }
    [Benchmark]
    public async Task<Int32> InterviewAlgo_LockInner()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_LockInner(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }
    [Benchmark]
    public async Task<Int32> InterviewAlgo_LockOuter()
    {
        var b = 0;
        var tasks = new Task[DegreeOfParallelism];

        for(var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => DoSomething_LockOuter(ref b));
        }

        await Task.WhenAll(tasks);

        return b;
    }

    void DoSomething_BrokenSample(ref Int32 b)
    {
        for(var i = 0; i < IncrementCount; i++)
            b++;
    }
    void DoSomething_InterlockedExchangeSimple(ref Int32 b)
    {
        var original = b;
        var local = original;
        for(var i = 0; i < IncrementCount; i++)
        {
            local++;
        }

        while(Interlocked.CompareExchange(ref b, local, original) != original)
        {
            original = b;
            local = original;
            for(var i = 0; i < IncrementCount; i++)
            {
                local++;
            }
        }
    }
    void DoSomething_InterlockedExchange(ref Int32 b)
    {
        var original = b;
        var local = original;
        for(var i = 0; i < IncrementCount; i++)
        {
            local++;
        }

        if(Interlocked.CompareExchange(ref b, local, original) != original)
        {
            DoSomething_InterlockedIncrement(ref b);
        }
    }
    void DoSomething_InterlockedIncrement(ref Int32 b)
    {
        for(var i = 0; i < IncrementCount; i++)
        {
            _ = Interlocked.Increment(ref b);
        }
    }
    readonly Object _doSomethingLockInnerSyncRoot = new();
    void DoSomething_LockInner(ref Int32 b)
    {
        for(var i = 0; i < IncrementCount; i++)
        {
            lock(_doSomethingLockInnerSyncRoot)
            {
                b++;
            }
        }
    }
    readonly Object _doSomethingLockOuterSyncRoot = new();
    void DoSomething_LockOuter(ref Int32 b)
    {
        for(var i = 0; i < IncrementCount; i++)
        {
            lock(_doSomethingLockOuterSyncRoot)
            {
                b++;
            }
        }
    }
}
