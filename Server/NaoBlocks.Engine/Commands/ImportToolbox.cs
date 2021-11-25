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
        /// Gets or sets the name of the robot type the toolbox is for.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the definition of the toolbox.
        /// </summary>
        public string? Definition { get; set; }

        /// <summary>
        /// Attempts to retrieve the robot type and validate the toolbox.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);

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
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.robotType);
            ValidateExecutionState(this.document);
            var categories = this.document!
                .Root!
                .Descendants("category")
                .Select(ParseCategory);
            this.robotType!.Toolbox.Clear();
            foreach (var category in categories)
            {
                this.robotType.Toolbox.Add(category);
            }

            return Task.FromResult(CommandResult.New(this.Number, this.robotType));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.Name, errors).ConfigureAwait(false);
            this.document = XDocument.Parse(this.Definition!);
            return errors.AsEnumerable();
        }

        private ToolboxCategory ParseCategory(XElement categoryEl, int elOrder)
        {
            var name = categoryEl.Attribute("name")?.Value ?? "Unknown";
            var colour = categoryEl.Attribute("colour")?.Value ?? "0";
            var tags = categoryEl.Attribute("tags")?.Value ?? "";
            var orderText = categoryEl.Attribute("order")?.Value ?? "-1";
            if (!int.TryParse(orderText, out int order) || (order < 0))
            {
                order = (elOrder + 10) * 10;
            }
            var custom = categoryEl.Attribute("custom")?.Value;

            var category = new ToolboxCategory
            {
                Name = name,
                Colour = colour,
                Order = order,
                Custom = custom
            };
            foreach (var tag in tags.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                category.Tags.Add(tag);
            }

            foreach (var block in categoryEl.Descendants("block").Select(ParseBlock))
            {
                category.Blocks.Add(block);
            }

            return category;
        }

        private ToolboxBlock ParseBlock(XElement blockEl, int elOrder)
        {
            var name = blockEl.Attribute("type")?.Value ?? "Unknown";
            var orderText = blockEl.Attribute("order")?.Value ?? "-1";
            if (!int.TryParse(orderText, out int order) || (order < 0))
            {
                order = (elOrder + 10) * 10;
            }

            var block = new ToolboxBlock
            {
                Name = name,
                Order = order,
                Definition = blockEl.ToString()
            };
            return block;
        }
    }
}
