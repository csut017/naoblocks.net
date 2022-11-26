using OfficeOpenXml;

namespace NaoBlocks.Engine.Tests.Commands
{
    internal static class ExcelPackageHelpers
    {
        public static ExcelWorksheet GenerateData(this ExcelWorksheet sheet, params string[][] data)
        {
            var startRow = sheet.Dimension?.End?.Row ?? 0;
            for (var rowNumber = 0; rowNumber < data.Length; rowNumber++)
            {
                var row = data[rowNumber];
                for (var columnNumber = 0; columnNumber < row.Length; columnNumber++)
                {
                    var value = row[columnNumber];
                    sheet.Cells[startRow + rowNumber + 1, columnNumber + 1].Value = value;
                }
            }

            return sheet;
        }
    }
}