using System;
using System.IO;
using System.IO.Pipes;

// Day se la client
namespace Question_5._2._1
{
    internal class Program
    {
        static void Main()
        {
            using (NamedPipeClientStream pipeClient = 
                new NamedPipeClientStream(".", "mypipe", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to server...");
                pipeClient.Connect();
                Console.WriteLine("Connected to server.");
                using (StreamReader reader = new StreamReader(pipeClient))
                using (StreamWriter writer = new StreamWriter(pipeClient))
                {
                    // Viec nay dam bao day du lieu tu client den server ngay lap tuc sau
                    writer.AutoFlush = true;
                    string message = "Hello from the client!";
                    writer.WriteLine(message);
                    Console.WriteLine("Sent to server: " + message);
                    string response = reader.ReadLine();
                    Console.WriteLine("Received from server: " + response);
                }
            }
            Console.WriteLine("Sever Close");
            Console.ReadLine();
        }
    }
}
