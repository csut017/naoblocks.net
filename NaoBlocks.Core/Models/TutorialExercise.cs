using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class TutorialExercise
    {
        public IList<TutorialExerciseLine> Lines { get; } = new List<TutorialExerciseLine>();

        public string Name { get; set; } = string.Empty;

        public int? Order { get; set; }

        public string? Title { get; set; } = string.Empty;
    }
}