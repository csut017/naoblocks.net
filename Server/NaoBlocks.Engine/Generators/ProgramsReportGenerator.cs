using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Helper methods for generating programs reports.
    /// </summary>
    public static class ProgramsReportGenerator
    {
        /// <summary>
        /// Generates the programs table.
        /// </summary>
        /// <param name="reportGenerator">The <see cref="ReportGenerator"/> to use.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="generator">The generator to use.</param>
        public static async Task GenerateProgramsForUserAsync(this ReportGenerator reportGenerator, Generator generator, DateTime fromDate, DateTime toDate)
        {
            var table = generator.AddTable("Programs");
            var programs = await reportGenerator.Session
                .Query<CodeProgram>()
                .Where(p => p.UserId == reportGenerator.User.Name)
                .Where(p => p.WhenAdded >= fromDate && p.WhenAdded <= toDate)
                .ToListAsync()
                .ConfigureAwait(false);
            table.AddRow(TableRowType.Header,
                "Number",
                "When Added",
                "Name",
                "Program");

            foreach (var program in programs)
            {
                table.AddRow(
                    program.Number,
                    program.WhenAdded,
                    program.Name,
                    program.Code);
            }
        }
    }
}