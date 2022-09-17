namespace NaoBlocks.RobotState
{
    /// <summary>
    /// An exception that has occurred in the engine.
    /// </summary>
    public class EngineException :
        Exception
    {
        /// <summary>
        /// Initialises a new <see cref="EngineException"/> instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public EngineException(string message)
            : base(message)
        {
        }
    }
}