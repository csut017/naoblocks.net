using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class AddTutorialCommand
        : CommandBase<Tutorial>
    {
        public string? Category { get; set; }

        public IList<TutorialExercise> Exercises { get; } = new List<TutorialExercise>();

        public string? Name { get; set; }

        public int? Order { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.Error($"Name is required for a tutorial"));
            }

            if (string.IsNullOrWhiteSpace(this.Category))
            {
                errors.Add(this.Error($"Category is required for a tutorial"));
            }

            if (this.Exercises.Count == 0)
            {
                errors.Add(this.Error($"At least one exercise is required for a tutorial"));
            }
            else
            {
                var pos = 0;
                foreach (var exercise in this.Exercises)
                {
                    ++pos;
                    if (string.IsNullOrWhiteSpace(exercise.Name))
                    {
                        errors.Add(this.Error($"Name is missing for exercise #{pos}"));
                    }

                    if (exercise.Lines.Count == 0)
                    {
                        errors.Add(this.Error($"Exercise #{pos} does not have any lines"));
                    }
                    else if (!exercise.Lines.Any(l => !string.IsNullOrWhiteSpace(l.Message)))
                    {
                        errors.Add(this.Error($"Exercise #{pos} does not have any non-blank lines"));
                    }
                }
            }

            if (!errors.Any() && await session.Query<Tutorial>().AnyAsync(t => t.Name == this.Name && t.Category == this.Category).ConfigureAwait(false))
            {
                errors.Add(this.Error($"Tutorial with name '{this.Name}' already exists in '{this.Category}'"));
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            if (!this.Order.HasValue)
            {
                var lastTutorial = await session.Query<Tutorial>()
                    .Where(t => t.Category == this.Category)
                    .OrderByDescending(t => t.Order)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                this.Order = lastTutorial == null ? 1 : lastTutorial.Order + 1;
            }

            var tutorial = new Tutorial
            {
                Category = this.Category ?? "<Unknown>",
                Name = this.Name ?? "<Unknown>",
                Order = this.Order ?? 0,
                WhenAdded = this.WhenExecuted
            };

            var exPos = 0;
            foreach (var exercise in this.Exercises)
            {
                var newExercise = new TutorialExercise
                {
                    Name = exercise.Name,
                    Order = ++exPos,
                    Title = exercise.Title
                };
                var linePos = 0;
                foreach (var line in exercise.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Message)))
                {
                    line.Order = ++linePos;
                    newExercise.Lines.Add(line);
                }
                tutorial.Exercises.Add(newExercise);
            }

            await session.StoreAsync(tutorial).ConfigureAwait(false);
            return this.Result(tutorial);
        }
    }
}