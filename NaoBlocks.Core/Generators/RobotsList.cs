using NaoBlocks.Core.Models;
using OfficeOpenXml;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Generators
{
    public static class RobotsList
    {
        public static async Task<byte[]> GenerateAsync(IAsyncDocumentSession session)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Robots");

            worksheet.Cells[1, 1].Value = "Machine Name";
            worksheet.Cells[1, 2].Value = "Friendly Name";
            worksheet.Cells[1, 3].Value = "Type";
            worksheet.Cells[1, 4].Value = "When Added";
            worksheet.Cells[1, 5].Value = "Initialized";

            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
            }

            var robots = await session.Query<Robot>()
                .Include(r => r.RobotTypeId)
                .ToListAsync()
                .ConfigureAwait(false);
            var row = 1;
            foreach (var robot in robots)
            {
                var type = await session.LoadAsync<RobotType>(robot.RobotTypeId).ConfigureAwait(false);
                row++;
                worksheet.Cells[row, 1].Value = robot.MachineName;
                worksheet.Cells[row, 2].Value = robot.FriendlyName;
                worksheet.Cells[row, 3].Value = type.Name;
                worksheet.Cells[row, 4].Value = robot.WhenAdded;
                worksheet.Cells[row, 5].Value = robot.IsInitialised;

                worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-mmm-yyyy";
            }

            worksheet.Cells[$"A1:E{row}"].AutoFilter = true;
            worksheet.Cells.AutoFitColumns(0);

            return await package.GetAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
