using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Question_4._3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("\nQuestion 3 — Thread-Safe File Access\n");
            await Question3_ThreadSafeFileAccessDemoAsync();
        }

        // Question 3
        private static async Task Question3_ThreadSafeFileAccessDemoAsync()
        {
            const int writerTasks = 5;
            const int linesPerTask = 200;
            string unsyncPath = "log_unsync.txt";
            string lockPath = "log_lock.txt";
            string queuePath = "log_queued.txt";

            // Ensure files are clean
            File.Delete(unsyncPath);
            File.Delete(lockPath);
            File.Delete(queuePath);

            // A)
            using (var sharedStream = new StreamWriter(new FileStream(unsyncPath, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                sharedStream.AutoFlush = true;
                Task[] tasks = new Task[writerTasks];
                for (int t = 0; t < writerTasks; t++)
                {
                    int id = t;
                    tasks[t] = Task.Run(() =>
                    {
                        for (int i = 0; i < linesPerTask; i++)
                        {
                            sharedStream.WriteLine($" Writer {id} - line {i}");
                        }
                    });
                }

                await Task.WhenAll(tasks);
            }

            Console.WriteLine($"Unsynchronized writes completed to {unsyncPath} (check file for interleaving or partial lines).");

            // B) 
            object fileLock = new();
            Task[] lockTasks = new Task[writerTasks];
            for (int t = 0; t < writerTasks; t++)
            {
                int id = t;
                lockTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < linesPerTask; i++)
                    {
                        lock (fileLock)
                        {
                            File.AppendAllText(lockPath, $"[Lock] Writer {id} - line {i}{Environment.NewLine}");
                        }
                    }
                });
            }

            await Task.WhenAll(lockTasks);
            Console.WriteLine($"Synchronized writes using lock completed to {lockPath}.");

            // C) 
            using var queue = new BlockingCollection<string>(boundedCapacity: 10_000);
            var logger = Task.Run(() =>
            {
                using var writer = new StreamWriter(new FileStream(queuePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
                foreach (var message in queue.GetConsumingEnumerable())
                {
                    writer.WriteLine(message);
                }
            });

            Task[] producerTasks = new Task[writerTasks];
            for (int t = 0; t < writerTasks; t++)
            {
                int id = t;
                producerTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < linesPerTask; i++)
                    {
                        queue.Add($"[Queued] Writer {id} - line {i}");
                    }
                });
            }

            await Task.WhenAll(producerTasks);
            queue.CompleteAdding();
            await logger; // wait for consumer to finish
            Console.WriteLine($"Synchronized writes using a dedicated logger (queue) completed to {queuePath}.");
        }
    }
}