using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public abstract class OutputCommandBase<TOuput>
        : CommandBase
    {
        [JsonIgnore]
        public TOuput Output { get; protected set; }
    }
}
