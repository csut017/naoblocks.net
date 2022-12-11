namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Exports the robot logs.
    /// </summary>
    public class RobotLogs
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robot type logs report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var (fromDate, toDate) = this.ParseFromToDates();
            var generator = new Generator();

            var reportName = this.HasRobotType
                ? this.RobotType.Name
                : "All";
            await this.GenerateLogsForRobotAsync(generator, fromDate, toDate, !this.HasRobotType);

            var (stream, name) = await generator.GenerateAsync(format, $"{reportName}-logs");
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
                ReportFormat.Csv => true,
                ReportFormat.Text => true,
                ReportFormat.Xml => true,
                _ => false,
            };
        }
    }
}