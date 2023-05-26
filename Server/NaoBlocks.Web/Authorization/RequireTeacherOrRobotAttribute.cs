using Microsoft.AspNetCore.Authorization;

namespace NaoBlocks.Web.Authorization
{
    /// <summary>
    /// Defines an authorization policy that requires the teacher or robot roles.
    /// </summary>
    public class RequireTeacherOrRobotAttribute
        : AuthorizeAttribute
    {
        /// <summary>
        /// Initialize a new <see cref="RequireTeacherOrRobotAttribute"/> instance.
        /// </summary>
        public RequireTeacherOrRobotAttribute()
            : base("TeacherOrRobot")
        {            
        }
    }
}
