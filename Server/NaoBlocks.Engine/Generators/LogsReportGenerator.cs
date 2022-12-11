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
        /// <param name="includeRobotType">Whether to include the robot type or not.</param>
        public static async Task GenerateLogsForRobotAsync(this ReportGenerator reportGenerator, Generator generator, DateTime fromDate, DateTime toDate, bool includeRobotType)
        {
            var table = generator.AddTable("Logs");
            var cells = new List<TableCell> {
                "Robot",
                "Date",
                "Conversation",
                "Time",
                "Type",
                "Description"
            };
            if (includeRobotType) cells.Insert(0, "Robot Type");
            var header = table.AddRow(
                TableRowType.Header,
                cells.ToArray());
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

            var types = new Dictionary<string, string>();
            foreach (var log in logs)
            {
                var robot = await reportGenerator.Session
                    .LoadAsync<Robot>(log.RobotId)
                    .ConfigureAwait(false);
                foreach (var line in log.Lines)
                {
                    cells = new List<TableCell> {
                        robot?.FriendlyName,
                        log!.WhenAdded.ToLocalTime(),
                        log.Conversation.ConversationId,
                        new TableCell(line.WhenAdded.ToLocalTime(), "HH:mm:ss"),
                        line.SourceMessageType.ToString(),
                        line.Description
                    };
                    if (includeRobotType && !string.IsNullOrEmpty(robot?.RobotTypeId))
                    {
                        if (!types.TryGetValue(robot.RobotTypeId, out string? typeName))
                        {
                            var robotType = await reportGenerator.Session
                                .LoadAsync<RobotType>(robot.RobotTypeId)
                                .ConfigureAwait(false);
                            typeName = robotType?.Name ?? "<Unknown>";
                            types[robot.RobotTypeId] = typeName;
                        }
                        cells.Insert(0, typeName);
                    }

                    var row = table.AddRow(cells.ToArray());
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