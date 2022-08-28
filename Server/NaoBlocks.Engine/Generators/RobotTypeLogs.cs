﻿using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Indices;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Globalization;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Exports the logs for a robot type.
    /// </summary>
    public class RobotTypeLogs
        : ReportGenerator
    {
        /// <summary>
        /// Generates the robot type logs report.
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
            var robotTypeId = this.RobotType.Id;
            var logs = await this.Session
                .Query<RobotLogByRobotTypeId.Result, RobotLogByRobotTypeId>()
                .Where(rl => rl.RobotTypeId == robotTypeId)
                .OfType<RobotLog>()
                .Include(rl => rl.RobotId)
                .Where(rl => (rl.WhenAdded >= fromDate) && (rl.WhenAdded <= toDate))
                .OrderByDescending(rl => rl.WhenAdded)
                .ToListAsync();
            foreach (var log in logs)
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
            var (stream, name) = await generator.GenerateAsync(format, $"{this.RobotType.Name}-logs");
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