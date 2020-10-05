using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public interface ICommandManager
    {
        ILogger Logger { get; }

        Task<CommandResult> ApplyAsync(CommandBase? command);

        Task CommitAsync();

        Task<IEnumerable<CommandError>> ValidateAsync(CommandBase? command);
    }
}