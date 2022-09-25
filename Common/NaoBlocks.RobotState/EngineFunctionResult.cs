namespace NaoBlocks.RobotState
{
    /// <summary>
    /// A result from executing an <see cref="EngineFunction"/>.
    /// </summary>
    public class EngineFunctionResult
    {
        /// <summary>
        /// Initialises a new instance of <see cref="EngineFunctionResult"/>.
        /// </summary>
        /// <param name="wasSuccessful">Whether the function was successful or not.</param>
        public EngineFunctionResult(bool wasSuccessful)
        {
            this.WasSuccessful = wasSuccessful;
        }

        /// <summary>
        /// Initialises a new instance of <see cref="EngineFunctionResult"/> from an exception.
        /// </summary>
        /// <param name="error">The error details.</param>
        public EngineFunctionResult(Exception error)
        {
            this.WasSuccessful = false;
            this.ErrorMessage = error.Message;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets whether the function was successful or not.
        /// </summary>
        public bool WasSuccessful { get; private set; }
    }
}