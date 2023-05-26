using Microsoft.AspNetCore.Authorization;

namespace NaoBlocks.Web.Authorization
{
    /// <summary>
    /// Defines an authorization policy that requires the synchronization role.
    /// </summary>
    public class RequireSynchronizationAttribute
        : AuthorizeAttribute
    {
        /// <summary>
        /// Initialize a new <see cref="RequireSynchronizationAttribute"/> instance.
        /// </summary>
        public RequireSynchronizationAttribute()
            : base("Synchronization")
        {
        }
    }
}
