﻿using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using System.Xml.Linq;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the toolbox for a user.
    /// </summary>
    public class UserToolbox
        : ReportGenerator
    {
        /// <summary>
        /// Generates the students list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var user = this.User!;
            var robotType = new RobotType();
            if (string.IsNullOrEmpty(this.User.Settings.RobotTypeId))
            {
                // If the user does not have an assigned robot type, then retrieve the system defined robot type
                var defaultRobotType = await this.Session.Query<RobotType>()
                    .Where(type => type.IsDefault)
                    .FirstOrDefaultAsync();
                if (defaultRobotType == null) throw new ApplicationException("Cannot determine robot type");
                robotType = defaultRobotType;
            }
            else
            {
                // Otherwise, the user has an assigned robot type, so we want to use it instead
                var userRobotType = await this.Session
                    .LoadAsync<RobotType>(this.User.Settings.RobotTypeId);
                if (userRobotType == null) throw new ApplicationException("Unknown robot type");
                robotType = userRobotType;
            }

            // Retrieve the toolbox to use
            var options = new List<string>();
            Toolbox? toolbox = null;
            if (user.Settings.Toolbox != null)
            {
                toolbox = robotType.Toolboxes
                    .FirstOrDefault(t => t.Name == user.Settings.Toolbox);
            }

            if (toolbox == null)
            {
                toolbox = robotType.Toolboxes
                    .FirstOrDefault(t => t.IsDefault);
            }

            var toolboxCategories = toolbox?.Categories ?? new List<ToolboxCategory>();

            // Determine the categories to include
            var categories = new Dictionary<string, ToolboxCategory>();
            foreach (var category in toolboxCategories)
            {
                var hasCategory = !category.IsOptional
                    || user.Settings.ToolboxCategories.Contains(category.Name);
                if (!hasCategory) continue;

                if (!categories.TryGetValue(category.Name, out ToolboxCategory? group))
                {
                    group = new ToolboxCategory
                    {
                        Name = category.Name,
                        Colour = category.Colour,
                        Custom = category.Custom,
                    };
                    categories.Add(category.Name, group);
                }

                foreach (var block in category.Blocks)
                {
                    group.Blocks.Add(block);
                }
            }

            // Generate the actual toolbox
            var rootEl = new XElement("xml");
            foreach (var category in categories.Values)
            {
                // Generate the category node
                var el = new XElement("category",
                    new XAttribute("name", category.Name),
                    new XAttribute("colour", category.Colour));
                if (!string.IsNullOrEmpty(category.Custom)) el.Add(new XAttribute("custom", category.Custom));

                // Generate the blocks
                foreach (var block in category.Blocks)
                {
                    var blockEl = XElement.Parse(block.Definition);
                    el.Add(blockEl);
                }

                // Finally, add it to the root
                rootEl.Add(el);
            }

            var document = new XDocument(rootEl);
            var stream = new MemoryStream();
            if (!(document?.Root?.IsEmpty ?? true)) document.Save(stream, SaveOptions.DisableFormatting);
            stream.Seek(0, SeekOrigin.Begin);
            return Tuple.Create((Stream)stream, $"Toolbox-{this.User.Name}.xml");
        }

        /// <summary>
        /// Checks if the report format is available.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>True if the format is available, false otherwise.</returns>
        public override bool IsFormatAvailable(ReportFormat format)
        {
            return format switch
            {
                ReportFormat.Text => true,
                _ => false,
            };
        }
    }
}