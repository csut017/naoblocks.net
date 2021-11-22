using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Parser;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for compiling code.
    /// </summary>
    public class CompileCode
        : CommandBase<CompiledCodeProgram>
    {
        /// <summary>
        /// The code to execute.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Validates the code to compile.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        public override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add(this.GenerateError("No code to compile"));
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        /// <summary>
        /// Compiles the code.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>A <see cref="CommandResult"/> containing the asbtract syntax tree.</returns>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            var parser = CodeParser.New(this.Code ?? string.Empty);
            var parseResult = await parser.ParseAsync().ConfigureAwait(false);
            return this.Result(new CompiledCodeProgram(parseResult));
        }
    }
}
