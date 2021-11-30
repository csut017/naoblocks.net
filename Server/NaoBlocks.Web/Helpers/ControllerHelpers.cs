using Microsoft.AspNetCore.Mvc;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Extension methods to assist with validating controller data.
    /// </summary>
    public static class ControllerHelpers
    {
        public static (int, int) ValidatePageArguments(this ControllerBase controller, int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;
            if (pageSize < 0) pageSize = 25;

            return (pageNum, pageSize);
        }
    }
}
