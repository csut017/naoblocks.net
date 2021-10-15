using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NaoBlocks.Client
{
    internal class CompiledCodeProgram
    {
        [JsonProperty("nodes")]
        public IEnumerable<AstNode> Nodes { get; private set; }
    }
}