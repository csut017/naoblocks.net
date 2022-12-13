using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using OfficeOpenXml;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for parsing a user import file.
    /// </summary>
    [Transient]
    public class ParseUsersImport
        : CommandBase
    {
        private readonly List<ItemImport<User>> users = new();

        /// <summary>
        /// Gets or sets the  data.
        /// </summary>
        public Stream? Data { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        /// <remarks>
        /// If this value is set, it will override any incoming values.
        /// </remarks>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether validation should be skipped or not.
        /// </summary>
        public bool SkipValidation { get; set; }

        /// <summary>
        /// Validates the user details.
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
                        if (!this.SkipValidation) await this.ValidateUsers(session).ConfigureAwait(false);
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
                    this.users.AsEnumerable()));
        }

        /// <summary>
        /// Parses the workbook into a series of <see cref="User"/> instance.
        /// </summary>
        /// <param name="workbook">The <see cref="ExcelWorkbook"/> to parse.</param>
        private void ParseExcel(ExcelWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.First();
            if (worksheet.Dimension?.End == null) throw new Exception("No data");

            var columns = worksheet.Dimension.End.Column;
            var rows = worksheet.Dimension.End.Row;
            var mappings = new Dictionary<int, Action<User, string>>();
            var allocationModes = new Dictionary<string, int>
            {
                { "any", 0 },
                { "require", 1 },
                { "prefer", 2 },
            };
            var viewModes = new Dictionary<string, int>
            {
                { "blocks", 0 },
                { "tangibles", 1 },
            };

            // Assuming the first row is headers
            for (var column = 1; column <= columns; column++)
            {
                var name = worksheet.Cells[1, column].Value?.ToString() ?? string.Empty;
                switch (name.Trim().ToLowerInvariant())
                {
                    case "name":
                        mappings.Add(column, (user, value) => user.Name = value);
                        break;

                    case "age":
                        mappings.Add(column, (user, value) =>
                        {
                            if (!int.TryParse(value, out var age)) return;
                            if (user.StudentDetails == null) user.StudentDetails = new StudentDetails();
                            user.StudentDetails.Age = age;
                        });
                        break;

                    case "gender":
                        mappings.Add(column, (user, value) =>
                        {
                            if (user.StudentDetails == null) user.StudentDetails = new StudentDetails();
                            user.StudentDetails.Gender = value;
                        });
                        break;

                    case "password":
                        mappings.Add(column, (user, value) =>
                        {
                            if (!string.IsNullOrWhiteSpace(value)) user.PlainPassword = value;
                        });
                        break;

                    case "role":
                        mappings.Add(column, (user, value) =>
                        {
                            if (!Enum.TryParse<UserRole>(value, true, out var parsedRole)) return;
                            user.Role = parsedRole;
                        });
                        break;

                    case "robot type":
                    case "type":
                        mappings.Add(column, (user, value) =>
                        {
                            if (string.IsNullOrEmpty(value)) return;
                            user.Settings.RobotType = value;
                        });
                        break;

                    case "toolbox":
                        mappings.Add(column, (user, value) =>
                        {
                            if (string.IsNullOrEmpty(value)) return;
                            user.Settings.Toolbox = value;
                        });
                        break;

                    case "view mode":
                    case "view":
                        mappings.Add(column, (user, value) =>
                        {
                            if (viewModes.TryGetValue(value.ToLowerInvariant(), out int mode)) user.Settings.ViewMode = mode;
                        });
                        break;

                    case "allocation mode":
                    case "allocation":
                        mappings.Add(column, (user, value) =>
                        {
                            if (allocationModes.TryGetValue(value.ToLowerInvariant(), out int mode)) user.Settings.AllocationMode = mode;
                        });
                        break;

                    case "allocated robot":
                    case "robot":
                        mappings.Add(column, (user, value) =>
                        {
                            if (string.IsNullOrEmpty(value)) return;
                            user.Settings.RobotId = value;
                        });
                        break;
                }
            }

            // Process the rest of the rows
            for (var row = 2; row <= rows; row++)
            {
                var user = new User();
                var hasData = false;
                foreach (var mapping in mappings)
                {
                    var value = worksheet.Cells[row, mapping.Key].Value?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(value)) continue;
                    mapping.Value(user, value);
                    hasData = true;
                }

                if (!hasData) continue;
                if (this.Role != null) user.Role = this.Role.Value;
                this.users.Add(ItemImport.New(user, row));
            }
        }

        private async Task ValidateUsers(IDatabaseSession session)
        {
            // Validate all the users
            var robotTypes = new Dictionary<string, bool>();
            var robots = new Dictionary<string, bool>();
            foreach (var record in this.users)
            {
                var errors = new List<string>();
                var user = record.Item!;
                if (string.IsNullOrEmpty(user.Name))
                {
                    errors.Add($"Name is required [row {record.Row}]");
                    record.CanImport = false;
                }
                else
                {
                    var userExists = await session.Query<User>()
                        .AnyAsync(r => r.Name == user.Name)
                        .ConfigureAwait(false);
                    if (userExists)
                    {
                        errors.Add($"User '{user.Name}' already exists");
                        record.IsDuplicate = true;
                    }
                }

                if (!string.IsNullOrEmpty(user.Settings.RobotType))
                {
                    if (!robotTypes.TryGetValue(user.Settings.RobotType, out var isTypeValid))
                    {
                        isTypeValid = await session.Query<RobotType>()
                            .AnyAsync(rt => rt.Name == user.Settings.RobotType)
                            .ConfigureAwait(false);
                        robotTypes.Add(user.Settings.RobotType, isTypeValid);
                    }

                    if (!isTypeValid) errors.Add($"Unknown robot type '{user.Settings.RobotType}' [row {record.Row}]");
                }

                if (!string.IsNullOrEmpty(user.Settings.RobotId))
                {
                    if (!robotTypes.TryGetValue(user.Settings.RobotId, out var isTypeValid))
                    {
                        isTypeValid = await session.Query<Robot>()
                            .AnyAsync(rt => rt.MachineName == user.Settings.RobotId || rt.FriendlyName == user.Settings.RobotId)
                            .ConfigureAwait(false);
                        robotTypes.Add(user.Settings.RobotId, isTypeValid);
                    }

                    if (!isTypeValid) errors.Add($"Unknown robot '{user.Settings.RobotId}' [row {record.Row}]");
                }

                if (errors.Any())
                {
                    record.Message = string.Join(", ", errors);
                }
            }
        }
    }
}