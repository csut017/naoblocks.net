using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using System.Xml.Linq;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to import a toolbox for a robot type.
    /// </summary>
    public class ImportToolbox
        : RobotTypeCommandBase
    {
        private XDocument? document;

        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the definition of the toolbox.
        /// </summary>
        public string? Definition { get; set; }

        /// <summary>
        /// Gets or sets whether to ignore any missing robot and only check if during execution.
        /// </summary>
        public bool IgnoreMissingRobotType { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default toolbox.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type the toolbox is for.
        /// </summary>
        public string? RobotTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the toolbox.
        /// </summary>
        public string? ToolboxName { get; set; }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (!this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotTypeName, errors).ConfigureAwait(false);
            }

            this.document = XDocument.Parse(this.Definition!);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Attempts to retrieve the robot type and validate the toolbox.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (!this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotTypeName, errors).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(this.ToolboxName))
            {
                errors.Add(this.GenerateError("Toolbox name is required"));
            }

            if (string.IsNullOrWhiteSpace(this.Definition))
            {
                errors.Add(this.GenerateError($"Definition is required"));
            }
            else
            {
                try
                {
                    this.document = XDocument.Parse(this.Definition!);
                }
                catch (Exception)
                {
                    this.document = null;
                    errors.Add(this.GenerateError($"Unable to load definition"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Updates the toolbox in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            if (this.IgnoreMissingRobotType)
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotTypeName, new List<CommandError>()).ConfigureAwait(false);
                if (this.robotType == null) this.robotType = session.GetFromCache<RobotType>(this.RobotTypeName ?? String.Empty);
            }

            ValidateExecutionState(this.robotType);
            ValidateExecutionState(this.document);
            var categories = this.document!
                .Root!
                .Descendants("category")
                .Select(ParseCategory);
            var defaultToobox = this.robotType!
                .Toolboxes
                .FirstOrDefault(t => t.IsDefault);
            var toolbox = this.robotType
                .Toolboxes
                .FirstOrDefault(t => t.Name == this.ToolboxName);
            var setAsDefault = this.IsDefault;
            if (defaultToobox != null)
            {
                if (defaultToobox.Name != this.ToolboxName)
                {
                    defaultToobox.IsDefault = false;
                }
            }
            else
            {
                setAsDefault = true;
            }

            if (toolbox == null)
            {
                toolbox = new Toolbox
                {
                    Name = this.ToolboxName!,
                    IsDefault = setAsDefault,
                };
                this.robotType.Toolboxes.Add(toolbox);
            }
            else
            {
                toolbox.Categories.Clear();
            }

            foreach (var category in categories)
            {
                toolbox.Categories.Add(category);
            }

            return CommandResult.New(this.Number, this.robotType!);
        }

        private ToolboxBlock ParseBlock(XElement blockEl, int elOrder)
        {
            var name = blockEl.Attribute("type")?.Value ?? "Unknown";

            var block = new ToolboxBlock
            {
                Name = name,
                Definition = blockEl.ToString()
            };
            return block;
        }

        private ToolboxCategory ParseCategory(XElement categoryEl, int elOrder)
        {
            var name = categoryEl.Attribute("name")?.Value ?? "Unknown";
            var colour = categoryEl.Attribute("colour")?.Value ?? "0";
            var optional = categoryEl.Attribute("optional")?.Value ?? "no";
            var isOptional = string.Equals(optional, "yes", StringComparison.InvariantCultureIgnoreCase);
            var custom = categoryEl.Attribute("custom")?.Value;

            var category = new ToolboxCategory
            {
                Name = name,
                Colour = colour,
                IsOptional = isOptional,
                Custom = custom
            };

            foreach (var block in categoryEl.Descendants("block").Select(ParseBlock))
            {
                category.Blocks.Add(block);
            }

            return category;
        }
    }
}