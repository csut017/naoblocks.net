using NaoBlocks.Common;
using System.Net;

namespace NaoBlocks.Utility.SocketHost
{
    internal class Program
    {
        private readonly Dictionary<int, Client> clients = new();
        private int nextClient = 0;

        public async Task Run(string[] args)
        {
            var address = IPAddress.Loopback;
            if (!args.Any(arg => "-local".Equals(arg)))
            {
                address = ClientAddressList.RetrieveAddresses().FirstOrDefault()
                    ?? IPAddress.Loopback;
            }

            var endpoint = new IPEndPoint(
                address,
                5010);
            var server = new SocketListener(endpoint);
            server.ClientConnected += (o, e) =>
            {
                Console.WriteLine($"[Client connected: {e.RemoteEndPoint}]");
                Console.Write(">");
                e.Index = nextClient++;
                clients.Add(e.Index, e);
            };
            server.ClientDisconnected += (o, e) =>
            {
                Console.WriteLine($"[Client disconnected: {e.RemoteEndPoint}]");
                Console.Write(">");
                clients.Remove(e.Index);
            };
            server.MessageReceived += (o, e) =>
            {
                Console.WriteLine($"[Received {e.Type}]");
                foreach (var kvp in e.Values)
                {
                    Console.WriteLine($"* {kvp.Key}={kvp.Value}");
                }
                Console.Write(">");
            };

            Console.WriteLine("NaoBlocks.Net SocketHost Utility");
            Console.WriteLine($"Listening on {endpoint}");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Factory.StartNew(() => server.Start(), TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await this.CommandProcessingLoop();
            server.Close();
        }

        private static async Task Main(string[] args)
        {
            var app = new Program();
            await app.Run(args);
        }

        private async Task CommandProcessingLoop()
        {
            var stopExecution = false;
            while (!stopExecution)
            {
                Console.Write(">");
                var input = Console.ReadLine()?
                    .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    ?? Array.Empty<string>();
                if (input.Length == 0) continue;

                switch (input[0])
                {
                    case "exit":
                        stopExecution = true;
                        break;

                    case "send":
                        await this.HandleSendCommand(input[1..]);
                        break;

                    case "list":
                        this.HandleListCommand();
                        break;

                    default:
                        Console.WriteLine($"Unknown command '{input[0]}'");
                        break;
                }
            }
        }

        private void HandleListCommand()
        {
            if (this.clients.Count == 0)
            {
                Console.WriteLine("There are no clients connected");
                return;
            }

            Console.WriteLine("The following clients are connected:");
            foreach (var client in this.clients.Values)
            {
                Console.WriteLine($"* {client.FullName}");
            }
        }

        private async Task HandleSendCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid number of arguments. Expect send <client> <command> [<alues>]");
                return;
            }

            if (!int.TryParse(args[0], out var clientNumber))
            {
                Console.WriteLine("Invalid client argument. Expected a whole number");
                return;
            }

            if (!this.clients.TryGetValue(clientNumber, out var client))
            {
                Console.WriteLine($"Invalid client argument. Client ${clientNumber} does not exist");
                return;
            }

            if (!int.TryParse(args[1], out var commandType))
            {
                Console.WriteLine("Invalid command argument. Expected a whole number");
                return;
            }

            var message = new ClientMessage(
                (ClientMessageType)commandType);
            foreach (var arg in args[2..])
            {
                var equalPos = arg.IndexOf('=');
                if (equalPos < 0)
                {
                    message.Values.Add(arg, string.Empty);
                }
                else
                {
                    message.Values.Add(arg[0..equalPos], arg[(equalPos + 1)..]);
                }
            }

            await client.SendMessageAsync(message, TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);
            Console.WriteLine($"Sent {message.Type} to client {client.FullName}");
        }
    }
}