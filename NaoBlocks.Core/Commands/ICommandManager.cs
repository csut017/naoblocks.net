using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public interface ICommandManager
    {
        Task<CommandResult> ApplyAsync(CommandBase? command);

        Task CommitAsync();

        Task<IEnumerable<CommandError>> ValidateAsync(CommandBase? command);
    }
}