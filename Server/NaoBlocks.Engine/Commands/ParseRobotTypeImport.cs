using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using System.Diagnostics;
using System.Text.Json;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for parsing a robot type definition file.
    /// </summary>
    [Transient]
    public class ParseRobotTypeImport
        : CommandBase
    {
        private readonly List<string> errors = new();

        private RobotType? robotType = null;

        /// <summary>
        /// Gets or sets the  data.
        /// </summary>
        public Stream? Data { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether validation should be skipped or not.
        /// </summary>
        public bool SkipValidation { get; set; }

        /// <summary>
        /// Validates the robot type details.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (this.Data == null)
            {
                errors.Add(this.GenerateError("Data is required"));
            }

            if (!errors.Any())
            {
                JsonElement definition = new();
                try
                {
                    definition = await JsonSerializer.DeserializeAsync<JsonElement>(this.Data!);
                }
                catch (JsonException)
                {
                    errors.Add(this.GenerateError("Unable to parse Robot Type definition: Definition is invalid JSON"));
                }

                if (!errors.Any())
                {
                    try
                    {
                        ParseJson(definition!);
                        if (!this.SkipValidation) await this.ValidateRobotType(session, errors);
                    }
                    catch (Exception error)
                    {
                        errors.Add(this.GenerateError($"Unable to parse Robot Type definition: {error.Message}"));
                    }
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Returns the results.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            return Task.FromResult(
                CommandResult.New(
                    this.Number,
                    this.robotType!));
        }

        private void ParseBoolean(JsonElement element, string propertyName, Action<bool> setter)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                try
                {
                    setter(property.GetBoolean());
                }
                catch (Exception)
                {
                    this.errors.Add($"Property '{propertyName}' is not a valid boolean (true or false)");
                }
            }
        }

        /// <summary>
        /// Parse the JSON definition.
        /// </summary>
        /// <param name="definition">The definition to parse.</param>
        private void ParseJson(JsonElement definition)
        {
            if (definition.ValueKind != JsonValueKind.Object) throw new Exception("Root level definition should be a single definition");

            this.robotType = new RobotType();
            this.ParseString(definition, "name", v => this.robotType.Name = v, true);
            this.ParseBoolean(definition, "isDefault", v => this.robotType.IsDefault = v);
            this.ParseBoolean(definition, "directLogging", v => this.robotType.AllowDirectLogging = v);
            if (this.errors.Any())
            {
                this.robotType.Message = string.Join(",", this.errors);
            }
        }

        private void ParseString(JsonElement element, string propertyName, Action<string> setter, bool isRequired = false)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                setter(property.GetString() ?? string.Empty);
            }
            else
            {
                if (isRequired) this.errors.Add($"Property '{propertyName}' not set");
            }
        }

        private async Task ValidateRobotType(IDatabaseSession session, List<CommandError> errors)
        {
            if (this.robotType == null) throw new UnreachableException("Should not be able to get null here");
            if (!string.IsNullOrEmpty(this.robotType.Name))
            {
                var robotTypeExists = await session.Query<RobotType>()
                    .AnyAsync(r => r.Name == robotType.Name)
                    .ConfigureAwait(false);
                if (robotTypeExists) this.robotType.IsDuplicate = true;
            }
        }
    }
}