using NaoBlocks.Core.Models;
using OfficeOpenXml;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using NaoBlocks.Parser;

namespace NaoBlocks.Core.Generators
{
    public static class StudentDetails
    {
        public static async Task<byte[]> GenerateAsync(IAsyncDocumentSession session, User student)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (student == null) throw new ArgumentNullException(nameof(student));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Student");
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
            worksheet.Cells[1, 11].Value = "Age";
            worksheet.Cells[1, 12].Value = "Gender";

            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
            }

            var row = 2;
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
                worksheet.Cells[row, 11].Value = student.StudentDetails?.Age;
                worksheet.Cells[row, 12].Value = student.StudentDetails?.Gender;
            }
            worksheet.Cells[$"A1:J{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            worksheet = package.Workbook.Worksheets.Add("Programs");
            worksheet.Cells[1, 1].Value = "Number";
            worksheet.Cells[1, 2].Value = "When Added";
            worksheet.Cells[1, 3].Value = "Name";
            worksheet.Cells[1, 4].Value = "Program";
            worksheet.Cells[1, 5].Value = "AST";
            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
            }

            row = 1;
            foreach (var program in student.Programs)
            {
                row++;
                worksheet.Cells[row, 1].Value = program.Number;
                worksheet.Cells[row, 2].Value = program.WhenAdded;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mmm-yyyy";
                worksheet.Cells[row, 3].Value = program.Name;
                worksheet.Cells[row, 4].Value = program.Code;

                var parser = CodeParser.New(program.Code ?? string.Empty);
                var parseResult = await parser.ParseAsync().ConfigureAwait(false);
                worksheet.Cells[row, 5].Value = string.Join(
                    Environment.NewLine,
                    parseResult.Nodes.Select(n => n.ToString()));
            }
            worksheet.Cells[$"A1:D{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            return await package.GetAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
