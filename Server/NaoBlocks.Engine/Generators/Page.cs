using OfficeOpenXml;

namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Defines a page in a report.
    /// </summary>
    public class Page
        : ReportItem
    {
        /// <summary>
        /// Gets the paragraphs in the page.
        /// </summary>
        public List<PageParagraph> Paragraphs { get; private set; } = new List<PageParagraph>();

        /// <summary>
        /// Adds a new <see cref="PageParagraph"/>.
        /// </summary>
        /// <returns>The new <see cref="PageParagraph"/>.</returns>
        public PageParagraph AddParagraph(params PageBlock[] blocks)
        {
            var paragraph = new PageParagraph();
            this.Paragraphs.Add(paragraph);
            paragraph.Blocks.AddRange(blocks);
            return paragraph;
        }

        /// <summary>
        /// Exports to CSV.
        /// </summary>
        /// <param name="writer">The <see cref="StreamWriter"/> to use.</param>
        /// <param name="separator">The characters to use as a cell seperator.</param>
        public override void ExportToCsv(StreamWriter writer, string separator)
        {
            foreach (var paragraph in this.Paragraphs)
            {
                foreach (var block in paragraph.Blocks)
                {
                    writer.Write(block.Contents);
                    writer.Write(separator);
                }

                writer.WriteLine();
            }
        }

        /// <summary>
        /// Exports to Excel.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to use.</param>
        public override void ExportToExcel(ExcelWorksheet worksheet)
        {
            var rowNum = 0;
            foreach (var paragraph in this.Paragraphs)
            {
                rowNum++;
                var cellNum = 0;
                foreach (var block in paragraph.Blocks)
                {
                    cellNum++;

                    var destCell = worksheet.Cells[rowNum, cellNum];
                    destCell.Value = block.Contents;
                    var style = destCell.Style;
                    if (block.IsEmphasized) style.Font.Bold = true;
                }
            }
        }
    }
}