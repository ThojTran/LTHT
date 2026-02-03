using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Question_4._1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Question 1 — Race Conditions and Thread Safety\n");
            await Question1_RaceConditionDemoAsync();
        }

        // -----------------------
        // Question 1
        // -----------------------
        private static async Task Question1_RaceConditionDemoAsync()
        {
            const int tasksCount = 5;
            const int incrementsPerTask = 100_000;
            int expected = tasksCount * incrementsPerTask;

            // 1) Unsynchronized (will typically be incorrect)
            int counter = 0;
            Task[] tasks = new Task[tasksCount];
            for (int t = 0; t < tasksCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < incrementsPerTask; i++)
                    {
                        // Race: read-modify-write without synchronization
                        counter = counter + 1;
                    }
                });
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"Unsynchronized counter: {counter} (expected {expected})");

            // 2) Using lock
            counter = 0;
            object sync = new();
            for (int t = 0; t < tasksCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < incrementsPerTask; i++)
                    {
                        lock (sync)
                        {
                            counter = counter + 1;
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"Counter with lock: {counter} (expected {expected})");

            // 3) Using Interlocked.Increment
            counter = 0;
            for (int t = 0; t < tasksCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < incrementsPerTask; i++)
                    {
                        Interlocked.Increment(ref counter);
                    }
                });
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"Counter with Interlocked.Increment: {counter} (expected {expected})");
        }
    }
}
