using NaoBlocks.Engine.Data;
using System.Globalization;

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
        private Robot? robot;
        private RobotType? robotType;
        private IDatabaseSession? session;
        private User? user;

        /// <summary>
        /// Gets whether a robot type has been set.
        /// </summary>
        public bool HasRobotType
        {
            get { return this.robotType != null; }
        }

        /// <summary>
        /// Gets whether a user has been set.
        /// </summary>
        public bool HasUser
        {
            get { return this.user != null; }
        }

        /// <summary>
        /// Gets the initialised robot.
        /// </summary>
        public Robot Robot
        {
            get
            {
                if (this.robot == null) throw new InvalidOperationException("Robot has not been initialised");
                return this.robot;
            }
        }

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
        /// Generates a robot-based report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <param name="robot">The robot to generate the report for.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data and filename.</returns>
        public Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format, Robot robot)
        {
            this.robot = robot;
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

        /// <summary>
        /// Attempts to parse the from and to dates.
        /// </summary>
        /// <param name="fromDefault">The default from date.</param>
        /// <param name="toDefault">The default to date.</param>
        /// <returns>The from and to dates.</returns>
        /// <exception cref="ApplicationException">One of the date formats is incorrect.</exception>
        protected (DateTime, DateTime) ParseFromToDates(DateTime? fromDefault = null, DateTime? toDefault = null)
        {
            var toDate = toDefault.GetValueOrDefault(DateTime.Now);
            var fromDate = fromDefault.GetValueOrDefault(toDate.AddDays(-7));
            var fromDateText = this.GetArgumentOrDefault("from");
            var toDateText = this.GetArgumentOrDefault("to");
            if (!string.IsNullOrEmpty(fromDateText))
            {
                if (!DateTime.TryParseExact(fromDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
                {
                    throw new ApplicationException($"From date is invalid, it should be yyyy-MM-dd, found {fromDateText}");
                }
            }
            if (!string.IsNullOrEmpty(toDateText))
            {
                if (!DateTime.TryParseExact(toDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                {
                    throw new ApplicationException($"To date is invalid, it should be yyyy-MM-dd, found {toDateText}");
                }
            }
            return (fromDate, toDate);
        }
    }
}