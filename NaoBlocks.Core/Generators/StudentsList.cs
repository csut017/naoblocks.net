using NaoBlocks.Core.Models;
using OfficeOpenXml;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Generators
{
    public static class StudentsList
    {
        public static async Task<byte[]> GenerateAsync(IAsyncDocumentSession session)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Students");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Robot";
            worksheet.Cells[1, 3].Value = "When Added";
            worksheet.Cells[1, 4].Value = "Mode";
            worksheet.Cells[1, 5].Value = "Dances";
            worksheet.Cells[1, 6].Value = "Conditionals";
            worksheet.Cells[1, 7].Value = "Loops";
            worksheet.Cells[1, 8].Value = "Sensors";
            worksheet.Cells[1, 9].Value = "Variables";
            worksheet.Cells[1, 10].Value = "Tutorial";

            using (var range = worksheet.Cells[1, 1, 1, 10])
            {
                range.Style.Font.Bold = true;
            }

            var students = await session.Query<User>()
                .Where(u => u.Role == UserRole.Student)
                .ToListAsync()
                .ConfigureAwait(false);
            var row = 1;
            foreach (var student in students)
            {
                row++;
                worksheet.Cells[row, 1].Value = student.Name;
                worksheet.Cells[row, 3].Value = student.WhenAdded;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-mmm-yyyy";
                if (student.Settings != null)
                {
                    worksheet.Cells[row, 2].Value = student.Settings.RobotType;
                    worksheet.Cells[row, 4].Value = student.Settings.Simple ? "Simple" : (student.Settings.Events ? "Events" : "Default");
                    worksheet.Cells[row, 5].Value = student.Settings.Dances;
                    worksheet.Cells[row, 6].Value = student.Settings.Conditionals;
                    worksheet.Cells[row, 7].Value = student.Settings.Loops;
                    worksheet.Cells[row, 8].Value = student.Settings.Sensors;
                    worksheet.Cells[row, 9].Value = student.Settings.Variables;
                    worksheet.Cells[row, 10].Value = student.Settings.CurrentTutorial;
                }

            }

            worksheet.Cells[$"A1:J{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            return await package.GetAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
