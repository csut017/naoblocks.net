using System.Net;

namespace NaoBlocks.Utility.SocketHost
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // var address = ClientAddressList.RetrieveAddresses().FirstOrDefault()
            //     ?? IPAddress.Loopback;
            var address = IPAddress.Loopback;
            var endpoint = new IPEndPoint(
                address,
                5010);
            var server = new SocketListener();
            server.MessageReceived += (o, e) =>
            {
                Console.WriteLine($"Received {e.Type}");
            };

            Console.WriteLine($"Listening on {endpoint}");
            Console.WriteLine("Press Ctrl-C to exit");
            await server.StartAsync(endpoint);
        }
    }
}