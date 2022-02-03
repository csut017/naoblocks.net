using NaoBlocks.Engine;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests
{
    public class FakeCommand
        : CommandBase
    {
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            return Task.FromResult(
                new CommandResult(1));
        }
    }
}
