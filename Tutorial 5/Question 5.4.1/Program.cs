using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Question_5._4._1
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 6000);
            server.Start();
            Console.WriteLine("Server started on port 5000...");

            TcpClient client = server.AcceptTcpClient();

            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Request request = JsonSerializer.Deserialize<Request>(requestJson);

            double result = 0;
            if (request.Method == "MoneyExchange")
            {
                result = MoneyExchange(request.Curency, request.Amount);
            }

            Response response = new Response { Result = result };
            string responseJson = JsonSerializer.Serialize(response);

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            stream.Write(responseBytes, 0, responseBytes.Length);

            client.Close();
            server.Stop();

            Console.ReadLine();
        }

        static double MoneyExchange(string curency, double amount)
        {
            double rate = curency == "USD" ? 26000 : 1;
            return amount * rate;
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
