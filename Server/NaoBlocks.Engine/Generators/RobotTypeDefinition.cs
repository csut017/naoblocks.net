using NaoBlocks.Engine.Data;
using System.Text.Json;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates a robot type definition.
    /// </summary>
    public class RobotTypeDefinition
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robots list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var definition = new
            {
                name = this.RobotType.Name,
                isDefault = this.RobotType.IsDefault,
                directLogging = this.RobotType.AllowDirectLogging,
                toolboxes = this.RobotType.Toolboxes.Select(tb => new
                {
                    name = tb.Name,
                    isDefault = tb.IsDefault,
                    definition = tb.ExportToXml(Toolbox.Format.Toolbox)
                }),
                values = this.RobotType.CustomValues.Select(v => new
                {
                    name = v.Name,
                    value = v.Value
                }),
                templates = this.RobotType.LoggingTemplates.Select(t => new
                {
                    category = t.Category,
                    text = t.Text,
                    type = t.MessageType.ToString(),
                    values = string.Join(",", t.ValueNames)
                })
            };
            var options = new JsonSerializerOptions { WriteIndented = false };
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, definition, options).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return Tuple.Create((Stream)stream, $"Robot-Type-Definition-{this.RobotType.Name}.json");
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
                ReportFormat.Json => true,
                _ => false,
            };
        }
    }
}