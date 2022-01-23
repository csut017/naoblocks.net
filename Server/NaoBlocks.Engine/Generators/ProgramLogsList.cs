using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the program logs list.
    /// </summary>
    public class ProgramLogsList
        : ReportGenerator
    {
        /// <summary>
        /// Generates the students list report.
        /// </summary>
        /// <param name="format">The export format.</param>
        /// <returns>The output <see cref="Stream"/> containing the generated data.</returns>
        public override async Task<Tuple<Stream, string>> GenerateAsync(ReportFormat format)
        {
            var generator = new Generator();
            await this.GenerateLogsAsync(generator);
            await this.GenerateProgramsAsync(generator);
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

        /// <summary>
        /// Generates the logs table.
        /// </summary>
        /// <param name="generator">The generator to use.</param>
        private async Task GenerateLogsAsync(Generator generator)
        {
            var table = generator.AddTable("Logs");
            var data = await this.Session.Query<RobotLog>()
                .Include(rl => rl.RobotId)
                .Where(rl => rl.Conversation.SourceId == this.User.Id)
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
        /// <param name="generator">The generator to use.</param>
        private async Task GenerateProgramsAsync(Generator generator)
        {
            var table = generator.AddTable("Programs");
            var programs = await this.Session
                .Query<CodeProgram>()
                .Where(p => p.UserId == this.User.Name)
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