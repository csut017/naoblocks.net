using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class Tutorial
    {
        public string? Category { get; set; }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Exercises can be null")]
        public IList<TutorialExercise>? Exercises { get; set; }

        public string? Name { get; set; }

        public int? Order { get; set; }

        public DateTime? WhenAdded { get; set; }

        public static Tutorial FromModel(Data.Tutorial value, bool includeAllDetails = true)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var tutorial = new Tutorial
            {
                Category = value.Category,
                Name = value.Name,
                Order = value.Order
            };

            if (includeAllDetails)
            {
                tutorial.WhenAdded = value.WhenAdded;
                tutorial.Exercises = value.Exercises.Select(TutorialExercise.FromModel).ToList();
            }

            return tutorial;
        }
    }
}