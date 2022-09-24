using NaoBlocks.Common;
using NaoBlocks.Parser;
using System.Reactive.Subjects;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Contains a representation of a robot's state during program execution.
    /// </summary>
    public class Engine
    {
        private readonly List<EngineFunction> currentFunctions = new();
        private readonly List<EngineFunction> defaultFunctions = new();
        private readonly Dictionary<string, EngineFunction> functions = new();
        private EngineRunOptions? options;
        private Subject<ClientMessage>? statusUpdateSubject;

        /// <summary>
        /// Initialises a new instance of <see cref="Engine"/>.
        /// </summary>
        public Engine()
        {
            AddBaseLevelEngineFunctions(this.currentFunctions);
            this.IndexFunctions();
        }

        /// <summary>
        /// Initialises a new instance of <see cref="Engine"/> with a set of default functions.
        /// </summary>
        /// <param name="defaultFunctions">The default functions to add.</param>
        public Engine(params EngineFunction[] defaultFunctions)
            : this()
        {
            var checkedFunctions = new List<EngineFunction>();
            AddDefaultEngineFunctions(checkedFunctions);
            var current = checkedFunctions.ToDictionary(f => f.Name);
            foreach (var newFunc in defaultFunctions)
            {
                if (current.ContainsKey(newFunc.Name))
                {
                    throw new EngineException($"Function '{newFunc.Name}' is already defined, each function name must be unique");
                }

                current[newFunc.Name] = newFunc;
            }

            this.defaultFunctions.AddRange(defaultFunctions);
        }

        /// <summary>
        /// Gets the currently registered functions for the engine.
        /// </summary>
        public IReadOnlyCollection<EngineFunction> CurrentFunctions
        {
            get { return this.currentFunctions.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the current node in the program.
        /// </summary>
        /// <remarks>If the node is -1, then it means the engine is not running.</remarks>
        public int CurrentNode { get; private set; } = -1;

        /// <summary>
        /// The program for the engine.
        /// </summary>
        public AstProgram? Program { get; private set; }

        /// <summary>
        /// Attempts to find a function.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <returns>The <see cref="EngineFunction"/> instance if found; null otherwise.</returns>
        public EngineFunction? FindFunction(string name)
        {
            return this.functions.TryGetValue(name, out var function)
                ? function
                : null;
        }

        /// <summary>
        /// Initialises the engine by compiling a program.
        /// </summary>
        /// <param name="code">The program code for the engine.</param>
        public async Task InitialiseAsync(string code)
        {
            var parser = CodeParser.New(code);
            var ast = await parser.ParseAsync();
            if (ast.Errors.Any())
            {
                var errors = string.Join($"{Environment.NewLine}* ", ast.Errors.Select(e => e.ToString()));
                var message = $"Unable to parse program code:{Environment.NewLine}* {errors}";
                throw new EngineException(message);
            }

            await this.InitialiseAsync(ast.Nodes);
        }

        /// <summary>
        /// Initialises the engine by loading a collection of <see cref="AstNode"/> instances.
        /// </summary>
        /// <param name="nodes">The nodes for the program.</param>
        public Task InitialiseAsync(ICollection<AstNode> nodes)
        {
            this.Program = AstProgram.New(nodes);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets the engine.
        /// </summary>
        public Task ResetAsync()
        {
            this.currentFunctions.Clear();
            AddDefaultEngineFunctions(this.currentFunctions);
            this.currentFunctions.AddRange(this.defaultFunctions);
            this.IndexFunctions();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the current program.
        /// </summary>
        /// <param name="options">The run options.</param>
        /// <returns>An <see cref="IObservable{ClientMessage}"/> containing the status updates.</returns>
        public Task<IObservable<ClientMessage>> StartAsync(EngineRunOptions? options = null)
        {
            // Validate we have a program first
            if (this.Program == null) throw new EngineException("Cannot start execution: no program loaded");

            // Initialise the engine state
            this.options = options ?? new EngineRunOptions();
            this.statusUpdateSubject = new();
            IObservable<ClientMessage> observable = this.statusUpdateSubject;
            this.CurrentNode = 0;

            // Return to the caller to decide what to do next
            return Task.FromResult(observable);
        }

        /// <summary>
        /// Generates a string representation of the current state.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var program = this.Program?.ToString();
            var variables = "{}";
            return $"Program:{program}{Environment.NewLine}Variables:{variables}";
        }

        /// <summary>
        /// Adds the default engine functions to the list of available functions.
        /// </summary>
        /// <param name="target">The target list to add the functions to.</param>
        private static void AddBaseLevelEngineFunctions(List<EngineFunction> target)
        {
            target.Add(new EngineFunction("reset", async engine => { await engine.ResetAsync(); return new EngineFunctionResult(); }));
        }

        /// <summary>
        /// Adds the default engine functions to the list of available functions.
        /// </summary>
        /// <param name="target">The target list to add the functions to.</param>
        private static void AddDefaultEngineFunctions(List<EngineFunction> target)
        {
            AddBaseLevelEngineFunctions(target);
            target.Add(new EngineFunction("start", engine => { throw new NotImplementedException(); }));
            target.Add(new EngineFunction("go", engine => { throw new NotImplementedException(); }));
        }

        /// <summary>
        /// Indexes all the functions.
        /// </summary>
        private void IndexFunctions()
        {
            this.functions.Clear();
            foreach (var func in this.currentFunctions)
            {
                this.functions.Add(func.Name, func);
            }
        }
    }
}