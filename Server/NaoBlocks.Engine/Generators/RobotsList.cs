using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the robots list export.
    /// </summary>
    public class RobotsList
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
            var table = generator.AddTable("Robots");
            var useImportFormat = GetArgumentOrDefault("import", "no") == "yes";
            if (useImportFormat)
            {
                table.AddRow(
                    TableRowType.Header,
                    "Machine Name",
                    "Friendly Name",
                    "Type",
                    "Password");
            }
            else
            {
                table.AddRow(
                    TableRowType.Header,
                    "Machine Name",
                    "Friendly Name",
                    "Type",
                    "When Added",
                    "Initialized");
            }

            var robots = await this.Session.Query<Robot>()
                .Include(r => r.RobotTypeId)
                .OrderBy(r => r.MachineName)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var robot in robots)
            {
                var type = await this.Session
                    .LoadAsync<RobotType>(robot.RobotTypeId)
                    .ConfigureAwait(false);
                if (useImportFormat)
                {
                    table.AddRow(
                        robot.MachineName,
                        robot.FriendlyName,
                        type?.Name,
                        string.Empty);
                }
                else
                {
                    table.AddRow(
                        robot.MachineName,
                        robot.FriendlyName,
                        type?.Name,
                        robot.WhenAdded,
                        robot.IsInitialised);
                }
            }

            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, "Robot-List");
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