using NaoBlocks.Core.Commands;
using System.Collections.Generic;
using System.Linq;

namespace NaoBlocks.Web.Dtos
{
    public class ExecutionResult
    {
        public IEnumerable<CommandError> ExecutionErrors { get; set; }

        public bool Successful
        {
            get
            {
                if ((this.ValidationErrors != null) && this.ValidationErrors.Any()) return false;
                if ((this.ExecutionErrors != null) && this.ExecutionErrors.Any()) return false;
                return true;
            }
        }

        public IEnumerable<CommandError> ValidationErrors { get; set; }

        public static ExecutionResult<TOutput> New<TOutput>(TOutput value)
        {
            return new ExecutionResult<TOutput>
            {
                Output = value
            };
        }
    }

    public class ExecutionResult<TOutput>
        : ExecutionResult
    {
        public TOutput Output { get; set; }
    }
}