using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Question_5._5._2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 6000);
            NetworkStream stream = client.GetStream();

            var response = new 
            {
                rpc = "2.0",
                id = 1,
                method = "MoneyExchange",
                @params = new 
                {
                    Curency = "USD",
                    Amount = 100.0
                }
            };

            string jsonRequest = JsonSerializer.Serialize(response);
            byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

            stream.Write(data, 0, data.Length);
            Console.WriteLine("Client sent RPC request");

            byte[] buffer = new byte[2048];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Exchange result: " + jsonResponse);

            client.Close();
            Console.ReadLine();
        }
    }
    public class Request
    {
            public string rpc { get; set; }
            public int id { get; set; }
            public string method { get; set; }
            public JsonElement @params { get; set; }
    }

    public class Response
    {
            public string rpc { get; set; }
            public int id { get; set; }
            public object result { get; set; }
            public Error error { get; set; }
    }

    public class Error
    {
            public int code { get; set; }
            public string message { get; set; }
    }
}
