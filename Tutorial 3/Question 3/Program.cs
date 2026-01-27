using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Question_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Question 3 — Writing Memory-Efficient Code");
            Question3_Demo();
        }
        public static void Question3_Demo()
        {
            const int count = 50_000;

            Console.WriteLine("Original approach: allocate many buffers with LINQ/List");
            long before = GC.GetTotalMemory(false);
            Original_BufferAllocation(count, 256);
            long after = GC.GetTotalMemory(false);
            Console.WriteLine($"Original approach memory delta (approx): {FormatBytes(after - before)}");

            // Force a GC to reduce background noise for measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine();

            Console.WriteLine("Refactored approach: arrays + ArrayPool + for-loop (reused buffers)");
            before = GC.GetTotalMemory(false);
            Refactored_BufferAllocation_WithArrayPool(count, 256);
            after = GC.GetTotalMemory(false);
            Console.WriteLine($"Refactored approach memory delta (approx): {FormatBytes(after - before)}");
        }

        // Original: uses LINQ and List which causes many allocations (buffers and LINQ iterator/closures)
        private static void Original_BufferAllocation(int count, int size)
        {
            // This pattern allocates `count` byte[] arrays and a List to hold them.
            // It also creates LINQ iterator objects if Enumerable.Range/Select are used.
            var list = new List<byte[]>(capacity: count);
            for (int i = 0; i < count; i++)
            {
                // allocate new buffer -> many heap allocations
                byte[] buf = new byte[size];
                // simulate use
                buf[0] = (byte)(i & 0xFF);
                list.Add(buf);
            }

            // Simulate finishing work and releasing references:
            list.Clear(); // remove references so GC can reclaim the arrays
        }

        // Refactored: uses ArrayPool to rent buffers and a single pre-sized array for references
        private static void Refactored_BufferAllocation_WithArrayPool(int count, int size)
        {
            // Pre-size an array to hold references (no growth/resizing)
            byte[][] buffers = new byte[count][];

            // Rent buffers from ArrayPool (reduces allocations)
            for (int i = 0; i < count; i++)
            {
                byte[] buf = ArrayPool<byte>.Shared.Rent(size); // may be larger than requested
                // Use only the requested portion of the buffer
                buf[0] = (byte)(i & 0xFF);
                buffers[i] = buf;
            }

            // Simulate work done with buffers...

            // Return buffers immediately to the pool to avoid keeping them alive
            for (int i = 0; i < count; i++)
            {
                ArrayPool<byte>.Shared.Return(buffers[i], clearArray: false);
                buffers[i] = null;
            }
        }

        // Helpers
        private static string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            if (bytes >= MB) return $"{bytes / (double)MB:N2} MB";
            if (bytes >= KB) return $"{bytes / (double)KB:N2} KB";
            return $"{bytes} B";
        }
    }
}
