using System.ComponentModel;
using System.Diagnostics;

namespace Question_3
{
    internal class Program
    {
        public static int Add(int a, int b)
        {
            return a + b;
        }
        static void Main(string[] args)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int sum = Add(4, 6);
            sw.Stop();
            Console.WriteLine($"Sum: {sum}");
            Console.WriteLine($"Time taken: {sw.ElapsedTicks} ticks");
            Console.ReadLine();
        }
    }
}
