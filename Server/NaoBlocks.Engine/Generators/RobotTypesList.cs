using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the robot types list export.
    /// </summary>
    public class RobotTypesList
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robot types list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var generator = new Generator();
            var table = generator.AddTable("Robot Types");
            table.AddRow(
                TableRowType.Header,
                "Name",
                "Is Default",
                "# Toolboxes",
                "When Added");

            var types = await this.Session.Query<RobotType>()
                .OrderBy(r => r.Name)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var robotType in types)
            {
                table.AddRow(
                    robotType.Name,
                    robotType.IsDefault,
                    robotType.Toolboxes.Count(),
                    robotType.WhenAdded);
            }

            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, "Robot-Type-List");
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