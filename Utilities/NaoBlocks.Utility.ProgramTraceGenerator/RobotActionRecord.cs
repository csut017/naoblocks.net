using System.Diagnostics;

namespace NaoBlocks.Utility.ProgramTraceGenerator
{
    [DebuggerDisplay("{RowNumber}:{Type}->{LogTime} [{Action}]")]
    public class RobotActionRecord
    {
        public RobotActionRecord(int rowNumber, string type, string? action, string? mode, DateTime logTime, DateTime? robotTime)
        {
            this.RowNumber = rowNumber;
            this.Type = type;
            this.Action = action;
            this.Mode = mode;
            this.RobotTime = robotTime;
            this.LogTime = logTime;
        }

        public string? Action { get; set; }

        public DateTime LogTime { get; set; }

        public string? Mode { get; set; }

        public DateTime? RobotTime { get; set; }

        public int RowNumber { get; }

        public string Type { get; set; }
    }
}