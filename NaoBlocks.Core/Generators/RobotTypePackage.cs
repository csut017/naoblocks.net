using Ionic.Zip;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NaoBlocks.Core.Generators
{
    public static class RobotTypePackage
    {
        public static Task<Stream> GeneraAsync(RobotType? type, IAsyncDocumentSession? session)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (session == null) throw new ArgumentNullException(nameof(session));

            // Generate the manifest
            var manifest = new Manifest
            {
                Name = type.Name,
                IsDefault = type.IsDefault
            };
            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var manifestJson = JsonSerializer.Serialize(manifest, opts);

            // Generate the toolbox
            var toolboxEl = new XElement("toolbox",
                type.Toolbox.OrderBy(c => c.Order).ThenBy(c => c.Name).Select(FormatCategory).ToArray());
            var toolbox = new XDocument(toolboxEl);

            // Zip everything together
            var stream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                zip.AddEntry("manifest.json", manifestJson);
                zip.AddEntry("toolbox.xml", toolbox.ToString());
                zip.Save(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult((Stream)stream);
        }

        private static XElement FormatCategory(ToolboxCategory category)
        {
            var el = new XElement("category",
                category.Blocks.OrderBy(b => b.Order).ThenBy(b => b.Name).Select(FormatBlock).ToArray());
            el.Add(new XAttribute("name", category.Name));
            el.Add(new XAttribute("colour", category.Colour));
            if (category.Tags.Any()) el.Add(new XAttribute("tags", string.Join(" ", category.Tags)));
            if (!string.IsNullOrEmpty(category.Custom)) el.Add(new XAttribute("custom", category.Custom));

            return el;
        }

        private static XElement FormatBlock(ToolboxBlock block)
        {
            return XElement.Parse(block.Definition);
        }

        private class Manifest
        {
            public string? Name { get; set; }
            
            public bool IsDefault { get; set; }


        }
    }
}
