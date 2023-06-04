using Microsoft.AspNetCore.Authorization;

namespace NaoBlocks.Web.Authorization
{
    /// <summary>
    /// Defines an authorization policy that requires the administrator role.
    /// </summary>
    public class RequireAdministrator
        : AuthorizeAttribute
    {
        /// <summary>
        /// Initialize a new <see cref="RequireAdministrator"/> instance.
        /// </summary>
        public RequireAdministrator()
            : base("Administrator")
        {
        }
    }
}