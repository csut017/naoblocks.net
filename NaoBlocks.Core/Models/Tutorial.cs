using System;
using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class Tutorial
    {
        public string Category { get; set; } = string.Empty;

        public IList<TutorialExercise> Exercises { get; } = new List<TutorialExercise>();

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;
    }
}