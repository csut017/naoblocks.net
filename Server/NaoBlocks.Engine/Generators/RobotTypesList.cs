namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the robot types list export.
    /// </summary>
    public class RobotTypesList
        : ListReportGenerator
    {
        /// <summary>
        /// Generates the robot types list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var generator = new Generator();
            await PopulateRobotTypes(generator).ConfigureAwait(false);
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