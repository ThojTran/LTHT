using System.Diagnostics;

namespace Question_2
{
    struct Point
    {
        public int X;
        public int Y;
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    class PointClass
    {
        public int X;
        public int Y;
        public PointClass(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            int size = 1000000;
            // Struct array
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long startStruct = GC.GetTotalMemory(true);
            Point[] structArray = new Point[size];
            for (int i = 0; i < size; i++)
            {
                structArray[i] = new Point(i, i);
            }
            long endStruct = GC.GetTotalMemory(true);
            Console.WriteLine($"Struct array memory usage: {endStruct - startStruct} bytes");
            
            Stopwatch stopwatchStruct = Stopwatch.StartNew();

            long structSum = 0;
            for (int i = 0; i < size; i++)
            {
                structSum += structArray[i].X;
            }
            stopwatchStruct.Stop();
            Console.WriteLine($" Time taken: {stopwatchStruct.ElapsedMilliseconds} ms");

            // Class array
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long startClass = GC.GetTotalMemory(true);
            PointClass[] classArray = new PointClass[size];
            for (int i = 0; i < size; i++)
            {
                classArray[i] = new PointClass(i, i);
            }
            long endClass = GC.GetTotalMemory(true);
            Console.WriteLine($"Class array memory usage: {endClass - startClass} bytes");
            
            Stopwatch stopwatchClass = Stopwatch.StartNew();
            
            long classSum = 0;
            for (int i = 0; i < size; i++)
            {
                classSum += classArray[i].X;
            }
            stopwatchClass.Stop();
            Console.WriteLine($" Time taken: {stopwatchClass.ElapsedMilliseconds} ms");

        }
    }
}
