namespace Question_5._1.A
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int data = new Random().Next(1, 100);

            Console.WriteLine("Process A is running...");
            Console.WriteLine("Data created by Process A: " + data);
            Console.ReadLine();
        }
    }
}
