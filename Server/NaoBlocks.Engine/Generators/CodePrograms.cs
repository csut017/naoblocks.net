using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Globalization;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Exports the code programs.
    /// </summary>
    public class CodePrograms
        : ReportGenerator
    {
        /// <summary>
        /// Generates the code programs report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddDays(-7);
            var fromDateText = this.GetArgumentOrDefault("from");
            var toDateText = this.GetArgumentOrDefault("to");
            if (!string.IsNullOrEmpty(fromDateText))
            {
                if (!DateTime.TryParseExact(fromDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
                {
                    throw new ApplicationException($"From date is invalid, it should be yyyy-MM-dd, found {fromDateText}");
                }
            }
            if (!string.IsNullOrEmpty(toDateText))
            {
                if (!DateTime.TryParseExact(toDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                {
                    throw new ApplicationException($"To date is invalid, it should be yyyy-MM-dd, found {toDateText}");
                }
            }

            var generator = new Generator();
            var table = generator.AddTable("Logs");
            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();
            List<CodeProgram> programs;
            var reportName = "All";
            var includeUser = true;
            if (this.HasUser)
            {
                includeUser = false;
                var userId = this.User.Id;
                reportName = this.User.Name;
                programs = await this.Session
                    .Query<CodeProgram>()
                    .Where(cp => cp.UserId == userId)
                    .Include(cp => cp.UserId)
                    .Where(cp => (cp.WhenAdded >= fromDate) && (cp.WhenAdded <= toDate))
                    .OrderByDescending(cp => cp.WhenAdded)
                    .ToListAsync();
                table.AddRow(
                    TableRowType.Header,
                    "Number",
                    "Name",
                    "When Added",
                    "Code");
            }
            else
            {
                programs = await this.Session
                    .Query<CodeProgram>()
                    .Include(cp => cp.UserId)
                    .Where(cp => (cp.WhenAdded >= fromDate) && (cp.WhenAdded <= toDate))
                    .OrderByDescending(cp => cp.WhenAdded)
                    .ToListAsync();
                table.AddRow(
                    TableRowType.Header,
                    "Person",
                    "Number",
                    "Name",
                    "When Added",
                    "Code");
            }

            foreach (var program in programs)
            {
                if (includeUser)
                {
                    var user = await this.Session
                        .LoadAsync<User>(program.UserId)
                        .ConfigureAwait(false);
                    table.AddRow(
                        user?.Name ?? string.Empty,
                        program.Number,
                        program.Name ?? string.Empty,
                        program.WhenAdded.ToLocalTime(),
                        program.Code);
                }
                else
                {
                    table.AddRow(
                        program.Number,
                        program.Name ?? string.Empty,
                        program.WhenAdded.ToLocalTime(),
                        program.Code);
                }
            }

            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, $"{reportName}-programs");
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