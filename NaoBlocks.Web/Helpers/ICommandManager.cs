using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public interface ICommandManager
    {
        Task<CommandResult> ApplyAsync(CommandBase command);

        Task<IEnumerable<string>> ValidateAsync(CommandBase command);

        Task CommitAsync();
    }
}
