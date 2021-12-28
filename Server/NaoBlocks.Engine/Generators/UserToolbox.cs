namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the toolbox for a user.
    /// </summary>
    public class UserToolbox
        : ReportGenerator
    {
        /// <summary>
        /// Generates the students list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            throw new System.NotImplementedException();
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
                ReportFormat.Text => true,
                _ => false,
            };
        }
    }
}