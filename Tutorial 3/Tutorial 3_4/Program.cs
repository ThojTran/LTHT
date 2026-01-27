using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Question_4
{
    internal class Program
    {
        const int WorkItems = 100;
        const int WorkDurationMs = 50;

        static async Task Main(string[] args)
        {
            // Catch async void exceptions for demonstration (console apps surface them here).
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine($"[UnhandledException] {e.ExceptionObject}");
            };

            Console.WriteLine("Question 4 — Multithreading and the Thread Pool\n");

            await RunComparisonAsync();
        }

        static async Task RunComparisonAsync()
        {
            Console.WriteLine($"Work items: {WorkItems}, per-item simulated work: {WorkDurationMs}ms\n");

            // Thread approach
            var threadResult = await MeasureExecutionAsync("Manual Threads", RunWithThreadsAsync);
            PrintResourceUsageSnapshot("Manual Threads", threadResult);

            // ThreadPool approach
            var poolResult = await MeasureExecutionAsync("ThreadPool", RunWithThreadPoolAsync);
            PrintResourceUsageSnapshot("ThreadPool", poolResult);

            // Task approach (Task.Run)
            var taskResult = await MeasureExecutionAsync("Tasks (Task.Run)", RunWithTasksAsync);
            PrintResourceUsageSnapshot("Tasks (Task.Run)", taskResult);
        }

        record RunMetrics(TimeSpan Elapsed, int PeakProcessThreadCount, int StartProcessThreadCount, int EndProcessThreadCount, (int workerAvailable, int workerMax) ThreadPoolInfo);

        static async Task<RunMetrics> MeasureExecutionAsync(string label, Func<CancellationToken, Task> runFunc)
        {
            using var cts = new CancellationTokenSource();
            var monitor = MonitorProcessThreadsAsync(cts.Token);

            // capture threadpool info before
            ThreadPool.GetAvailableThreads(out int availWorkerBefore, out int availIOCBefore);
            ThreadPool.GetMaxThreads(out int maxWorker, out int maxIOC);

            var sw = Stopwatch.StartNew();
            await runFunc(cts.Token);
            sw.Stop();

            // stop monitor and get peak
            cts.Cancel();
            int peakThreads = await monitor;

            int endThreads = Process.GetCurrentProcess().Threads.Count;
            int startThreads = Math.Max(1, peakThreads - 1); // best-effort (peak includes start)
            return new RunMetrics(sw.Elapsed, peakThreads, startThreads, endThreads, (availWorkerBefore, maxWorker));
        }

        static void PrintResourceUsageSnapshot(string label, RunMetrics m)
        {
            Console.WriteLine($"{label} -> Elapsed: {m.Elapsed.TotalMilliseconds:F0}ms, Peak process thread count: {m.PeakProcessThreadCount}, ThreadPool workers (available/max at start): {m.ThreadPoolInfo.workerAvailable}/{m.ThreadPoolInfo.workerMax}");
        }

        static async Task<int> MonitorProcessThreadsAsync(CancellationToken token)
        {
            int peak = Process.GetCurrentProcess().Threads.Count;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int current = Process.GetCurrentProcess().Threads.Count;
                    if (current > peak) peak = current;
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            return peak;
        }

        static void DoWork(int id, string label)
        {
            // Print managed thread id for each execution
            Console.WriteLine($"[{label}] Item {id} running on thread {Thread.CurrentThread.ManagedThreadId}");
            // Simulated work (CPU or blocking)
            Thread.Sleep(WorkDurationMs);
        }

        // --- Manual Threads ---
        static Task RunWithThreadsAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                var threads = new List<Thread>(WorkItems);

                for (int i = 0; i < WorkItems; i++)
                {
                    int capture = i;
                    var t = new Thread(() => DoWork(capture, "Thread"));
                    t.IsBackground = false;
                    threads.Add(t);
                }

                // start all
                foreach (var t in threads) t.Start();

                // join all
                foreach (var t in threads) t.Join();
            }, token);
        }

        // --- ThreadPool ---
        static Task RunWithThreadPoolAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            int remaining = WorkItems;

            for (int i = 0; i < WorkItems; i++)
            {
                int capture = i;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    DoWork(capture, "ThreadPool");
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        tcs.TrySetResult(null);
                    }
                });
            }

            return tcs.Task;
        }

        // --- Tasks (Task.Run) ---
        static Task RunWithTasksAsync(CancellationToken token)
        {
            var tasks = Enumerable.Range(0, WorkItems)
                                  .Select(i => Task.Run(() => DoWork(i, "Task"), token))
                                  .ToArray();
            return Task.WhenAll(tasks);
        }
    }
}
