namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines the available roles a user can have.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// The user is a student.
        /// </summary>
        Student,

        /// <summary>
        /// The user is a teacher.
        /// </summary>
        Teacher,

        /// <summary>
        /// The user is an administrator.
        /// </summary>
        Administrator,

        /// <summary>
        /// The user is a robot (non-human user.)
        /// </summary>
        Robot,

        /// <summary>
        /// The user's role is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// A generic user (specific role unknown.)
        /// </summary>
        User,

        /// <summary>
        /// The user can only participate in synchronization activities.
        /// </summary>
        Synchronization,
    }
}