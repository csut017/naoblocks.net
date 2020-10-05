using NaoBlocks.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NaoBlocks.Core.Generators
{
    public static class UserToolbox
    {
        public static string Generate(User user, RobotType robotType)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (robotType == null) throw new ArgumentNullException(nameof(robotType));
            var rootEl = new XElement("xml");

            // Generate the list of options
            var options = new List<string>();
            if (string.IsNullOrEmpty(user.CustomToolbox))
            {
                if (user.Settings.Conditionals) options.Add("conditionals");
                if (user.Settings.Dances) options.Add("dances");
                if (user.Settings.Events) options.Add("events");
                if (user.Settings.Loops) options.Add("loops");
                if (user.Settings.Sensors) options.Add("sensors");
                if (user.Settings.Tutorials) options.Add("tutorials");
                if (user.Settings.Variables) options.Add("variables");
                if (user.Settings.Simple) options.Add("simple");
                if (!user.Settings.Simple) options.Add("default");
            } else
            {
                options.AddRange(user.CustomToolbox.Split(',').Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)));
            }

            // Determine the categories to include
            var categories = new Dictionary<string, ToolboxCategory>();
            foreach (var category in robotType.Toolbox)
            {
                var hasCategory = category
                    .Tags
                    .Any(tag => options.Contains(tag));
                if (!hasCategory) continue;

                if (!categories.TryGetValue(category.Name, out ToolboxCategory group))
                {
                    group = new ToolboxCategory
                    {
                        Name = category.Name,
                        Colour = category.Colour,
                        Custom = category.Custom,
                        Order = category.Order
                    };
                    categories.Add(category.Name, group);
                }

                foreach (var block in category.Blocks)
                {
                    group.Blocks.Add(block);
                }
            }

            // Generate the actual toolbox
            foreach (var category in categories.Values.OrderBy(c => c.Order).ThenBy(c => c.Name))
            {
                // Generate the category node
                var el = new XElement("category",
                    new XAttribute("name", category.Name),
                    new XAttribute("colour", category.Colour));
                if (!string.IsNullOrEmpty(category.Custom)) el.Add(new XAttribute("custom", category.Custom));

                // Generate the blocks
                foreach (var block in category.Blocks.OrderBy(b => b.Order).ThenBy(b => b.Name))
                {
                    var blockEl = XElement.Parse(block.Definition);
                    el.Add(blockEl);
                }

                // Finally, add it to the root
                rootEl.Add(el);
            }

            var document = new XDocument(rootEl);
            return document.ToString();
        }
    }
}
