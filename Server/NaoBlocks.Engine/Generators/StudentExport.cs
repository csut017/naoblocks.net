namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the student details export.
    /// </summary>
    public class StudentExport
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
            GenerateStudentDetails(generator);
            var includeLogs = this.GetArgumentOrDefault("logs", "yes");
            if ("yes".Equals(includeLogs) || "true".Equals(includeLogs)) await this.GenerateLogsAsync(generator, fromDate, toDate);
            var includePrograms = this.GetArgumentOrDefault("programs", "yes");
            if ("yes".Equals(includePrograms) || "true".Equals(includePrograms)) await this.GenerateProgramsAsync(generator, fromDate, toDate);
            var (stream, name) = await generator.GenerateAsync(format, $"Student-Export-{this.User.Name}");
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

        private void GenerateStudentDetails(Generator generator)
        {
            var page = generator.AddPage("Details");
            page.AddParagraph(
                new PageBlock("Name", true),
                new PageBlock(this.User.Name));
            if (this.User?.StudentDetails != null)
            {
                var studentDetails = this.User.StudentDetails;
                page.AddParagraph(
                    new PageBlock("Gender", true),
                    new PageBlock(studentDetails.Gender ?? "Unknown"));
                page.AddParagraph(
                    new PageBlock("Age", true),
                    new PageBlock(studentDetails.Age ?? 0));
            }
        }
    }
}