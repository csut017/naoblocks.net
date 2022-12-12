using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using OfficeOpenXml;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for parsing a robot import file.
    /// </summary>
    [Transient]
    public class ParseRobotsImport
        : CommandBase
    {
        private readonly List<ItemImport<Robot>> robots = new();

        /// <summary>
        /// Gets or sets the  data.
        /// </summary>
        public Stream? Data { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether validation should be skipped or not.
        /// </summary>
        public bool SkipValidation { get; set; }

        /// <summary>
        /// Validates the robot details.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (this.Data == null)
            {
                errors.Add(this.GenerateError("Data is required"));
            }

            if (!errors.Any())
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                try
                {
                    await package.LoadAsync(this.Data);
                }
                catch (Exception error)
                {
                    errors.Add(this.GenerateError($"Unable to open file: {error.Message}"));
                }

                if (!errors.Any())
                {
                    try
                    {
                        this.ParseExcel(package.Workbook);
                        if (!this.SkipValidation) await this.ValidateRobots(session).ConfigureAwait(false);
                    }
                    catch (Exception error)
                    {
                        errors.Add(this.GenerateError($"Unable to parse Excel workbook: {error.Message}"));
                    }
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Returns the results.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            return Task.FromResult(
                CommandResult.New(
                    this.Number,
                    this.robots.AsEnumerable()));
        }

        /// <summary>
        /// Parses the workbook into a series of <see cref="Robot"/> instance.
        /// </summary>
        /// <param name="workbook">The <see cref="ExcelWorkbook"/> to parse.</param>
        private void ParseExcel(ExcelWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.First();
            if (worksheet.Dimension?.End == null) throw new Exception("No data");

            var columns = worksheet.Dimension.End.Column;
            var rows = worksheet.Dimension.End.Row;
            var mappings = new Dictionary<int, Action<Robot, string>>();

            // Assuming the first row is headers
            for (var column = 1; column <= columns; column++)
            {
                var name = worksheet.Cells[1, column].Value?.ToString() ?? string.Empty;
                switch (name.Trim().ToLowerInvariant())
                {
                    case "machine name":
                    case "machine":
                    case "name":
                        mappings.Add(column, (robot, value) => robot.MachineName = value);
                        break;

                    case "friendly name":
                    case "friendly":
                        mappings.Add(column, (robot, value) => robot.FriendlyName = value);
                        break;

                    case "type":
                        mappings.Add(column, (robot, value) => robot.RobotTypeId = value);
                        break;

                    case "password":
                        mappings.Add(column, (robot, value) =>
                        {
                            if (!string.IsNullOrWhiteSpace(value)) robot.PlainPassword = value;
                        });
                        break;
                }
            }

            // Process the rest of the rows
            for (var row = 2; row <= rows; row++)
            {
                var robot = new Robot();
                foreach (var mapping in mappings)
                {
                    var value = worksheet.Cells[row, mapping.Key].Value?.ToString() ?? string.Empty;
                    mapping.Value(robot, value);
                }

                this.robots.Add(ItemImport.New(robot, row));
            }
        }

        private async Task ValidateRobots(IDatabaseSession session)
        {
            // Validate all the robots
            var robotTypes = new Dictionary<string, bool>();
            foreach (var record in this.robots)
            {
                var errors = new List<string>();
                var robot = record.Item!;
                if (!string.IsNullOrEmpty(robot.RobotTypeId))
                {
                    if (!robotTypes.TryGetValue(robot.RobotTypeId, out var isTypeValid))
                    {
                        isTypeValid = await session.Query<RobotType>()
                            .AnyAsync(rt => rt.Name == robot.RobotTypeId)
                            .ConfigureAwait(false);
                        robotTypes.Add(robot.RobotTypeId, isTypeValid);
                    }

                    if (!isTypeValid) errors.Add($"Unknown robot type '{robot.RobotTypeId}'");
                }
                else
                {
                    errors.Add($"Robot type is required");
                }

                if (string.IsNullOrEmpty(robot.FriendlyName)) robot.FriendlyName = robot.MachineName;
                if (string.IsNullOrEmpty(robot.MachineName))
                {
                    errors.Add($"Machine name is required");
                }
                else
                {
                    var robotExists = await session.Query<Robot>()
                        .AnyAsync(r => r.MachineName == robot.MachineName)
                        .ConfigureAwait(false);
                    if (robotExists) errors.Add($"Robot '{robot.MachineName}' already exists");
                }

                if (errors.Any())
                {
                    record.Message = string.Join(", ", errors);
                }
            }
        }
    }
}