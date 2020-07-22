using Newtonsoft.Json;

namespace NaoBlocks.Core.Models
{
    public class UserSettings
    {
        public int AllocationMode { get; set; }

        public bool Conditionals { get; set; }

        public int? CurrentExercise { get; set; }

        public string? CurrentTutorial { get; set; }

        public bool Dances { get; set; }

        public bool Events { get; set; }

        public bool Loops { get; set; }

        public string? RobotType { get; set; }
        
        public string? RobotTypeId { get; set; }

        public string? RobotId { get; set; }

        public bool Sensors { get; set; }

        public bool Simple { get; set; }

        public bool Tutorials { get; set; }

        public bool Variables { get; set; }
    }
}