namespace NaoBlocks.Utility.ProgramTraceGenerator
{
    public class RobotActionRecord
    {
        public RobotActionRecord(string type, string? action, string? mode, DateTime logTime, DateTime robotTime)
        {
            this.Type = type;
            this.Action = action;
            this.Mode = mode;
            this.RobotTime = robotTime;
            this.LogTime = logTime;
        }

        public string? Action { get; set; }

        public DateTime LogTime { get; set; }

        public string? Mode { get; set; }

        public DateTime RobotTime { get; set; }

        public string Type { get; set; }
    }
}