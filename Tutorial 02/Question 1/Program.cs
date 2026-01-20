using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial_2
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
            Console.WriteLine("Structs vs Classes in C#");
            // structs thì nó là kiểu values type , còn class thì nó là tham chiếu type
            Console.WriteLine("-Strcts-");
            Point p1 = new Point(10, 20);
            Point p2 = p1; // Copying the value

            p2.X = 30;
            p2.Y = 40;
            Console.WriteLine($"p1: X={p1.X}, Y={p1.Y}");
            Console.WriteLine($"p2: X={p2.X}, Y={p2.Y}");

            Console.WriteLine("-Classes-");
            PointClass pc1 = new PointClass(10, 20);
            PointClass pc2 = pc1; // Copying the reference

            pc2.X = 30;
            pc2.Y = 40;
            Console.WriteLine($"pc1: X={pc1.X}, Y={pc1.Y}");
            Console.WriteLine($"pc2: X={pc2.X}, Y={pc2.Y}");

            Console.ReadLine();

        }
    }
}
