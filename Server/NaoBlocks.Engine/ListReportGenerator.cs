using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using Raven.Client.Documents;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// A report generator for generating lists.
    /// </summary>
    public abstract class ListReportGenerator
        : ReportGenerator
    {
        /// <summary>
        /// Adds a table containing the robots in the system.
        /// </summary>
        /// <param name="generator">The <see cref="Generator"/> to use.</param>
        protected async Task PopulateRobots(Generator generator)
        {
            var table = generator.AddTable("Robots");
            var useImportFormat = GetArgumentOrDefault("import", "no") == "yes";
            if (useImportFormat)
            {
                table.AddRow(
                    TableRowType.Header,
                    "Machine Name",
                    "Friendly Name",
                    "Type",
                    "Password");
            }
            else
            {
                table.AddRow(
                    TableRowType.Header,
                    "Machine Name",
                    "Friendly Name",
                    "Type",
                    "When Added",
                    "Initialized");
            }

            var robots = await this.Session.Query<Robot>()
                .Include(r => r.RobotTypeId)
                .OrderBy(r => r.MachineName)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var robot in robots)
            {
                var type = await this.Session
                    .LoadAsync<RobotType>(robot.RobotTypeId)
                    .ConfigureAwait(false);
                if (useImportFormat)
                {
                    table.AddRow(
                        robot.MachineName,
                        robot.FriendlyName,
                        type?.Name,
                        string.Empty);
                }
                else
                {
                    table.AddRow(
                        robot.MachineName,
                        robot.FriendlyName,
                        type?.Name,
                        robot.WhenAdded,
                        robot.IsInitialised);
                }
            }

            table.EnsureAllRowsSameLength();
        }

        /// <summary>
        /// Adds a table containing the robot types in the system.
        /// </summary>
        /// <param name="generator">The <see cref="Generator"/> to use.</param>
        protected async Task PopulateRobotTypes(Generator generator)
        {
            var table = generator.AddTable("Robot Types");
            table.AddRow(
                TableRowType.Header,
                "Name",
                "Is Default",
                "# Toolboxes",
                "When Added");

            var types = await this.Session.Query<RobotType>()
                .OrderBy(r => r.Name)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var robotType in types)
            {
                table.AddRow(
                    robotType.Name,
                    robotType.IsDefault,
                    robotType.Toolboxes.Count(),
                    robotType.WhenAdded);
            }

            table.EnsureAllRowsSameLength();
        }

        /// <summary>
        /// Adds a table containing the students in the system.
        /// </summary>
        /// <param name="generator">The <see cref="Generator"/> to use.</param>
        protected async Task PopulateStudents(Generator generator)
        {
            var table = generator.AddTable("Students");
            table.AddRow(
                TableRowType.Header,
                "Name",
                "Robot Type",
                "When Added",
                "Toolbox",
                "Age",
                "Gender",
                "View Mode",
                "Allocation Mode",
                "Allocated Robot");

            var students = await this.Session
                .Query<User>()
                .Where(u => u.Role == UserRole.Student)
                .OrderBy(u => u.Name)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var student in students)
            {
                if (student.Settings.RobotType == null)
                {
                    table.AddRow(student.Name, null, student.WhenAdded);
                }
                else
                {
                    table.AddRow(
                        student.Name,
                        student.Settings.RobotType,
                        student.WhenAdded,
                        student.Settings.Toolbox ?? string.Empty,
                        student.StudentDetails?.Age,
                        student.StudentDetails?.Gender,
                        ValueMappings.ViewModes[student.Settings.ViewMode],
                        ValueMappings.AllocationModes[student.Settings.AllocationMode],
                        student.Settings.AllocationMode > 0 ? student.Settings.RobotId : string.Empty);
                }
            }

            table.EnsureAllRowsSameLength();
        }
    }
}