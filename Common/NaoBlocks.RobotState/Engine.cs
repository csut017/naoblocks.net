using NaoBlocks.Parser;

namespace NaoBlocks.RobotState
{
    /// <summary>
    /// Contains a representation of a robot's state during program execution.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// The program for the engine.
        /// </summary>
        public AstProgram? Program { get; private set; }

        /// <summary>
        /// Initialises the engine.
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

            this.Program = AstProgram.New(ast.Nodes);
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
    }
}