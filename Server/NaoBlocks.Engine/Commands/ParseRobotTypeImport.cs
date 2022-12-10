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
        private readonly RobotTypeImport definition = new();
        private readonly List<string> errors = new();

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
            ValidateExecutionState(this.definition.RobotType);
            return Task.FromResult(
                CommandResult.New(
                    this.Number,
                    this.definition));
        }

        private void AddError(string message, string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                this.errors.Add(message);
            }
            else
            {
                this.errors.Add($"{message} for {name}");
            }
        }

        private void ParseArray(JsonElement definition, string propertyName, Action<JsonElement, int> parser)
        {
            if (definition.TryGetProperty(propertyName, out var toolboxes))
            {
                if (toolboxes.ValueKind == JsonValueKind.Array)
                {
                    var index = 0;
                    foreach (var element in toolboxes.EnumerateArray())
                    {
                        parser(element, index++);
                    }
                }
                else
                {
                    this.errors.Add($"Property '{propertyName}' is not a valid array");
                }
            }
        }

        private void ParseBoolean(JsonElement element, string propertyName, Action<bool> setter, string? name = null)
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
                    AddError($"Property '{propertyName}' is not a valid boolean value", name);
                }
            }
        }

        private void ParseEnum<TEnum>(JsonElement element, string propertyName, Action<TEnum> setter, string? name = null)
                    where TEnum : struct
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                var text = property.GetString();
                if (Enum.TryParse<TEnum>(text, out var value))
                {
                    setter(value);
                }
                else
                {
                    AddError($"Property '{propertyName}' is not a valid {typeof(TEnum).Name} value", name);
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

            this.definition.RobotType = new RobotType();
            this.ParseString(definition, "name", v => this.definition.RobotType.Name = v, isRequired: true);
            this.ParseBoolean(definition, "isDefault", v => this.definition.RobotType.IsDefault = v);
            this.ParseBoolean(definition, "directLogging", v => this.definition.RobotType.AllowDirectLogging = v);
            this.ParseArray(definition, "toolboxes", (element, index) =>
            {
                var toolbox = new Toolbox();
                var name = $"toolbox #{index + 1}";
                this.ParseString(element, "name", v => toolbox.Name = v, name, true);
                this.ParseBoolean(element, "isDefault", v => toolbox.IsDefault = v, name);
                this.ParseString(element, "definition", v => toolbox.RawXml = v, name, true);
                this.definition.RobotType.Toolboxes.Add(toolbox);
            });
            this.ParseArray(definition, "values", (element, index) =>
            {
                var value = new NamedValue();
                var name = $"value #{index + 1}";
                this.ParseString(element, "name", v => value.Name = v, name, true);
                this.ParseString(element, "value", v => value.Value = v);
                this.definition.RobotType.CustomValues.Add(value);
            });
            this.ParseArray(definition, "templates", (element, index) =>
            {
                var template = new LoggingTemplate();
                this.definition.RobotType.LoggingTemplates.Add(template);
                var name = $"template #{index + 1}";
                this.ParseString(element, "category", v => template.Category = v, name, true);
                this.ParseString(element, "text", v => template.Text = v, name, true);
                this.ParseEnum<ClientMessageType>(element, "type", v => template.MessageType = v, name);
                this.ParseString(element, "values", v => template.ValueNames = v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            });

            if (this.errors.Any())
            {
                this.definition.RobotType.Message = string.Join(",", this.errors);
            }
        }

        private bool ParseString(JsonElement element, string propertyName, Action<string> setter, string? name = null, bool isRequired = false)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                setter(property.GetString() ?? string.Empty);
                return true;
            }

            if (isRequired)
            {
                AddError($"Property '{propertyName}' not set", name);
            }
            return false;
        }

        private async Task ValidateRobotType(IDatabaseSession session, List<CommandError> errors)
        {
            if (this.definition.RobotType == null) throw new UnreachableException("Should not be able to get null here");
            if (!string.IsNullOrEmpty(this.definition.RobotType.Name))
            {
                var robotTypeExists = await session.Query<RobotType>()
                    .AnyAsync(r => r.Name == this.definition.RobotType.Name)
                    .ConfigureAwait(false);
                if (robotTypeExists) this.definition.IsDuplicate = true;
            }
        }
    }
}