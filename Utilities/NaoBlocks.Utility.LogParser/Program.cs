using NaoBlocks.Common;
using NaoBlocks.Parser;
using OfficeOpenXml;

namespace NaoBlocks.Utility.LogParser
{
    internal class Program
    {
        public async Task Run(string[] args)
        {
            WriteLine("Starting Log Parser");

            // Validate the arguments
            if (args.Length < 2)
            {
                WriteLine("Utility arguments are <input> <output>", ConsoleColor.Red);
                return;
            }
            var inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                WriteLine($"Input file {inputPath} does not exist", ConsoleColor.Red);
                return;
            }
            var outputPath = args[1];
            var outputFolder = Path.GetDirectoryName(outputPath)!;
            if (!Directory.Exists(outputFolder))
            {
                WriteLine($"Making output folder {outputFolder} does not exist", ConsoleColor.DarkYellow);
                Directory.CreateDirectory(outputFolder);
            }

            using var outputPackage = new ExcelPackage();
            var outputWorksheet = outputPackage.Workbook.Worksheets.Add("Programs");
            outputWorksheet.Cells[1, 1].Value = "Person";
            outputWorksheet.Cells[1, 2].Value = "Number";
            outputWorksheet.Cells[1, 3].Value = "Name";
            outputWorksheet.Cells[1, 4].Value = "When Added";
            outputWorksheet.Cells[1, 5].Value = "Code";

            WriteLine($"Loading data from {inputPath}");
            using var inputPackage = new ExcelPackage();
            await inputPackage.LoadAsync(inputPath);
            var inputWorksheet = inputPackage.Workbook.Worksheets.First();
            var count = 0;
            var displayOptions = new AstNode.DisplayOptions
            {
                Children = AstNode.DisplayType.Ignore,
                IncludeSourceIDs = false,
                IncludeNodeTypes = false,
                IncludeTokenTypes = false
            };
            for (var row = 2; row <= inputWorksheet.Dimension.End.Row; row++)
            {
                inputWorksheet.Cells[row, 1, row, 5].Copy(outputWorksheet.Cells[row, 1]);
                WriteLine($"-> Compiling code in row {row}");

                var parser = CodeParser.New(inputWorksheet.Cells[row, 5].Value.ToString() ?? string.Empty);
                var result = await parser.ParseAsync();
                if (result.Errors.Any())
                {
                    WriteLine($"-> Unable to compile: {result.Errors.First()}");
                    continue;
                }

                WriteNodes(outputWorksheet, row, 6, result.Nodes[1], displayOptions);
                count++;
            }

            WriteLine($"-> Processed {count} rows");
            WriteLine($"Saving to {outputPath}");
            outputWorksheet.Cells[outputWorksheet.Dimension.Address].AutoFitColumns();
            await outputPackage.SaveAsAsync(outputPath);
        }

        private static async Task Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var app = new Program();
            try
            {
                await app.Run(args);
            }
            catch (Exception ex)
            {
                WriteLine($"An expected error has occurred: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void WriteLine(string message = "", ConsoleColor colour = ConsoleColor.Gray)
        {
            var currentColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColour;
        }

        private int WriteNodes(ExcelWorksheet outputWorksheet, int row, int start, AstNode node, AstNode.DisplayOptions displayOptions)
        {
            foreach (var child in node.Children)
            {
                outputWorksheet.Cells[row, start++].Value = child.ToString(displayOptions);
                start = WriteNodes(outputWorksheet, row, start, child, displayOptions);
            }

            return start;
        }
    }
}