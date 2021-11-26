using NaoBlocks.Common;
using System.Collections.Generic;
using System.Linq;

namespace NaoBlocks.Web.Tests.Controllers
{
    public static class TestHelpers
    {
        internal static IEnumerable<string> ExtractValidationErrors(ExecutionResult result)
        {
            return result.ValidationErrors
                .Select(err => err.Error);
        }
    }
}
