namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Main application.
    /// </summary>
    public class App
        : IDisposable
    {
        private bool disposedValue;
        private InstructionFactory instructionFactory = new();

        /// <summary>
        /// Initialises a new instance of <see cref="App"/>.
        /// </summary>
        public App()
        {
            this.instructionFactory.Initialise<App>();
        }

        /// <summary>
        /// Initialise a new instance of <see cref="App"/> with a custom instruction source.
        /// </summary>
        /// <param name="instructionSource">A type in the instruction source.</param>
        public App(Type instructionSource)
        {
            this.instructionFactory.Initialise(instructionSource);
        }

        /// <summary>
        /// Disposes all the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="console">The console to log all output to.</param>
        /// <param name="args">The command line arguments for the application.</param>
        public async Task<int> RunAsync(IConsole console, string[] args)
        {
            var commandName = args[0];
            var instruction = this.instructionFactory.Retrieve(commandName);
            if (instruction == null)
            {
                console.WriteError($"Unknown command '{commandName}'");
                return -1;
            }

            if (!instruction.Validate(console, args)) return -1;
            var result = await instruction.RunAsync(console);
            return result;
        }

        /// <summary>
        /// Disposes all the resources used by this instance.
        /// </summary>
        /// <param name="disposing">True to dispose .Net resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                disposedValue = true;
            }
        }
    }
}