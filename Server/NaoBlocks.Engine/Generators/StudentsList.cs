using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Generates the students list export.
    /// </summary>
    public class StudentsList
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
            var table = generator.AddTable("Students");
            table.AddRow(
                TableRowType.Header,
                "Name",
                "Robot",
                "When Added",
                "Mode",
                "Dances",
                "Conditionals",
                "Loops",
                "Sensors",
                "Variables",
                "Age",
                "Gender");

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
                        student.Settings.Simple ? "Simple" : (student.Settings.Events ? "Events" : "Default"),
                        student.Settings.Dances,
                        student.Settings.Conditionals,
                        student.Settings.Loops,
                        student.Settings.Sensors,
                        student.Settings.Variables,
                        student.StudentDetails?.Age,
                        student.StudentDetails?.Gender);
                }
            }

            table.EnsureAllRowsSameLength();
            var (stream, name) = await generator.GenerateAsync(format, "Student-List");
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
                ReportFormat.Csv => true,
                ReportFormat.Text => true,
                _ => false,
            };
        }
    }
}