using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using OfficeOpenXml;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for parsing a robot import file.
    /// </summary>
    [Transient]
    public class ParseRobotsImport
        : CommandBase
    {
        private readonly List<Robot> robots = new();

        /// <summary>
        /// Gets or sets the  data.
        /// </summary>
        public Stream? Data { get; set; }

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
        /// Adds the robot to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            return Task.FromResult(CommandResult.New(this.Number, this.robots.AsReadOnly()));
        }

        /// <summary>
        /// Parses the workbook into a series of <see cref="Robot"/> instance.
        /// </summary>
        /// <param name="workbook">The <see cref="ExcelWorkbook"/> to parse.</param>
        private void ParseExcel(ExcelWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.First();
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
                        mappings.Add(column, (robot, value) => robot.MachineName = value);
                        break;

                    case "friendly name":
                        mappings.Add(column, (robot, value) => robot.FriendlyName = value);
                        break;

                    case "type":
                        mappings.Add(column, (robot, value) => robot.RobotTypeId = value);
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

                this.robots.Add(robot);
            }
        }
    }
}