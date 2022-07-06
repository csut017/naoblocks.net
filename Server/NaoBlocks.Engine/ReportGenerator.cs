using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// A base class for all report generators to inherit from.
    /// </summary>
    /// <remarks>
    /// This class will ensure the query is correctly wired to the database session.
    /// </remarks>
    public abstract class ReportGenerator
    {
        private Dictionary<string, string> arguments = new();
        private RobotType? robotType;
        private IDatabaseSession? session;
        private User? user;

        /// <summary>
        /// Gets the initialised robot type.
        /// </summary>
        public RobotType RobotType
        {
            get
            {
                if (this.robotType == null) throw new InvalidOperationException("RobotType has not been initialised");
                return this.robotType;
            }
        }

        /// <summary>
        /// Gets the session to use.
        /// </summary>
        public IDatabaseSession Session
        {
            get
            {
                if (session == null) throw new InvalidOperationException("Session has not been initialised");
                return session;
            }
        }

        /// <summary>
        /// Gets the initialised user.
        /// </summary>
        public User User
        {
            get
            {
                if (this.user == null) throw new InvalidOperationException("User has not been initialised");
                return this.user;
            }
        }

        /// <summary>
        /// Generates a report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data and filename.</returns>
        public abstract Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format);

        /// <summary>
        /// Generates a user-based report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="user">The user the report is for.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data and filename.</returns>
        public Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format, User user)
        {
            this.user = user;
            return this.GenerateAsync(format);
        }

        /// <summary>
        /// Generates a robot type-based report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="robotType">The robot type to generate the report for.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data and filename.</returns>
        public Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format, RobotType robotType)
        {
            this.robotType = robotType;
            return this.GenerateAsync(format);
        }

        /// <summary>
        /// Generates a user-based report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="user">The user the report is for.</param>
        /// <param name="robotType">The robot type to generate the report for.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data and filename.</returns>
        public Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format, User user, RobotType robotType)
        {
            this.user = user;
            this.robotType = robotType;
            return this.GenerateAsync(format);
        }

        /// <summary>
        /// Initialises the database session for this query.
        /// </summary>
        /// <param name="session">The session to use.</param>
        public void InitialiseSession(IDatabaseSession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Checks if the report format is available.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>True if the format is available, false otherwise.</returns>
        public abstract bool IsFormatAvailable(ReportFormat format);

        /// <summary>
        /// Adds arguments to the generator.
        /// </summary>
        /// <param name="args">The arguments to add.</param>
        public void UseArguments(Dictionary<string, string> args)
        {
            foreach (var pair in args)
            {
                this.arguments[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Gets an argument value or the default value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns>The argument or default value.</returns>
        protected string? GetArgumentOrDefault(string name, string? defaultValue = null)
        {
            if (this.arguments.TryGetValue(name, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}