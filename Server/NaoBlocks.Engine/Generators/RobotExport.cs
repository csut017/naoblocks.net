using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the robot export.
    /// </summary>
    public class RobotExport
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robot export report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var (fromDate, toDate) = this.ParseFromToDates();
            var generator = new Generator();
            await GenerateRobotDetails(generator);
            var includeLogs = this.GetArgumentOrDefault("logs", "yes");
            if ("yes".Equals(includeLogs) || "true".Equals(includeLogs)) await this.GenerateLogsForRobotAsync(generator, fromDate, toDate);
            generator.Title = "Robot Details";
            generator.IsLandScape = true;
            var (stream, name) = await generator.GenerateAsync(format, $"Robot-Export-{this.Robot.FriendlyName}");
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
                ReportFormat.Xml => true,
                _ => false,
            };
        }

        private async Task GenerateRobotDetails(Generator generator)
        {
            var page = generator.AddPage("Details");
            if (this.Robot.Type == null)
            {
                this.Robot.Type = await this.Session
                    .Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Id == this.Robot.RobotTypeId);
            }

            page.AddParagraph(
                new PageBlock("Machine Name", true),
                new PageBlock(this.Robot.MachineName));
            page.AddParagraph(
                new PageBlock("Friendly Name", true),
                new PageBlock(this.Robot.FriendlyName));
            if (this.Robot.Type == null)
            {
                this.Robot.Type = await this.Session.LoadAsync<RobotType>(this.Robot.RobotTypeId);
            }
            page.AddParagraph(
                new PageBlock("Type", true),
                new PageBlock(this.Robot.Type?.Name ?? "<Unknown>"));
        }
    }
}