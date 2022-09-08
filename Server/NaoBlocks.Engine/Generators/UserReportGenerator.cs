using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Helper methods for generating user reports.
    /// </summary>
    public abstract class UserReportGenerator
        : ReportGenerator
    {
        /// <summary>
        /// Generates the logs table.
        /// </summary>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="generator">The generator to use.</param>
        protected async Task GenerateLogsAsync(Generator generator, DateTime fromDate, DateTime toDate)
        {
            var table = generator.AddTable("Logs");
            var data = await this.Session.Query<RobotLog>()
                .Include(rl => rl.RobotId)
                .Where(rl => rl.Conversation.SourceId == this.User.Id)
                .Where(rl => rl.WhenAdded >= fromDate && rl.WhenAdded <= toDate)
                .ToListAsync()
                .ConfigureAwait(false);

            var header = table.AddRow(
                TableRowType.Header,
                "Robot",
                "Date",
                "Conversation",
                "Time",
                "Type",
                "Description");
            var columns = new Dictionary<string, int>();
            foreach (var log in data.Where(l => l != null))
            {
                var robot = await this.Session
                    .LoadAsync<Robot>(log.RobotId)
                    .ConfigureAwait(false);
                foreach (var line in log.Lines)
                {
                    var row = table.AddRow(
                        robot?.FriendlyName,
                        log!.WhenAdded,
                        log.Conversation.ConversationId,
                        new TableCell(line.WhenAdded, "HH:mm:ss"),
                        line.SourceMessageType.ToString(),
                        line.Description);
                    foreach (var value in line.Values)
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
            }
            table.EnsureAllRowsSameLength();
        }

        /// <summary>
        /// Generates the programs table.
        /// </summary>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="generator">The generator to use.</param>
        protected async Task GenerateProgramsAsync(Generator generator, DateTime fromDate, DateTime toDate)
        {
            var table = generator.AddTable("Programs");
            var programs = await this.Session
                .Query<CodeProgram>()
                .Where(p => p.UserId == this.User.Name)
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