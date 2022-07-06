using System.Xml.Linq;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox.
    /// </summary>
    public class Toolbox
    {
        /// <summary>
        /// Gets the categories.
        /// </summary>
        public IList<ToolboxCategory> Categories { get; private set; } = new List<ToolboxCategory>();

        /// <summary>
        /// Gets or sets whether this is the default toolbox.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the name of the toolbox.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Exports the definition as an XML string.
        /// </summary>
        /// <returns>A string containing the XML definition.</returns>
        public string? ExportToXml()
        {
            var root = new XElement("toolbox");
            foreach (var category in Categories)
            {
                var categoryEl = new XElement(
                    "category",
                    new XAttribute("name", category.Name),
                    new XAttribute("colour", category.Colour),
                    new XAttribute("optional", category.IsOptional ? "yes" : "no"));
                if (!string.IsNullOrEmpty(category.Custom))
                {
                    categoryEl.Add(new XAttribute("custom", category.Custom));
                }
                root.Add(categoryEl);
                foreach (var block in category.Blocks)
                {
                    var blockEl = XElement.Parse(block.Definition);
                    categoryEl.Add(blockEl);
                }
            }
            var doc = new XDocument(root);
            return doc.ToString(SaveOptions.DisableFormatting);
        }
    }
}