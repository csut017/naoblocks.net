using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Newtonsoft.Json;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to add a UI definition.
    /// </summary>
    public class AddUIDefinition
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the <see cref="IUIDefinition"/> to add.
        /// </summary>
        public IUIDefinition? Definition { get; set; }

        /// <summary>
        /// Gets or sets whether to ignore an existing definition.
        /// </summary>
        [JsonIgnore]
        public bool IgnoreExisting { get; set; }

        /// <summary>
        /// Gets or sets the name of the definition.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Attempts to validate the definition.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(GenerateError("Definition name is required"));
            }

            if (this.Definition == null)
            {
                errors.Add(GenerateError("Definition is required"));
            }
            else if (!this.IgnoreExisting)
            {
                var existing = await session.Query<UIDefinition>()
                    .FirstOrDefaultAsync(d => d.Name == this.Name)
                    .ConfigureAwait(false);
                if (existing != null)
                {
                    errors.Add(GenerateError("Definition already exists"));
                }
                else
                {
                    errors.AddRange(
                        await this.Definition.ValidateAsync(engine).ConfigureAwait(false));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the robot type in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var definition = new UIDefinition
            {
                Name = this.Name!,
                Definition = this.Definition
            };
            await session.StoreAsync(definition);
            return CommandResult.New(this.Number, definition);
        }
    }
}