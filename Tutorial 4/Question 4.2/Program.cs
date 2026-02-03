using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Question_4._2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("\nQuestion 2 — Task Coordination and Synchronization\n");
            await Question2_SynchronizationDemoAsync();

        }

        // Question 2
        private static async Task Question2_SynchronizationDemoAsync()
        {
            var rnd = Random.Shared;

            // Helper factory to create a task that simulates work
            Func<int, Task> workTask = async id =>
            {
                int delay = rnd.Next(200, 1200);
                await Task.Delay(delay);
                Console.WriteLine($"Task {id} completed after {delay} ms");
            };

            // A) Task.WhenAll
            Console.WriteLine("Using Task.WhenAll:");
            Task[] tasksA = { workTask(1), workTask(2), workTask(3) };
            await Task.WhenAll(tasksA);
            Console.WriteLine("All tasks finished (Task.WhenAll)\n");

            // B) CountdownEvent
            Console.WriteLine("Using CountdownEvent:");
            using var countdown = new CountdownEvent(3);
            for (int i = 1; i <= 3; i++)
            {
                int id = i;
                _ = Task.Run(async () =>
                {
                    await workTask(id);
                    countdown.Signal();
                });
            }

            // Wait synchronously (block) until CountdownEvent reaches zero
            countdown.Wait();
            Console.WriteLine("All tasks finished (CountdownEvent)\n");

            // C) ManualResetEventSlim
            Console.WriteLine("Using ManualResetEventSlim:");
            using var mres = new ManualResetEventSlim(false);
            int remaining = 3;
            for (int i = 1; i <= 3; i++)
            {
                int id = i;
                _ = Task.Run(async () =>
                {
                    await workTask(id);
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        mres.Set();
                    }
                });
            }

            // Wait until last task sets the event
            mres.Wait();
            Console.WriteLine("All tasks finished (ManualResetEventSlim)\n");
        }
    }
}