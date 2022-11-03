using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Indices;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Helper methods for generating user reports.
    /// </summary>
    public static class LogsReportGenerator
    {
        /// <summary>
        /// Generates the logs table for a user.
        /// </summary>
        /// <param name="reportGenerator">The <see cref="ReportGenerator"/> to use.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="generator">The generator to use.</param>
        public static async Task GenerateLogsForRobotAsync(this ReportGenerator reportGenerator, Generator generator, DateTime fromDate, DateTime toDate)
        {
            var table = generator.AddTable("Logs");
            var header = table.AddRow(
                TableRowType.Header,
                "Robot",
                "Date",
                "Conversation",
                "Time",
                "Type",
                "Description");
            var columns = new Dictionary<string, int>();

            fromDate = fromDate.ToUniversalTime();
            toDate = toDate.ToUniversalTime();
            List<RobotLog> logs;
            if (reportGenerator.HasRobotType)
            {
                var robotTypeId = reportGenerator.RobotType.Id;
                logs = await reportGenerator.Session
                    .Query<RobotLogByRobotTypeId.Result, RobotLogByRobotTypeId>()
                    .Where(rl => rl.RobotTypeId == robotTypeId)
                    .OfType<RobotLog>()
                    .Include(rl => rl.RobotId)
                    .Where(rl => (rl.WhenAdded >= fromDate) && (rl.WhenAdded <= toDate))
                    .OrderByDescending(rl => rl.WhenAdded)
                    .ToListAsync();
            }
            else
            {
                logs = await reportGenerator.Session
                    .Query<RobotLogByRobotTypeId.Result, RobotLogByRobotTypeId>()
                    .OfType<RobotLog>()
                    .Include(rl => rl.RobotId)
                    .Where(rl => (rl.WhenAdded >= fromDate) && (rl.WhenAdded <= toDate))
                    .OrderByDescending(rl => rl.WhenAdded)
                    .ToListAsync();
            }

            foreach (var log in logs)
            {
                var robot = await reportGenerator.Session
                    .LoadAsync<Robot>(log.RobotId)
                    .ConfigureAwait(false);
                foreach (var line in log.Lines)
                {
                    var row = table.AddRow(
                        robot?.FriendlyName,
                        log!.WhenAdded.ToLocalTime(),
                        log.Conversation.ConversationId,
                        new TableCell(line.WhenAdded.ToLocalTime(), "HH:mm:ss"),
                        line.SourceMessageType.ToString(),
                        line.Description);
                    foreach (var value in line.Values)
                    {
                        if (string.Equals("token", value.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Ignore any security tokens
                            // They are not needed in the analysis and could be a potential security hole
                            continue;
                        }

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
        /// Generates the logs table for a user.
        /// </summary>
        /// <param name="reportGenerator">The <see cref="ReportGenerator"/> to use.</param>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="generator">The generator to use.</param>
        public static async Task GenerateLogsForUserAsync(this ReportGenerator reportGenerator, Generator generator, DateTime fromDate, DateTime toDate)
        {
            var table = generator.AddTable("Logs");
            var data = await reportGenerator.Session.Query<RobotLog>()
                .Include(rl => rl.RobotId)
                .Where(rl => rl.Conversation.SourceId == reportGenerator.User.Id)
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
                var robot = await reportGenerator.Session
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
    }
}