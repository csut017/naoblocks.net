using NaoBlocks.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class FakeUIDefinition
        : IUIDefinition
    {
        public Task<Stream> GenerateAsync(string component)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            throw new System.NotImplementedException();
        }
    }
}