using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for storing user settings.
    /// </summary>
    public class StoreSettings
        : UserCommandBase
    {
        private User? user;

        /// <summary>
        /// Gets or sets the user's settings.
        /// </summary>
        public UserSettings? Settings { get; set; }


        /// <summary>
        /// Gets or sets the name of the user to associate the program with.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Validates the program settings.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (this.Settings == null)
            {
                errors.Add(this.GenerateError($"Settings are required"));
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Settings.RobotType))
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
                        this.Settings.RobotTypeId = robotType.Id;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(this.UserName))
            {
                errors.Add(this.GenerateError($"User name is required"));
            }

            if (!errors.Any())
            {
                this.user = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors).ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.user);
            var settingsToStore = this.Settings ?? new UserSettings();
            this.user!.Settings = settingsToStore;
            return Task.FromResult(CommandResult.New(this.Number, settingsToStore));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.user = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors).ConfigureAwait(false);
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
                    this.Settings.RobotTypeId = robotType.Id;
                }
            }
            return errors.AsEnumerable();
        }
    }
}
