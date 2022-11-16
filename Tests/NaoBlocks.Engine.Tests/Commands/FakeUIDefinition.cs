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
        private readonly IList<string> validationErrors = new List<string>();

        public void AddValidationError(string message)
        {
            this.validationErrors.Add(message);
        }

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
            var errors = this.validationErrors.Select(e => new CommandError(0, e));
            return Task.FromResult(errors.AsEnumerable());
        }
    }
}