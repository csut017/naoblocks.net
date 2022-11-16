namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Main application.
    /// </summary>
    public class App
        : IDisposable
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public const string ApplicationName = "NaoBlocks";

        private bool disposedValue;
        private InstructionFactory instructionFactory = new();
        private Func<string, InstructionBase?> instructionResolver;

        /// <summary>
        /// Initialises a new instance of <see cref="App"/>.
        /// </summary>
        public App()
        {
            this.instructionFactory.Initialise<App>();
            this.instructionResolver = name => this.instructionFactory.Retrieve(name);
        }

        /// <summary>
        /// Initialise a new instance of <see cref="App"/> with a custom instruction source.
        /// </summary>
        /// <param name="instructionSource">A type in the instruction source.</param>
        public App(Type instructionSource)
        {
            this.instructionFactory.Initialise(instructionSource);
            this.instructionResolver = name => this.instructionFactory.Retrieve(name);
        }

        /// <summary>
        /// Initialise a new instance of <see cref="App"/> with an instruction resolver.
        /// </summary>
        /// <param name="instructionResolver">The instruction resolver.</param>
        public App(Func<string, InstructionBase?> instructionResolver)
        {
            this.instructionResolver = instructionResolver;
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
            var commandName = args.Length > 0
                ? args[0]
                : Instructions.Help.InstructionName;
            var instruction = this.instructionResolver(commandName);
            if (instruction == null)
            {
                console.WriteError($"Unknown command '{commandName}'");
                return -1;
            }

            instruction.Factory = this.instructionFactory;
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