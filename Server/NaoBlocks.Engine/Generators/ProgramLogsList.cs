namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the program logs list.
    /// </summary>
    public class ProgramLogsList
        : UserReportGenerator
    {
        /// <summary>
        /// Generates the students list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var (fromDate, toDate) = this.ParseFromToDates();
            var generator = new Generator();
            await this.GenerateLogsAsync(generator, fromDate, toDate);
            await this.GenerateProgramsAsync(generator, fromDate, toDate);
            var (stream, name) = await generator.GenerateAsync(format, $"Student-Logs-{this.User.Name}");
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
                _ => false,
            };
        }
    }
}