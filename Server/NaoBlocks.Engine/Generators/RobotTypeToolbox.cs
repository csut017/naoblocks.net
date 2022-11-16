using NaoBlocks.Engine.Data;
using System.Text;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Exports a toolbox for a robot type.
    /// </summary>
    public class RobotTypeToolbox
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robots list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var type = await this.Session
                .LoadAsync<RobotType>(this.RobotType.Id)
                .ConfigureAwait(false);
            var toolboxName = this.GetArgumentOrDefault("toolbox", "unknown");
            var toolbox = type
                ?.Toolboxes
                .FirstOrDefault(t => t.Name == toolboxName);
            if (toolbox == null)
            {
                throw new Exception($"Unknown toolbox {toolboxName}");
            }

            var xml = toolbox.ExportToXml(Toolbox.Format.Toolbox);

            Stream stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
            writer.Write(xml);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return Tuple.Create(stream!, $"{this.RobotType.Name}-toolbox.xml");
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
                ReportFormat.Xml => true,
                _ => false,
            };
        }
    }
}