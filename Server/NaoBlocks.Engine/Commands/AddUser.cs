using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Command for adding a new user.
    /// </summary>
    public class AddUser
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the user's age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the user's gender.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Gets or sets the user's hashed password.
        /// </summary>
        public Password HashedPassword { get; set; } = Data.Password.Empty;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's plain text password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Unknown;

        /// <summary>
        /// Gets or sets the user's settings.
        /// </summary>
        public UserSettings? Settings { get; set; }

        /// <summary>
        /// Validates the values for the new user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            var roleName = this.Role.ToString();

            if (this.Role == UserRole.Unknown)
            {
                roleName = "User";
                errors.Add(this.GenerateError($"Role is unknown or missing"));
            }

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"{roleName} name is required"));
            }

            if (this.Password == null)
            {
                errors.Add(this.GenerateError("Password is required"));
            }
            else
            {
                this.HashedPassword = Data.Password.New(this.Password);
                this.Password = null;
            }

            if (!string.IsNullOrEmpty(this.Settings?.RobotType))
            {
                var robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.Settings.RobotType)
                    .ConfigureAwait(false);
                if (robotType == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot type {this.Settings.RobotType}"));
                }
                else
                {
                    this.Settings.RobotType = robotType.Name;
                }
            }

            if (!string.IsNullOrEmpty(this.Settings?.RobotId))
            {
                var robot = await session.Query<Robot>()
                    .FirstOrDefaultAsync(rt => (rt.MachineName == this.Settings.RobotId) || (rt.FriendlyName == this.Settings.RobotId))
                    .ConfigureAwait(false);
                if (robot == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot {this.Settings.RobotId}"));
                }
                else
                {
                    this.Settings.RobotId = robot.MachineName;
                }
            }

            if (!errors.Any() && await session.Query<User>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add(this.GenerateError($"{roleName} with name {this.Name} already exists"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the user to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var user = new User
            {
                Name = this.Name!.Trim(),
                Role = this.Role,
                Password = this.HashedPassword,
                Settings = this.Settings ?? new UserSettings(),
                WhenAdded = this.WhenExecuted
            };
            if (this.Role == UserRole.Student)
            {
                user.StudentDetails = new StudentDetails
                {
                    Age = this.Age,
                    Gender = this.Gender
                };
            }
            await session.StoreAsync(user).ConfigureAwait(false);
            return CommandResult.New(this.Number, user);
        }
    }
}