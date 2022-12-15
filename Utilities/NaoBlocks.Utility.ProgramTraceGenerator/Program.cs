using OfficeOpenXml;

namespace NaoBlocks.Utility.ProgramTraceGenerator
{
    internal class Program
    {
        public async Task<int> Run(string[] args)
        {
            WriteLine("Starting Log Parser");

            // Validate the arguments
            if (args.Length < 3)
            {
                WriteLine("Utility arguments are <input> <output> <type>", ConsoleColor.Red);
                return -1;
            }
            var inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                WriteLine($"Input file {inputPath} does not exist", ConsoleColor.Red);
                return -1;
            }
            var outputPath = args[1];
            var outputFolder = Path.GetDirectoryName(outputPath)!;
            if (!Directory.Exists(outputFolder))
            {
                WriteLine($"Making output folder {outputFolder} does not exist", ConsoleColor.DarkYellow);
                Directory.CreateDirectory(outputFolder);
            }

            var robotTypeToProcess = args[2];

            // Load the input worksheet
            WriteLine($"Loading data from {inputPath}");
            using var inputPackage = new ExcelPackage();
            await inputPackage.LoadAsync(inputPath);
            var inputWorksheet = inputPackage.Workbook.Worksheets.First();
            var count = 0;
            var columns = new Dictionary<string, int>();
            for (var column = 1; column < inputWorksheet.Dimension.End.Column; column++)
            {
                var name = inputWorksheet.Cells[1, column].Value.ToString() ?? string.Empty;
                columns[name] = column;
            }

            // Validate we have the correct columns
            var isValid = true;
            if (columns.TryGetValue("Robot Type", out var robotTypeColumn))
            {
                WriteLine($"-> 'Robot Type' is in column {robotTypeColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Robot Type' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("Robot", out var robotColumn))
            {
                WriteLine($"-> 'Robot' is in column {robotColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Robot' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("Date", out var dateColumn))
            {
                WriteLine($"-> 'Date' is in column {dateColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Date' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("Type", out var typeColumn))
            {
                WriteLine($"-> 'Type' is in column {typeColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Type' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("RobotTime", out var robotTimeColumn))
            {
                WriteLine($"-> 'RobotTime' is in column {robotTimeColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'RobotTime' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("Action", out var actionColumn))
            {
                WriteLine($"-> 'Action' is in column {actionColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Action' column", ConsoleColor.Red);
                isValid = false;
            }
            if (columns.TryGetValue("Mode", out var modeColumn))
            {
                WriteLine($"-> 'Mode' is in column {modeColumn}");
            }
            else
            {
                WriteLine("-> Could not find 'Mode' column", ConsoleColor.Red);
                isValid = false;
            }

            // Exit if the data is invalid
            if (!isValid)
            {
                WriteLine("One or more columns are missing - stopping execution");
                return -2;
            }

            WriteLine("Processing input file");
            var row = 2;
            var records = new Dictionary<string, List<RobotActionRecord>>();
            for (; row <= inputWorksheet.Dimension.End.Row; row++)
            {
                WriteProgressIndicator(row);
                var robotType = inputWorksheet.Cells[row, robotTypeColumn].Value;
                if (!robotTypeToProcess.Equals(robotType)) continue;

                var robot = inputWorksheet.Cells[row, robotColumn]?.Value?.ToString();
                if (string.IsNullOrEmpty(robot)) continue;

                var timeText = inputWorksheet.Cells[row, robotTimeColumn]?.Value?.ToString();
                if (!DateTime.TryParse(timeText, out var robotTime)) continue;

                var dateValue = inputWorksheet.Cells[row, dateColumn]?.Value as double?;
                if (dateValue == null) continue;
                var logTime = DateTime.FromOADate(dateValue.Value);

                var type = inputWorksheet.Cells[row, typeColumn]?.Value?.ToString();
                if (string.IsNullOrEmpty(type)) continue;

                var action = inputWorksheet.Cells[row, actionColumn]?.Value?.ToString();
                var mode = inputWorksheet.Cells[row, modeColumn]?.Value?.ToString();

                if (!records.TryGetValue(robot, out var robotRecords))
                {
                    robotRecords = new();
                    records[robot] = robotRecords;
                }

                robotRecords.Add(new(type, action, mode, (DateTime)logTime, robotTime));
                count++;
            }
            WriteLine();
            WriteLine($"Processed {row - 1:#,##0} rows, found {count:#,##0} records");

            // Generate the traces in an output file
            WriteLine("Generating output file");
            using var outputPackage = new ExcelPackage();
            count = 1;
            foreach (var robot in records)
            {
                var outputWorksheet = outputPackage.Workbook.Worksheets.Add(robot.Key);
                outputWorksheet.Cells[1, 1].Value = "Time";
                outputWorksheet.Cells[1, 2].Value = "Action";
                outputWorksheet.Cells[1, 3].Value = "Mode";
                outputWorksheet.Cells[1, 4].Value = "Duration (s)";
                row = 2;
                DateTime lastTime = DateTime.MinValue;
                var mode = "Continuous";
                foreach (var record in robot.Value.OrderBy(r => r.LogTime).ThenBy(r => r.RobotTime))
                {
                    WriteProgressIndicator(count);
                    switch (record.Type)
                    {
                        case "RobotAction":
                            var timeCell = outputWorksheet.Cells[row, 1];
                            timeCell.Value = record.RobotTime;
                            timeCell.Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss AM/PM";
                            outputWorksheet.Cells[row, 2].Value = record.Action;
                            outputWorksheet.Cells[row, 3].Value = mode;
                            if (lastTime > DateTime.MinValue)
                            {
                                var duration = record.RobotTime - lastTime;
                                var durationCell = outputWorksheet.Cells[row - 1, 4];
                                durationCell.Value = duration.TotalSeconds;
                                durationCell.Style.Numberformat.Format = "#,###0.00";
                            }

                            lastTime = record.RobotTime;
                            row++;
                            break;

                        case "RobotStateUpdate":
                            mode = record.Mode;
                            lastTime = DateTime.MinValue;
                            break;

                        case "StartProgram":
                            lastTime = DateTime.MinValue;
                            break;
                    }

                    count++;
                }
                outputWorksheet.Cells[outputWorksheet.Dimension.Address].AutoFitColumns();
            }
            WriteLine();
            WriteLine($"Processed {count - 1:#,##0} records for {records.Count} robots");

            // Save the output worksheet
            WriteLine($"Saving to {outputPath}");
            await outputPackage.SaveAsAsync(outputPath);

            return 0;
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

        private static void WriteProgressIndicator(int row)
        {
            if (row % 10 == 0)
            {
                if (row % 100 == 0)
                {
                    var number = row / 100;
                    Console.Write(number);
                }
                else
                {
                    Console.Write(".");
                }
            }
        }
    }
}