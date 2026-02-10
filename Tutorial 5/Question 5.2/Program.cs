using System;
using System.IO;
using System.IO.Pipes;

namespace Question_5._2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (NamedPipeServerStream pipeServer = 
                new NamedPipeServerStream("mypipe", PipeDirection.InOut))
            {
                pipeServer.WaitForConnection();
                Console.WriteLine("Client connected.");
                using (StreamReader reader = new StreamReader(pipeServer))
                using (StreamWriter writer = new StreamWriter(pipeServer))
                {
                    // Viec nay dam bao day du lieu tu server den client ngay lap tuc sau
                    writer.AutoFlush = true;

                    string message = reader.ReadLine();
                    Console.WriteLine("Received from client: " + message);

                    string response = "Hello from the server!";
                    writer.WriteLine(response);
                    Console.WriteLine("Sent to client: " + response);
                }
            }
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
