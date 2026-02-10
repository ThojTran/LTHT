using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

namespace Question_5._3._1
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 5000;
            // Đây là đại diện của máy chủ nó sẽ trỏ đến localhost và cổng 5000
            TcpListener sever = new TcpListener(IPAddress.Loopback, port);
            sever.Start();

            Console.WriteLine("Sever listening" + port);

            TcpClient client = sever.AcceptTcpClient();

            NetworkStream stream = client.GetStream();
            // tạo 1 mạng byte dung lượng 1kb
            byte[] buffer = new byte[1024];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Received message from client: " + message);

            string response = "Hello, Client!";
            byte[] responseData = Encoding.UTF8.GetBytes(response);
            stream.Write(responseData, 0, responseData.Length);
            
            client.Close();
            sever.Stop();

            Console.ReadLine();

        }
    }
}
