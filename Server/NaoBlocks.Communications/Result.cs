namespace NaoBlocks.Communications
{
    /// <summary>
    /// A listener result.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Generates a new failure result.
        /// </summary>
        /// <param name="message">The error.</param>
        /// <returns>The new <see cref="Result{TType}"/> instance.</returns>
        public static Result<TType> Fail<TType>(Exception message)
        {
            return new Result<TType>(false, message, default);
        }

        /// <summary>
        /// Generates a new success result.
        /// </summary>
        /// <param name="value">The value to pass.</param>
        /// <returns>The new <see cref="Result{TType}"/> instance.</returns>
        public static Result<TType> Ok<TType>(TType value)
        {
            return new Result<TType>(true, null, value);
        }
    }

    /// <summary>
    /// A listener result.
    /// </summary>
    public class Result<T>
    {
        internal Result(bool success, Exception? error, T? value)
        {
            this.Success = success;
            this.Error = error;
            this.Value = value;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public Exception? Error { get; }

        /// <summary>
        /// Gets whether the operation was successful or not.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T? Value { get; }
    }
}