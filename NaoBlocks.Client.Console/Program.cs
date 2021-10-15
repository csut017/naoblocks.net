using NaoBlocks.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Client.Console
{
    class Program
    {
        private static readonly Queue<AstNode> steps = new Queue<AstNode>();
        private static AstNode currentStep = null;

        static void Main(string[] args)
        {
            WriteMessage("Starting connection...");
            var conn = new Connection("localhost:5000", "one", false);
            conn.ConnectAsync().Wait();
            WriteMessage("...connected");
            MainLoop(conn).Wait();
            WriteMessage("Shutting down...");
            conn.DisconnectAsync().Wait();
            WriteMessage("...done");
        }

        // This method is the main loop for the program. It will monitor for incoming messages and respond.
        private async static Task MainLoop(Connection conn)
        {
            // We need to handle in-coming messages and do something when they arrive
            var cancellation = new CancellationTokenSource();
            await Task.Factory.StartNew(() => conn.OnMessageReceived.Subscribe(async msg => await MessageProcessor(msg, conn), cancellation.Token),
                cancellation.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            // And we also need to handle user input
            var input = string.Empty;
            var opts = new AstNode.DisplayOptions
            {
                ExcludeArguments = true,
                ExcludeChildren = true
            };
            while (input != "quit")
            {
                System.Console.Write(">");
                input = System.Console.ReadLine().ToLowerInvariant();

                switch (input)
                {
                    case "help":
                        ShowHelp();
                        break;

                    case "quit":
                        cancellation.Cancel();
                        break;

                    case "stop":
                        await conn.SendMessageAsync(new ClientMessage(ClientMessageType.ProgramFinished));
                        await conn.SendMessageAsync(new ClientMessage(ClientMessageType.RobotStateUpdate, new { state = "Waiting" }));
                        steps.Clear();
                        WriteMessage("Sent finished message", ConsoleColor.Yellow);
                        break;

                    case "step":
                        if (currentStep != null)
                        {
                            var values = new
                            {
                                sourceID = currentStep.SourceId,
                                status = "end",
                                function = currentStep.Token.Value
                            };
                            await conn.SendMessageAsync(new ClientMessage(ClientMessageType.RobotDebugMessage, values));
                            WriteMessage($"Sent step stop message: {currentStep.ToString(opts)}", ConsoleColor.Yellow);
                            currentStep = null;
                        }
                        else if (steps.TryDequeue(out AstNode step))
                        {
                            var values = new { 
                                sourceID = step.SourceId,
                                status = "start",
                                function = step.Token.Value
                            };
                            await conn.SendMessageAsync(new ClientMessage(ClientMessageType.RobotDebugMessage, values));
                            currentStep = step;
                            WriteMessage($"Sent step start message: {currentStep.ToString(opts)}", ConsoleColor.Yellow);
                        } else
                        {
                            WriteMessage("!! No more steps to send !!", ConsoleColor.Red);
                        }
                        break;

                    case "steps":
                        var count = 0;
                        foreach (var step in steps)
                        {
                            WriteMessage($"{count,4}: {step.ToString(opts)}", ConsoleColor.Gray);
                            ++count;
                        }
                        break;

                    default:
                        WriteMessage($"!! Unknown command: {input} !!", ConsoleColor.Red);
                        break;
                }
            }
        }

        private static void ShowHelp()
        {
            WriteMessage("TODO: show some sort of help information");
        }

        private async static Task MessageProcessor(ClientMessage msg, Connection conn)
        {
            WriteMessage(string.Empty);
            WriteMessage($"Received message {msg.Type}", ConsoleColor.Green);

            switch (msg.Type)
            {
                case ClientMessageType.Authenticated:
                    // We can ignore this message as it is just a confirmation that authentication was successful
                    break;

                case ClientMessageType.DownloadProgram:
                    var program = await conn.RetrieveCodeAsync(msg);
                    await conn.SendMessageAsync(new ClientMessage(ClientMessageType.ProgramDownloaded));
                    steps.Clear();
                    LoadProgram(program);
                    break;

                case ClientMessageType.StartProgram:
                    await conn.SendMessageAsync(new ClientMessage(ClientMessageType.ProgramStarted));
                    await conn.SendMessageAsync(new ClientMessage(ClientMessageType.RobotStateUpdate, new { state = "Running" }));
                    break;

                case ClientMessageType.StopProgram:
                    await conn.SendMessageAsync(new ClientMessage(ClientMessageType.ProgramStopped));
                    await conn.SendMessageAsync(new ClientMessage(ClientMessageType.RobotStateUpdate, new { state = "Waiting" }));
                    steps.Clear();
                    break;

                default:
                    WriteMessage($"!! Unhandled message {msg.Type} !!", ConsoleColor.Red);
                    break;
            }

            System.Console.Write(">");
        }

        private static void LoadProgram(IEnumerable<AstNode> program)
        {
            foreach (var node in program)
            {
                if (node.Arguments != null) LoadProgram(node.Arguments);

                if (!string.IsNullOrEmpty(node.SourceId))
                {
                    steps.Enqueue(node);
                }

                if (node.Children != null) LoadProgram(node.Children);
            }
        }

        private static void WriteMessage(string message, ConsoleColor colour = ConsoleColor.White)
        {
            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colour;
            try
            {
                System.Console.WriteLine(message);
            }
            finally
            {
                System.Console.ForegroundColor = current;
            }
        }
    }
}
