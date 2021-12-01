using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;

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

        /// <summary>
        /// Retrieves the current user details.
        /// </summary>
        /// <param name="controller">The controller to use for retrieving the user.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>A <see cref="User"/> instance, if found, otherwise null.</returns>
        public static async Task<User?> LoadUserAsync(this ControllerBase controller, IExecutionEngine engine)
        {
            var userId = controller.User?.Identity?.Name;
            if (userId == null) return null;

            var user = await engine.Query<UserData>()
                .RetrieveByNameAsync(userId)
                .ConfigureAwait(false);
            return user;
        }
    }
}
