using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class TutorialExercise
    {
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Lines may be null")]
        public IList<TutorialExerciseLine>? Lines { get; set; }

        public string? Name { get; set; } = string.Empty;

        public int? Order { get; set; }

        public string? Title { get; set; } = string.Empty;

        public static Data.TutorialExercise FromDto(TutorialExercise value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var exercise = new Data.TutorialExercise
            {
                Name = value.Name ?? string.Empty,
                Order = value.Order,
                Title = value.Title
            };
            if (value.Lines != null)
            {
                foreach (var line in value.Lines)
                {
                    exercise.Lines.Add(new Data.TutorialExerciseLine
                    {
                        Message = line.Message ?? string.Empty,
                        Order = line.Order ?? 0
                    });
                }
            }
            return exercise;
        }

        public static TutorialExercise FromModel(Data.TutorialExercise value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var exercise = new TutorialExercise
            {
                Name = value.Name,
                Order = value.Order,
                Title = value.Title,
                Lines = value.Lines.Select(l => new TutorialExerciseLine
                {
                    Message = l.Message,
                    Order = l.Order
                }).ToList()
            };

            return exercise;
        }
    }
}