using System;
using System.Net;
using System.Net.Sockets;


namespace Question_5._3._2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string serverIp = "127.0.0.1";
            int port = 5000;

            TcpClient client = new TcpClient();
            client.Connect(serverIp, port);

            NetworkStream stream = client.GetStream();

            string message = "Hello, Server!";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Message sent to the server: " + message);

            // tạo 1 mạng byte dung lượng 1kb
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received response from server: " + response);
            client.Close();
            Console.ReadLine();
        }
    }
}
