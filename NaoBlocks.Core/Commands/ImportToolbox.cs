using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NaoBlocks.Core.Commands
{
    public class ImportToolbox
        : CommandBase<RobotType>
    {
        private XDocument? document;

        private RobotType? robotType;

        public string? Name { get; set; }

        public string? Definition { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            this.robotType = await session.Query<RobotType>()
                                        .FirstOrDefaultAsync(u => u.Name == this.Name)
                                        .ConfigureAwait(false);
            if (this.robotType == null) errors.Add(this.GenerateError($"Robot type {this.Name} does not exist"));

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Definition is required"));
            }
            else {
                try
                {
                    this.document = XDocument.Parse(this.Definition);
                }
                catch (Exception ex)
                {
                    this.document = null;
                    errors.Add(this.GenerateError($"Unable to load definition: {ex.Message}"));
                }
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if ((this.robotType == null) || (this.document == null)) throw new InvalidOperationException("ValidateAsync must be called first");
            if (!string.IsNullOrEmpty(this.Name) && (this.Name != this.robotType.Name)) this.robotType.Name = this.Name;

            var categories = this.document
                .Root
                .Descendants("category")
                .Select(ParseCategory);
            this.robotType.Toolbox.Clear();
            foreach (var category in categories)
            {
                this.robotType.Toolbox.Add(category);
            }

            return Task.FromResult(CommandResult.New(this.Number, this.robotType));
        }

        private ToolboxCategory ParseCategory(XElement categoryEl, int elOrder)
        {
            var name = categoryEl.Attribute("name")?.Value ?? "Unknown";
            var colour = categoryEl.Attribute("colour")?.Value ?? "0";
            var tags = categoryEl.Attribute("tags")?.Value ?? "";
            var orderText = categoryEl.Attribute("order")?.Value ?? "-1";
            if (!int.TryParse(orderText, out int order) || (order < 0))
            {
                order = (elOrder + 10)* 10;
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