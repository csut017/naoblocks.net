using Output = System.Console;

namespace NaoBlocks.Client.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Output.WriteLine("Starting connection");
            var conn = new Connection();
            conn.ConnectAsync("localhost:5000", "one", false).Wait();
            conn.DisconnectAsync().Wait();
        }
    }
}
