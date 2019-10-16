using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Dtos
{
    public class ExecutionResult<TOutput>
    {
        public IEnumerable<string> ValidationErrors { get; set; }

        public IEnumerable<string> ExecutionErrors { get; set; }

        public TOutput Output { get; set; }
    }
}
