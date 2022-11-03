using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates a robot type details export.
    /// </summary>
    public class RobotTypeExport
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robots list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var generator = new Generator();

            var page = generator.AddPage("Details");
            page.AddParagraph(
                new PageBlock("Name", true),
                new PageBlock(this.RobotType.Name));
            page.AddParagraph(
                new PageBlock("Is Default", true),
                new PageBlock(this.RobotType.IsDefault));

            var table = generator.AddTable("Toolboxes");
            table.AddRow(
                TableRowType.Header,
                "Name",
                "Is Default",
                "Uses Events",
                "Categories",
                "Definition");
            foreach (var toolbox in this.RobotType.Toolboxes)
            {
                table.AddRow(
                    toolbox.Name,
                    toolbox.IsDefault,
                    toolbox.UseEvents,
                    string.Join(",", toolbox.Categories.Select(c => c.Name)),
                    toolbox.ExportToXml(Toolbox.Format.Toolbox));
            }

            table = generator.AddTable("Robots");
            table.AddRow(
                TableRowType.Header,
                "Machine Name",
                "Friendly Name",
                "When Added",
                "Initialized");

            var robots = await this.Session.Query<Robot>()
                .OrderBy(r => r.MachineName)
                .Where(r => r.RobotTypeId == this.RobotType.Id)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var robot in robots)
            {
                table.AddRow(
                    robot.MachineName,
                    robot.FriendlyName,
                    robot.WhenAdded,
                    robot.IsInitialised);
            }

            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, $"Robot-Type-Export-{this.RobotType.Name}");
            return Tuple.Create(stream, name);
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
                ReportFormat.Excel => true,
                ReportFormat.Pdf => true,
                ReportFormat.Text => true,
                ReportFormat.Csv => true,
                _ => false,
            };
        }
    }
}