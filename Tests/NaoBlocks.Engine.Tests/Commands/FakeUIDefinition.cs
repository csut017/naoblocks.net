using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class FakeUIDefinition
        : IUIDefinition
    {
        public Task<IEnumerable<UIDefinitionItem>> DescribeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GenerateAsync(string component)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CommandError>> ValidateAsync(IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            return Task.FromResult(errors.AsEnumerable());
        }
    }
}