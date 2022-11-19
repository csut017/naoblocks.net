namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates an export with all lists.
    /// </summary>
    public class AllLists
        : ListReportGenerator
    {
        /// <summary>
        /// Generates the robots list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var generator = new Generator();
            if (GetArgumentOrDefault("robots", "no") == "yes") await PopulateRobots(generator).ConfigureAwait(false);
            if (GetArgumentOrDefault("students", "no") == "yes") await PopulateStudents(generator).ConfigureAwait(false);
            if (GetArgumentOrDefault("types", "no") == "yes") await PopulateRobotTypes(generator).ConfigureAwait(false);
            var (stream, name) = await generator.GenerateAsync(format, "All-Lists");
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