using System.Xml.Linq;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a toolbox.
    /// </summary>
    public class Toolbox
    {
        /// <summary>
        /// The possible export formats for the definition.
        /// </summary>
        public enum Format
        {
            /// <summary>
            /// Toobox format (same as the import format.)
            /// </summary>
            Toolbox,

            /// <summary>
            /// Blockly format for direct import into a Blockly editor.
            /// </summary>
            Blockly,

            /// <summary>
            /// The format is unknown (mainly used for testing.)
            /// </summary>
            Unknown
        }

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
        /// Gets or sets the raw XML definition of the toolbox.
        /// </summary>
        public string? RawXml { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this toolbox uses events.
        /// </summary>
        public bool UseEvents { get; set; }

        /// <summary>
        /// Exports the definition as an XML string.
        /// </summary>
        /// <param name="format">The format to use for the definition.</param>
        /// <returns>A string containing the XML definition.</returns>
        public string? ExportToXml(Format format)
        {
            return format switch
            {
                Format.Toolbox => ExportToToolboxXml(),
                Format.Blockly => ExportToBlocklyXml(),
                _ => throw new Exception($"Unknown format: {format}"),
            };
        }

        private static void ExportNextBlock(int position, XElement parent, ToolboxCategory category)
        {
            var block = category.Blocks[position];
            var blockEl = new XElement(
                "block",
                new XAttribute("type", block.Name));
            parent.Add(blockEl);
            if ((position + 1) < category.Blocks.Count)
            {
                var nextEl = new XElement("next");
                ExportNextBlock(position + 1, nextEl, category);
                blockEl.Add(nextEl);
            }
        }

        private void ExportNextCategory(int position, XElement parent)
        {
            var category = this.Categories[position];
            var categoryEl = new XElement(
                "block",
                new XAttribute("type", "category"));
            categoryEl.Add(new XElement(
                "field",
                new XAttribute("name", "NAME"),
                new XText(category.Name)));
            categoryEl.Add(new XElement(
                "field",
                new XAttribute("name", "OPTIONAL"),
                new XText(category.IsOptional ? "TRUE" : "FALSE")));
            categoryEl.Add(new XElement(
                "field",
                new XAttribute("name", "COLOUR"),
                new XText(category.Colour)));
            parent.Add(categoryEl);
            var blocksEl = new XElement(
                "statement",
                new XAttribute("name", "BLOCKS"));
            categoryEl.Add(blocksEl);
            if (category.Blocks.Any()) ExportNextBlock(0, blocksEl, category);
            if ((position + 1) < this.Categories.Count)
            {
                var nextEl = new XElement("next");
                ExportNextCategory(position + 1, nextEl);
                categoryEl.Add(nextEl);
            }
        }

        private string ExportToBlocklyXml()
        {
            var root = new XElement("xml");
            if (this.Categories.Any())
            {
                ExportNextCategory(0, root);
            }

            var doc = new XDocument(root);
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private string ExportToToolboxXml()
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