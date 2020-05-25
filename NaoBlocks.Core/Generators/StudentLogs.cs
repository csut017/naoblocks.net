using NaoBlocks.Core.Models;
using OfficeOpenXml;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;

namespace NaoBlocks.Core.Generators
{
    public static class StudentLogs
    {
        public static async Task<byte[]> GenerateAsync(IAsyncDocumentSession session, User student)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (student == null) throw new ArgumentNullException(nameof(student));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var data = await session.Query<RobotLog>()
                .Include(rl => rl.RobotId)
                .Where(rl => rl.Conversation.UserId == student.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var worksheet = package.Workbook.Worksheets.Add("Logs");
            worksheet.Cells[1, 1].Value = "Robot";
            worksheet.Cells[1, 2].Value = "Date";
            worksheet.Cells[1, 3].Value = "Conversation";
            worksheet.Cells[1, 4].Value = "Time";
            worksheet.Cells[1, 5].Value = "Type";
            worksheet.Cells[1, 6].Value = "Description";
            var columns = new Dictionary<string, int>();
            var nextColumn = 6;

            var row = 1;
            foreach (var log in data)
            {
                var robot = await session.LoadAsync<Robot>(log.RobotId).ConfigureAwait(false);
                foreach (var line in log.Lines) {
                    ++row;
                    worksheet.Cells[row, 1].Value = robot.FriendlyName;
                    worksheet.Cells[row, 2].Value = log.WhenAdded;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-mmm-yyyy";
                    worksheet.Cells[row, 3].Value = log.Conversation.ConversationId;
                    worksheet.Cells[row, 4].Value = line.WhenAdded;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "h:mm:ss AM/PM";
                    worksheet.Cells[row, 5].Value = line.SourceMessageType.ToString();
                    worksheet.Cells[row, 6].Value = line.Description;
                    foreach (var value in line.Values)
                    {
                        if (!columns.TryGetValue(value.Name, out int column))
                        {
                            column = ++nextColumn;
                            columns[value.Name] = column;
                            worksheet.Cells[1, column].Value = value.Name;
                        }
                        worksheet.Cells[row, column].Value = value.Value;
                    }
                }
            }

            using (var range = worksheet.Cells[1, 1, 1, nextColumn])
            {
                range.Style.Font.Bold = true;
            }
            var columnLetter = nextColumn.ToExcelColumn();
            worksheet.Cells[$"A1:{columnLetter}{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            worksheet = package.Workbook.Worksheets.Add("Programs");
            worksheet.Cells[1, 1].Value = "Number";
            worksheet.Cells[1, 2].Value = "When Added";
            worksheet.Cells[1, 3].Value = "Name";
            worksheet.Cells[1, 4].Value = "Program";
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
            }
            worksheet.Cells[$"A1:D{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            return await package.GetAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
