using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public class CommandResult
    {
        public CommandResult(int number, string error = null)
        {
            this.Number = number;
            this.Error = error;
        }

        public bool WasSuccessful
        {
            get { return string.IsNullOrEmpty(this.Error); }
        }

        public int Number { get; private set; }

        public string Error { get; set; }
    }
}
