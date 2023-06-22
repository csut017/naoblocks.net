namespace NaoBlocks.Engine
{
    /// <summary>
    /// Defines the sub-systems that a command targets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandTargetAttribute
            : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="CommandTargetAttribute"/> instance.
        /// </summary>
        /// <param name="target">The target system.</param>
        public CommandTargetAttribute(CommandTarget target)
        {
            Target = target;
        }

        /// <summary>
        /// Gets the targetted subsystem.
        /// </summary>
        public CommandTarget Target { get; }
    }
}