using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the snapshots list.
    /// </summary>
    public class SnapshotsList
        : ReportGenerator
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
            var table = generator.AddTable("Snapshots");
            var data = await this.Session
                .Query<Snapshot>()
                .Where(s => s.UserId == this.User.Id)
                .Where(s => s.WhenAdded >= fromDate && s.WhenAdded <= toDate)
                .OrderBy(s => s.WhenAdded)
                .ToListAsync()
                .ConfigureAwait(false);

            var header = table.AddRow(
                TableRowType.Header,
                "Date/Time",
                "Source",
                "State");
            var columns = new Dictionary<string, int>();
            foreach (var snapshot in data)
            {
                var row = table.AddRow(
                    new TableCell(snapshot.WhenAdded, "yyyy-MM-dd HH:mm:ss"),
                    snapshot.Source,
                    snapshot.State);
                foreach (var value in snapshot.Values)
                {
                    if (!columns.TryGetValue(value.Name, out int column))
                    {
                        column = header.Values.Count;
                        columns[value.Name] = column;
                        header.Values.Add(value.Name);
                    }
                    row.SetCell(column, value.Value);
                }
            }
            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, $"Snapshots-{this.User.Name}");
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