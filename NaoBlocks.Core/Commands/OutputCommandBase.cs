using Newtonsoft.Json;

namespace NaoBlocks.Core.Commands
{
    public abstract class OutputCommandBase<TOuput>
        : CommandBase
        where TOuput : class
    {
        [JsonIgnore]
        public TOuput? Output { get; protected set; }
    }
}