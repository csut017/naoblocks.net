using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to update the custom values for a robot type.
    /// </summary>
    public class UpdateCustomValuesForRobotType
        : UserCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the custom values.
        /// </summary>
        public IEnumerable<NamedValue?> Values { get; set; } = Array.Empty<NamedValue?>();

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await session.Query<RobotType>()
                .FirstOrDefaultAsync(RobotType => RobotType.Name == this.Name)
                .ConfigureAwait(false);
            if (this.robotType == null)
            {
                errors.Add(this.GenerateError($"Unknown robot type '{this.Name}'"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates the robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Robot type name is required"));
            }
            else
            {
                this.robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(RobotType => RobotType.Name == this.Name)
                    .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Unknown robot type '{this.Name}'"));
                }
            }

            if (!errors.Any())
            {
                // The existing names should be unique
                var names = new HashSet<string>();
                foreach (var value in this.Values.Where(v => v != null))
                {
                    if (names.Contains(value!.Name))
                    {
                        errors.Add(this.GenerateError($"Value '{value.Name}' is a duplicated name"));
                    }
                    else
                    {
                        names.Add(value.Name);
                    }
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the custom values in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            var robotType = this.robotType!;
            robotType.CustomValues.Clear();
            foreach (var value in this.Values.Where(v => v != null))
            {
                robotType.CustomValues.Add(value!);
            }

            return Task.FromResult(CommandResult.New(this.Number, robotType));
        }
    }
}