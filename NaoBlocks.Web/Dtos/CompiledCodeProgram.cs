using NaoBlocks.Parser;
using System.Collections.Generic;
using System.Linq;

using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class CompiledCodeProgram
    {
        public IEnumerable<ParseError>? Errors { get; private set; }

        public IEnumerable<AstNode>? Nodes { get; private set; }

        public long? ProgramId { get; set; }

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
                program.Nodes = value.Nodes.Select(n => AstNode.FromModel(n) ?? AstNode.Empty).Where(n => !string.IsNullOrEmpty(n.Type)).ToList();
                if (!program.Nodes.Any()) program.Nodes = null;
            }

            return program;
        }
    }
}