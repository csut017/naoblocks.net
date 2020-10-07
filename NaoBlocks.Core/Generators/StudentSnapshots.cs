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
    public static class StudentSnapshots
    {
        public static async Task<byte[]> GenerateAsync(IAsyncDocumentSession session, User student)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (student == null) throw new ArgumentNullException(nameof(student));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var data = await session.Query<Snapshot>()
                .Where(s => s.UserId == student.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var worksheet = package.Workbook.Worksheets.Add("Snapshots");
            worksheet.Cells[1, 1].Value = "Date/Time";
            worksheet.Cells[1, 2].Value = "Source";
            worksheet.Cells[1, 3].Value = "State";
            var columns = new Dictionary<string, int>();
            var lastColumn = 3;

            var row = 1;
            foreach (var snapshot in data)
            {
                ++row;
                worksheet.Cells[row, 1].Value = snapshot.WhenAdded;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "dd-mmm-yyyy h:mm:ss AM/PM";
                worksheet.Cells[row, 2].Value = snapshot.Source;
                worksheet.Cells[row, 3].Value = snapshot.State;
                foreach (var value in snapshot.Values)
                {
                    if (!columns.TryGetValue(value.Name, out int column))
                    {
                        column = ++lastColumn;
                        columns[value.Name] = column;
                        worksheet.Cells[1, column].Value = value.Name;
                    }
                    worksheet.Cells[row, column].Value = value.Value;
                }
            }

            using (var range = worksheet.Cells[1, 1, 1, lastColumn])
            {
                range.Style.Font.Bold = true;
            }
            var columnLetter = lastColumn.ToExcelColumn();
            worksheet.Cells[$"A1:{columnLetter}{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            return await package.GetAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
