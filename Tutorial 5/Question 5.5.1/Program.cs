using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Question_5._5._1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 6000);
            server.Start();
            Console.WriteLine("Server started on port 5000...");

            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[2048];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Request request = JsonSerializer.Deserialize<Request>(requestJson);

            Response response = new Response()
            {
                rpc = "2.0",
                id = request.id
            };
            try
            {
                switch (request.method)
                {
                    case "MoneyExchange":
                        string curency = request.@params.GetProperty("Curency").GetString();
                        double amount = request.@params.GetProperty("Amount").GetDouble();
                        double result = MoneyExchange(curency, amount);
                        response.result = result;
                        break;
                    case "Add":
                        double a = request.@params.GetProperty("A").GetDouble();
                        double b = request.@params.GetProperty("B").GetDouble();
                        response.result = a + b;
                        break;
                    default:
                        response.error = new Error { code = -32601, message = "Method not found" };
                        break;
                }
            }
            catch (Exception ex)
            {
                response.error = new Error { code = -32602, message = "Invalid params: " + ex.Message };
            }

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
}