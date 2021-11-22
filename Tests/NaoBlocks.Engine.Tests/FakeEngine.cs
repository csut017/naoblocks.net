using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Common;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    public class FakeEngine : IExecutionEngine
    {
        public ILogger Logger => throw new System.NotImplementedException();

        public Mock<IDatabaseSession> DatabaseSession { get; } = new Mock<IDatabaseSession>();

        public Task CommitAsync()
        {
            throw new System.NotImplementedException();
        }

        internal static string[] GetErrors(IEnumerable<CommandError> errors)
        {
            return errors.Select(e => e.Error).ToArray();
        }

        public async Task<CommandResult> ExecuteAsync(CommandBase command)
        {
            var result = await command.ExecuteAsync(this.DatabaseSession.Object);
            return result;
        }

        public async Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            return await command.ValidateAsync(this.DatabaseSession.Object);
        }
    }
}
