using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Question_5._4._2
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 6000);
            NetworkStream stream = client.GetStream();

            Request response = new Request
            {
                Method = "MoneyExchange",
                Curency = "USD",
                Amount = 99
            };

            string jsonRequest = JsonSerializer.Serialize(response);
            byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

            stream.Write(data, 0, data.Length);
            Console.WriteLine("Client sent RPC request");

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Response serverResponse = JsonSerializer.Deserialize<Response>(jsonResponse);

            Console.WriteLine("Exchange result: " + jsonResponse);

            client.Close();
            Console.ReadLine();
        }
        public class Request
        {
            public string Method { get; set; }
            public string Curency { get; set; }
            public double Amount { get; set; }

        }

        public class Response
        {
            public double Result { get; set; }
        }
    }

}
