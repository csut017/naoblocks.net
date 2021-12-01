using NaoBlocks.Parser;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A compiled program.
    /// </summary>
    public class CompiledCodeProgram
    {
        /// <summary>
        /// Gets the errors from parsing the code.
        /// </summary>
        public IEnumerable<ParseError>? Errors { get; private set; }

        /// <summary>
        /// Gets the Abstract Syntax Tree from compilation.
        /// </summary>
        public IEnumerable<AstNode>? Nodes { get; private set; }

        /// <summary>
        /// Gets or sets the program id.
        /// </summary>
        public long? ProgramId { get; set; }

        /// <summary>
        /// Converts from a data entity to a data transfer object.
        /// </summary>
        /// <param name="value">The <see cref="Data.CompiledCodeProgram"/> to convert.</param>
        /// <param name="includeDetails">Whether to include the details or not.</param>
        /// <returns>A <see cref="CompiledCodeProgram"/> instance.</returns>
        public static CompiledCodeProgram? FromModel(Data.CompiledCodeProgram? value)
        {
            if (value == null) return null;
            var program = new CompiledCodeProgram
            {
                ProgramId = value.ProgramId
            };

            if (value.Errors != null) program.Errors = value.Errors;
            if (value.Nodes != null)
            {
                program.Nodes = value.Nodes.Select(n => AstNode.FromModel(n) ?? AstNode.Empty).ToList();
            }

            return program;
        }
    }
}
