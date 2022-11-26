using System.Diagnostics.CodeAnalysis;

namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Main program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args">The arguments for the program.</param>
        [ExcludeFromCodeCoverage]
        private static async Task Main(string[] args)
        {
            using var app = new App();
            await app.RunAsync(
                new StandardConsole(),
                args);
        }
    }
}