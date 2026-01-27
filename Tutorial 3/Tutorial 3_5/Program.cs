using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Question_5
{
    internal class Program
    {
        const int WorkItems = 100;
        const int WorkDurationMs = 50;
        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine($"[UnhandledException] {e.ExceptionObject}");
            };

            Console.WriteLine("\nQuestion 5 — Async/Await and Common Pitfalls\n");

            await DemoAsyncAwaitPitfalls();
        }
        static async Task DemoAsyncAwaitPitfalls()
        {
            Console.WriteLine("1) Proper async/await example (improves CPU utilization):");
            Console.WriteLine("   Starting awaited long-running operation...");
            var sw = Stopwatch.StartNew();
            await LongRunningOperationAsync(1); // await frees the current thread to do other work while awaiting
            sw.Stop();
            Console.WriteLine($"   Awaited call completed in {sw.Elapsed.TotalMilliseconds:F0}ms\n");

            Console.WriteLine("Explanation: awaiting an async Task releases the calling thread while the operation is pending, allowing the CPU to schedule other work. This reduces thread blocking and improves scalability.");

            Console.WriteLine("\n2) async void should be avoided:");
            try
            {
                // The try/catch will NOT catch exceptions thrown inside an async void method.
                BadAsyncVoid();
                Console.WriteLine("   Called async void method; it runs fire-and-forget. Exceptions from it are not catchable by the caller.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("   Unexpectedly caught: " + ex);
            }

            // Give the async void a chance to run (for demo only)
            await Task.Delay(300);

            Console.WriteLine("\n3) Blocking on async with Task.Result / Task.Wait (bad):");
            Console.WriteLine("   Starting a long-running operation and blocking on Result (this will block current thread)...");
            var blockingSw = Stopwatch.StartNew();
            try
            {
                // This blocks the current thread until the task completes. In synchronization-context environments (UI, ASP.NET classic),
                // this can cause a deadlock. It also blocks a thread, wasting resources.
                var t = LongRunningOperationAsync(2);
                t.Wait(); // or use t.Result
                blockingSw.Stop();
                Console.WriteLine($"   Blocking wait completed in {blockingSw.ElapsedMilliseconds}ms");
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("   AggregateException from blocking wait: " + ae.Flatten().InnerException);
            }
        }

        static async Task LongRunningOperationAsync(int id)
        {
            Console.WriteLine($"   [LongRunningOperation {id}] started on thread {Thread.CurrentThread.ManagedThreadId}");
            // Simulate I/O-bound or asynchronous waiting work — Task.Delay does not occupy a thread while waiting.
            await Task.Delay(1000).ConfigureAwait(false);
            Console.WriteLine($"   [LongRunningOperation {id}] finished on thread {Thread.CurrentThread.ManagedThreadId}");
        }

        static async void BadAsyncVoid()
        {
            // async void example — do not use in library code or where caller needs to observe completion/exceptions
            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}
