using System.Net;

namespace NaoBlocks.Utility.SocketHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // var address = ClientAddressList.RetrieveAddresses().FirstOrDefault()
            //     ?? IPAddress.Loopback;
            var address = IPAddress.Loopback;
            var endpoint = new IPEndPoint(
                address,
                5010);
            var server = new SocketListener();
            server.ClientConnected += (o, e) =>
            {
                Console.WriteLine($"Client connected: {e.RemoteEndPoint}");
            };
            server.ClientDisconnected += (o, e) =>
            {
                Console.WriteLine($"Client disconnected: {e.RemoteEndPoint}");
            };
            server.MessageReceived += (o, e) =>
            {
                Console.WriteLine($"Received {e.Type}");
            };

            Console.WriteLine($"Listening on {endpoint}");
            Console.WriteLine("Press Ctrl-C to exit");
            server.Start(endpoint);
        }
    }
}