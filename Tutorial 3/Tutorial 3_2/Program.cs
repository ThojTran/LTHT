using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Question_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Question 2 — Garbage Collection and Memory Pressure");
            Question2_Demo();
        }
        private static void Question2_Demo()
        {
            const int iterations = 200_000;
            const int bufferSize = 512; // smallish allocation

            Console.WriteLine($"Allocating {iterations} arrays of {bufferSize} bytes each.");

            // Observe memory before allocations:
            long beforeAlloc = GC.GetTotalMemory(false);
            Console.WriteLine($"Memory before allocations: {FormatBytes(beforeAlloc)}");

            List<byte[]> list = new List<byte[]>(iterations);
            for (int i = 0; i < iterations; i++)
            {
                // allocate many small byte[] arrays on the managed heap
                list.Add(new byte[bufferSize]);
            }

            long afterAlloc = GC.GetTotalMemory(false);
            Console.WriteLine($"Memory after allocations (no GC): {FormatBytes(afterAlloc)}");

            // Drop references to make allocations eligible for GC:
            list = null;

            // Memory before forced GC:
            long justBeforeGc = GC.GetTotalMemory(false);
            Console.WriteLine($"Memory just before forced GC: {FormatBytes(justBeforeGc)}");

            // Force garbage collection for observation (discouraged in production)
            GC.Collect();        // forces collection of all generations
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long afterGc = GC.GetTotalMemory(true);
            Console.WriteLine($"Memory after forced GC: {FormatBytes(afterGc)}");

            // Small pause so the console output can be read when running inside IDE:
            Thread.Sleep(250);
        }
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
